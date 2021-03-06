---
author: rahulpnath
comments: true
date: 2013-08-25 04:23:00+00:00
layout: post
slug: windows-phone-series-jump-lists
title: Windows Phone Series – Jump Lists
wordpress_id: 563
categories:
- Windows Phone
- WP7
tags:
- Windows Phone Series
---

One of the best things about the windows phone is that way you can navigate large lists of data.  Jumplist provides an easy and fast way to navigate large data, just like we do in the contacts hub or the apps collection. It would be great to have this behaviour even in the apps that we develop so that we can give a consistent experience to our application users.
Jumplist can be implemented using LongListSelector, which is part of the [Windows Phone toolkit](http://phone.codeplex.com/) for Windows phone 7. With Windows Phone 8 this is included along with the sdk. Implementing jump list with the longlistselector is easy enough, that you can get this into your application in a few minutes. Lets see how.
If you are already using a listbox then you should be changing it to a longlistselector or if you are already using that, then you might be using it in the “ungrouped mode”, by setting IsFlatList=”True”. Change this to False.

```xml
<toolkit:LongListSelector Name="allPersons" IsFlatList="False" >
</toolkit:LongListSelector>
```

We need to specify a few templates for the date to be rendered to our needs as shown below. In this example we would be looking at a contacts sample.

![windows_phone_jumplist_items]({{ site.images_root}}/wp_jumplist_items.png)![windows_phone_jumplist_groups]({{ site.images_root}}/wp_jumplist_groups.png)

**ItemTemplate**

The itemtemplate specifies how the bound list of items should be displayed. This would be the same as that you have been using earlier for your listbox.

```xml
<toolkit:LongListSelector.ItemTemplate>
    <DataTemplate>
        <TextBlock Text="{Binding Name}" FontSize="30" />
    </DataTemplate>
</toolkit:LongListSelector.ItemTemplate>
```

**GroupHeaderTemplate**

The groupheadertemplate specifies the template for each header of the group.

```xml
 <toolkit:LongListSelector.GroupHeaderTemplate>
    <DataTemplate>
       <Border Background="Red" HorizontalAlignment="Left" Width="50" Height="50">
         <TextBlock Text="{Binding Title}" FontSize="30" HorizontalAlignment="Center"/>
       </Border>
    </DataTemplate>
</toolkit:LongListSelector.GroupHeaderTemplate>
```

**GroupItemTemplate**

The groupitemtemplate specifies the template for the headers, when in group view mode. This is the display that would be presented when we are to choose a group to navigate to.

```xml
 <toolkit:LongListSelector.GroupItemTemplate>
    <DataTemplate>
        <Button IsEnabled="{Binding HasData}" BorderThickness="0" Background="Transparent">
            <Border Background="Red" BorderThickness="0" Width="60" Height="60">
                <TextBlock VerticalAlignment="Center" HorizontalAlignment="Center" FontSize="38" Margin="4" Text="{Binding Title}" />
            </Border>
        </Button>
    </DataTemplate>
</toolkit:LongListSelector.GroupItemTemplate>
```

**GroupItemsPanel**

The groupitemspanel specifies the panel to be used to display the groupitems. If we are using only alphabets as the group headers, then we would want to wrap each of these items. If our group items are long enough then it would be better to leave it as default which would be a stackpanel.

```xml
 <toolkit:LongListSelector.GroupItemsPanel>
    <ItemsPanelTemplate>
        <toolkit:WrapPanel Margin="5" Background="Black" />
    </ItemsPanelTemplate>
</toolkit:LongListSelector.GroupItemsPanel>
```

We need to group the data that gets bound to the list. For a normal listbox you would either bind a List or an ObservableCollection. In this we would need to group the data that gets bound to the control as groups that we want to display. In this example our data is a list of Persons, and we would want to group the data with their starting character. If the starting character is an alphabet, then we would display that person under that character. For any non-alphabet we would group it under ‘#’. The GroupedPersons in our DataRepository would return the data for us.

```csharp
 public static IEnumerable<Group<Person>> GroupedPersons
{
    get
    {
        List<Group<Person>> groupedArticles = new List<Group<Person>>();
        char[] az = Enumerable.Range('a', 26).Select(a => (char)a).ToArray();

        foreach (char letter in az)
        {
            Group<Person> groupedLetter = new Group<Person>()
            {
                Title = letter.ToString(),
                Items = Persons.Where(a => a.Name.StartsWith(letter.ToString(), StringComparison.CurrentCultureIgnoreCase)).Where(a => a != null).ToList()
            };
            groupedLetter.HasData = groupedLetter.Items.Any();
            groupedArticles.Add(groupedLetter);
        }

        // Articles that start with a number should be added to # tag
        var list = groupedArticles.SelectMany(a => a.Items).ToList();
        var nonAlphabetIssues = Persons.Except(list);
        groupedArticles.Insert(
            0,
            new Group<Person>()
            {
                Items = nonAlphabetIssues.ToList(),
                Title = "#",
                HasData = nonAlphabetIssues.Any()
            });

        return groupedArticles;
    }
}
```

To get all the characters from A-Z we use the below code, after which it is some simple logics, that would is self explanatory

```csharp
 char[] az = Enumerable.Range('a', 26).Select(a => (char)a).ToArray();
 ```

This should now have made your flat list into a easily navigable jump list, that your users would love to use and make it easier for them to use you application.

You can find the code for this sample [here](https://github.com/rahulpnath/JumpList)
