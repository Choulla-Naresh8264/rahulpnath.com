---
author: rahulpnath
comments: true
date: 2010-06-30 16:39:00+00:00
layout: post
slug: role-based-access-control
title: Role Based Access Control
wordpress_id: 518
categories:
- WCF
tags:
- WCF
- WPF
---

RBAC(Role Based Access Control) is something that is very common in the day-to-day world.  
So what is this all about.It is just about a authorization check on whether you have the access to a particular resource or not.  
When faced with scenarios like this when developing applications, where you have to implement Role based access for the different users that are to use the system you might be confused on how to implement this.  
Say you have a WCF service exposing a set of services.You have a WPF thick client consuming this service.Say for example you are exposing a service to Add/Delete/View Employees.Based on the various roles you need to allow/disallow the access to the functionality.The easiest way would be enable/disable the controls that would be used invoke the corresponding functionality,based on the user role.  
So am I done?  
What if tomorrow you are exposing this service to some other client of yours,who is to develop his on User Interface(UI) for the service.  
Do I have a problem here?  
Yes of course!!!  
What if he does not make the same check on the UI to enable/disable the controls that would act as his inputs.So here exactly is where you have a access break.Any user will be able to perform all functions irrespective of the access specified for him.  
So how do I go about?  
Make this check at the service level itself.Check for access and throw a NoAccess exception if not authorized.What exactly happens when you try to enter a no-access area in your office :)  
UI synchronization is an added level to this,so that you can stop unnecessary service calls.  
  
Will soon post a implementation sample :)
