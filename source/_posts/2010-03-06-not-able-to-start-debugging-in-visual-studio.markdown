---
author: rahulpnath
comments: true
date: 2010-03-06 11:52:00+00:00
layout: post
slug: not-able-to-start-debugging-in-visual-studio
title: Not Able to “Start Debugging” in Visual Studio
wordpress_id: 14
categories:
- .Net
tags:
- Visual Studio
---

<img class ="left" alt="visual studio debug not working" src="{{ site.images_root}}/vs_debug_not_working.png" />
Quite a few days back,I faced a peculiar problem :).Visual Studio was not having the green play button(the one for Start Debugging) enabled.No way was I able to start debugging.  
Google gave many suggestions,none was of help.  
I soon found out that,in Startup Projects(From the menu Project -> Properties),the option multiple was selected and all the projects were set to an action None)(Setting this to Start Without debugging also creates this same problem) :)   
  
Yet to find out how this happened automatically but still setting up the startup project,made the green button glow :)  
  
Be sure to check this the next time if the green button doesn't glow :)
