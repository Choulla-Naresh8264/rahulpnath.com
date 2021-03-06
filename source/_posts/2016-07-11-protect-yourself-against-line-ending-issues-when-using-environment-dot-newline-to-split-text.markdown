---
layout: post
title: "Protect Yourself Against Line Ending Issues when Using Environment.Newline to Split Text"
comments: true
categories:
- .Net
- Programming
tags: 
date: 2016-07-11 05:45:31 
keywords: 
description: The invisible character that at times take a lot of your time!
primaryImage: newline_diff.png
---
>*In computing, a [newline](https://en.wikipedia.org/wiki/Newline), also known as a line ending, end of line (EOL), or line break, is a special character or sequence of characters signifying the end of a line of text and the start of a new line. The actual codes representing a newline vary across operating systems, which can be a problem when exchanging text files between systems with different newline representations.*

I was using a Resource (resx) file to store large text of comma separated values (CSV). This key-value mapping represented the mapping of product codes between an old and new system. In code, I split this whole text using [Environment.NewLine](https://msdn.microsoft.com/en-us/library/system.environment.newline(v=vs.110\).aspx) and then by comma to generate the map, as shown below.

``` csharp
AllMappings = Resources.UsageMap
    .Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
    .Select(s => s.Split(new[] { ',' }))
    .ToDictionary(item => item[0], item => item[1]);
```

It all worked fine on my machine and even on other team members machines. There was no reason to doubt this piece of code, until on the development environment we noticed the mapped value in the destination system always null. 

### Analyzing the Issue ###

Since in the destination system, all the other values were getting populated as expected, except for this mapping it was easy to narrow down to the class that returned the mapping value, to be the problematic one. Initially, I thought this was an issue with the resource file not getting bundled properly. I used [dotPeek](https://www.jetbrains.com/decompiler/) to decompile the application and verified that resource file was getting bundled properly and had exactly the same text (visually) as expected. 

<img src="{{site.images_root}}/newline_dotpeek.png" alt ="Resource file disassembled in dotPeek" />

I copied the resource file text from disassembled code in dotPeek into [Notepad2](http://www.flos-freeware.ch/notepad2.html) (configured to show the line endings) and everything started falling into place. The resource text file from the build generated code ended with LF (\n), while the one on our development machines had CRLF (\r\n). All machines, including the build machines are running Windows and the expected value for [Environemnt.Newline](https://msdn.microsoft.com/en-us/library/system.environment.newline(v=vs.110\).aspx) is CRLF - ** A string containing "\r\n" for non-Unix platforms, or a string containing "\n" for Unix platforms.**

<figure>
<img src="{{site.images_root}}/newline_diff.png" alt ="Difference between build generated and development machine resource file" />
<figcaption><em>Difference between build generated and development machine resource file</em></figcaption>
</figure>

### Finding the Root Cause ###

We use git for our source control and [configured to use 'auto' line endings](https://help.github.com/articles/dealing-with-line-endings/) at the repository level. This ensures that the source code, when checked out, matches the line ending format of the machine. We use [Bamboo](https://www.atlassian.com/software/bamboo) on our build servers running Windows. The checked out files on the build server had LF line endings, which in turn gets compiled into the assembly. 

The checkout step in Bamboo used the built in git plugin (JGit) and has certain limitations. It's recommended to use native git to use the full git features. JGit also has a known issue with [line endings on a Windows machine](https://jira.atlassian.com/plugins/servlet/mobile#issue/BAM-9591) and checks out a file with LF endings. So whenever the source code was checked out, it replaced all line endings in the file with LF before compilation. So the resource file ended up having LF line endings in the assembly, and the code could no longer find Environment.Newline (\r\n) to split.

### Possible Fixes ###

Two possible ways to fix this issue is   

- Switch to using native git on the bamboo build process
- Use LF to split the text and trim any excess characters. This reduces dependency on line endings variations and settings between different machines only until we are on a different machine which has a different format.

I chose to use LF to split the text and trim any additional characters, while also [updating Bamboo to use native git](https://confluence.atlassian.com/bamboo/defining-a-new-executable-capability-289277164.html) for checkout.

``` csharp
AllMappings = Resources.UsageMap
    .Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries)
    .Select(s => s.Split(new[] { ',' }))
    .ToDictionary(item => item[0].Trim().ToUpper(), item => item[1].Trim());
```

### Protecting Against Line Endings ###

The easiest and fastest way that this would have come to my notice was to have a unit test in place. This would ensure that the test fails on the build machine. A test like below will pass on my local but not on the build machine as UsageMap would not return any value for the destination system.

``` csharp
[Theory]
[InlineData("MovieWeek", "Weekly-Movie")]
[InlineData("Dell15", "Laptop-Group3")]
public void SutReturnsExpected(string sourceSystemCode, string expected)
{
    var sut = new UsageMap();
    var actual = sut.GetDestinationCode(sourceSystemCode);
    Assert.Equal(expected, actual);
}
```

Since there are different systems with different line endings and also applications with different line ending settings and issues of its own, there does not seem to be a 'one fix for all' cases. The best I can think of in these cases is it protect us with such unit tests. It fails fast and brings it immediately to out notice. Have you ever had to deal with an issue with line endings and found better ways to handle them?
