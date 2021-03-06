---
author: rahulpnath
comments: true
date: 2014-01-12 10:05:08+00:00
layout: post
slug: windows-phone-series-using-ucwa-to-connect-to-lync-server
title: Windows Phone Series – Using UCWA to connect to Lync Server
wordpress_id: 822
categories:
- Windows Phone
tags:
- communicator
- lync
- ucwa
- Windows Phone 8
- Windows Phone Series
---

One of the main things that makes enterprise or intranet applications more lively and connected with your business contacts, is to integrate with [Lync](http://office.microsoft.com/en-in/lync/) (formerly Microsoft Office Communicator) and provide options to interact with them. UCWA(Unified Communications Web Api) is a REST API that exposes Lync Server and its presence capabilities, which can be used to enhance your application capabilities. This can also be used to integrate with Windows phone applications as well and will see how to get started with it here.

To connect to a lync server the only details that we would want is the full email address and the password – thanks to [AutoDiscover](http://msdn.microsoft.com/en-us/library/office/jj900169(v=exchg.150).aspx) for exchange, which makes this possible. AutoDiscover is the way to find the users home server so that we can connect to it. The url for auto discover, otherwise called the [Root Url](https://ucwa.lync.com/documentation/GettingStarted-RootURL) can take different forms. In this sample I have assumed that it would take the below form, primarily because it works with the test domain that I was using (microsoft.com).

``` csharp
private const string autoDiscoverUrl = "https://lyncdiscover.{0}";
```

Creating a UCWA application is the starting point for every app that needs to work with UCWA.  The following steps indicated in the below diagram are to be followed to create an application.
[![HTTP call flow prior to creating an application in UCWA]({{ site.images_root}}/ucwa_createapp.png)](https://ucwa.lync.com/documentation/KeyTasks-CreateApplication)

Issuing a get request to ‘[https://lyncdiscover.microsoft.com/](https://lyncdiscover.microsoft.com/)’, will give the details of the home server that we need to connect to.

```xml 
<resource xmlns="http://schemas.microsoft.com/rtc/2012/03/ucwa" rel="root" href="https://lync32.lyncweb.microsoft.com/Autodiscover/AutodiscoverService.svc/root?originalDomain=microsoft.com">
    <link rel="user" href="https://lync32.lyncweb.microsoft.com/Autodiscover/AutodiscoverService.svc/root/oauth/user?originalDomain=microsoft.com"/>
    <link rel="xframe" href="https://lync32.lyncweb.microsoft.com/Autodiscover/XFrame/XFrame.html"/>
</resource>
```

We now need to authenticate the user with the home server.for which we need the oauth url. This can be obtained by issuing a dummy get request to the _user _url obtained above. This request will fail with an unauthorized access but also returns the url ([https://lync32.lyncweb.microsoft.com/WebTicket/oauthtoken](https://lync32.lyncweb.microsoft.com/WebTicket/oauthtoken)) from which the token needs to be obtained

```
HTTP/1.1 401 Unauthorized
Cache-Control: no-cache
Content-Type: text/html
Server: Microsoft-IIS/7.5
WWW-Authenticate: Bearer trusted_issuers="00000002-0000-0ff1-ce00-000000000000@3bdbdd27-2373-4baf-9469-4b10e76564c6,00000001-0001-0000-c000-000000000000@f686d426-8d16-42db-81b7-ab578e110ccd,00000001-0000-0000-c000-000000000000@72f988bf-86f1-41af-91ab-2d7cd011db47", client_id="00000004-0000-0ff1-ce00-000000000000"
WWW-Authenticate: MsRtcOAuth href="https://lync32.lyncweb.microsoft.com/WebTicket/oauthtoken",grant_type="urn:microsoft.rtc:windows,urn:microsoft.rtc:passive,urn:microsoft.rtc:anonmeeting,password"
X-MS-Server-Fqdn: 000DCO2L50FE1G.redmond.corp.microsoft.com
X-Powered-By: ASP.NET
X-Content-Type-Options: nosniff
Date: Sun, 12 Jan 2014 10:47:50 GMT
Content-Length: 1293
```

UCWA supports Windows Authentication, Anonymous meeting and Password Authentication mechanisms to authorize the user. i am using the Password Authentication here. On successful authentication a token is returned which can be used to issue the get request on the _user _url with the token in the header.

``` csharp    
    private void Authenticate(string authenticateUrl, string authenticateToken, string authenticateTokenType)
    {
        // Make a GET request to get the ouath url
        request = new RestRequest(authenticateUrl);
        request.AddHeader("Accept", "application/json");
        if (!string.IsNullOrEmpty(userToken))
        {
            request.AddHeader("Authorization", String.Format("{0} {1}", authenticateTokenType, authenticateToken));
        }
        ucwaClient.ExecuteAsync(request, this.ParseAuthenticateResponse);
    }

```
A successful request for the above returns the applications url. The applications url might be hosted on a different server in which case we would need to get a separate token for creating a new application. The host of applications url and the oauth url we got above should be same , or else we need to get a new token. When creating a new application, we add the token only if the host’s are same. Not adding this will again fail with Unauthorized exception giving us the new oauth url.

``` csharp
    private void CreateNewApplications(string applications)
    {
        request = new RestRequest(applications, Method.POST);
        if (CheckIfSameDomain(applications, oauthUrl))
        {
            request.AddHeader("Authorization", String.Format("{0} {1}", applicationTokenType, applicationToken));
        }
        var applicationBody = @"""UserAgent"":""{0}"",""EndpointId"":""{1}"",""Culture"":""en-US""";
        request.RequestFormat = DataFormat.Xml;
        request.AddParameter(
            "application/json",
           "{" + string.Format(applicationBody, "UCWAWindowsPhoneSample", Guid.NewGuid().ToString()) + "}",
            ParameterType.RequestBody);
        request.AddHeader("Accept", "application/json");
        ucwaClient.ExecuteAsync(request, this.CreateNewApplicationsResponse);
    }
    
    private bool CheckIfSameDomain(string url1, string url2)
    {
        // Check if the token is for the correct domain
        return new Uri(url1).Host == new Uri(url2).Host;
    }
```

Once this is done we have successfully created an application, that can be used to do a lot more things. As for the sample I have just retrieved the user’s full name, department and title that comes as part of successfully creating an application. You can do a lot more like getting the users presence, image, contacts, join meetings and a [lot more](https://ucwa.lync.com/documentation/core-features).

![image]({{ site.images_root}}/ucwa_wp_login.png)![image]({{ site.images_root}}/ucwa_wp_loggedIn_details.png)

You can find the sample [here](https://github.com/rahulpnath/Blog/tree/master/UCWA.WindowsPhone).Hope this helps you to build connected enterprise applications.

**PS**: I have tested this only against **microsoft.com** domain. In case you find any issues with the domain that you are using please do put it in the comments.
