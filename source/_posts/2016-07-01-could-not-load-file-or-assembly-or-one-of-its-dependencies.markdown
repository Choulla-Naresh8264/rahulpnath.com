---
layout: post
title: "Web Application Occasionally Throwing 'Could not Load File or Assembly or one of its Dependencies' Exception"
comments: true
categories: 
- Tools
- .Net
tags: 
date: 2016-07-01 00:45:05 
keywords: 
description: 
primaryImage: doubleWrite_dll_conflict.jpg
---

We were facing a strange 'could not load DLL issue', when building and running multiple host projects in Visual Studio (VS 2015), side by side. We had 2 host projects - an NServiceBus worker role project (a console application) and a Web application and a few other projects, a couple of which are shared between both the host projects. It often happened in our team, when running the IIS-hosted Web application, it threw the error :

 <span style='color:red'>*Could not load file or assembly 'Newtonsoft.Json' or one of its dependencies. The located assembly's manifest definition does not match the assembly reference. *</span>.

The bin folder of the Web application did have a Newtonsoft.Json DLL, but of a different version of it than what was specified in the packages.config/csproj file. On a rebuild, the correct DLL version gets placed into the bin folder and everything works fine. Though the exception was observed by most of the team members, it did not happen always, which was surprising

> *Knowing what exactly caused the issue, I created a sample project to demonstrate it for this blog post. All screenshots and code samples are of the sample application.*

### Using AsmSpy to find conflicting assemblies ###
[AsmSpy](https://github.com/mikehadlow/AsmSpy) is a command-line tool to view conflicting assembly references in a given folder. This is helpful to find the different assemblies that refer to different versions of the same assembly in the given folder. Using AsmSpy, on the bin folder of the web application, it showed the conflicting  Newtonsoft.Json DLL references by different projects in the solution. There were three different versions of Newtonsoft Nuget package referred in the whole solution. The web project referred to an older version than the shared project and the worker project.

``` text
asmspy WebApplication1\bin\ nonsystem

Detailing only conflicting assembly references.
Reference: Newtonsoft.Json
   7.0.0.0 by SharedLibrary
   6.0.0.0 by WebApplication1
   4.5.0.0 by WebGrease
```   

The assembly binding redirects for both the host projects were correct and using the version of the package that it referred to in the packages.config and project (csproj) file.

### Using MsBuild Structured Log to find conflicting writes ###
Using the [Msbuild Structured Log Viewer](https://github.com/KirillOsenkov/MSBuildStructuredLog) to analyze what was happening with the build, I noticed the below '*DoubleWrites*' happening with Newtonsoft DLL. The double writes list shows all the folders from where the DLL was getting written into the bin folder of the project getting building. In the MSBuild Structured log viewer, a DLL pops up only when there are more than one places from where a DLL is getting written, hence the name '*Double writes*. This is a problem as there is a possibility of one write overriding other, depending on the order of writes, causing DLL version conflicts (which is exactly what's happening here).

<img src="{{site.images_root}}/doubleWrite_msbuildLogViewer.png" alt="Double Write Dll conflict" />

But in this specific case, the log captured above does not show the full problem but hints us of a potential problem. The build capture when building the whole solution (sln) shows that there are 2 writes happening from 2 different Newtonsoft package folders, which shows a potential conflict (*as shown above*). This does not explain the specific error we are facing with the Web application. Running the tool on just the Web application project (csproj), it does not show any DoubleWrites (*as shown below*). 

<img class="left" src="{{site.images_root}}/doubleWrite_proj_msbuildLogViewer.png" alt="Double Write Dll conflict" />

This confirms that there is something happening with the Web application bin outputs when we build the worker/shared dependency project.

### Building Web application in Visual Studio ###

When building a solution with a Web application project in Visual Studio (VS), I noticed that VS copies all the files from the bin folder of referred projects into the bin of the Web application project. This happens even if you build the shared project alone, as VS notices a change in the files of a dependent project and copies it over. So in this particular case, every time we build the dependent shared project or the worker project (which in turn triggers a build on the shared project), it ended up changing the files in shared projects bin folder, triggering VS to copy it over to the Web application's bin folder. This auto copy happens only for the Web application project and not for the Console/WPF project. ([Yet to find](https://twitter.com/rahulpnath/status/745841691979022336) what causes this auto copy on VS build)

<figure>
    <img src="{{site.images_root}}/doubleWrite_dll_conflict.jpg" alt="Double Write Dll conflict" />
    <figcaption><em>Bin folder of Web application and Console application after building Shared project</em></figcaption>
</figure>     

Since ***[CopyLocal](https://msdn.microsoft.com/en-us/library/aa984582(v=vs.71\).aspx)***, by default was true for the shared project, Newtonsoft DLLs were also getting copied into the shared project bin and in turn into Web applications bin (by VS). Since the Web application did not build during the above rebuild, it now has a conflicting DLL version of Newtonsoft in its bin folder, that does not match the assembly version it depends on, hence throws the exception, the next time I load the Web application from IIS.

I confirmed with other team members on the repro steps for this issue   

- Get the latest code and do a full rebuild from VS      
- Launch Web app works fine   
- Rebuild just one of the dependent projects that have Newtonsoft DLL dependency (which has CopyLocal set to true)    
- Launch Web app throws the error!    

It was a consistent repro with the above steps.

To fix the issue, I can choose either to update the Newtonsoft Package version across all the projects in the solution, or set CopyLocal to false, to prevent the DLL getting copied into the bin folder of the shared project and end up copied to Web application bin. ***I chose to set CopyLocal to false in this specific case.***

### The Sample Application ###

Now that we know what exactly causes the issue, it is easy to create a [sample application](https://github.com/rahulpnath/Blog/tree/master/DoubleWrites) to reproduce this issue.

- Create a Web application project and add NuGet package reference to older version of Newtonsoft 
- Create a console application/WPF application with a newer version of Newtonsoft Package.
- Create a shared library project with a newer version of Newtonsoft Nuget package. Add this shared project as  a project reference to both Web application and console/WPF application.

``` powershell 
Install-Package Newtonsoft.Json -ProjectName WebApplication1 -Version 6.0.1
Install-Package Newtonsoft.Json -ProjectName SharedLibrary -Version 7.0.1
Install-Package Newtonsoft.Json -ProjectName WpfApplication1 -Version 8.0.3
```

Follow the build repro steps above to reproduce the error. Change CopyLocal or update NuGet references and see issue gets resolved. 

Hope this helps in case you come across a similar issue!

 
