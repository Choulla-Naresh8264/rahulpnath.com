---
layout: post
title: "Configuring Unity Container: Comparing Code and Xml Configuration Side by Side"
date: 2014-10-19 11:42:54 
comments: true
categories: 
- .net
- Dependency Injection
tags:
- .net
- unity
---
Setting up dependency containers from code is very easy, but not at all the same when done using a configuration file. The project that I am currently working on uses xml configuration for [Unity container](https://unity.codeplex.com/) and I did struggle mapping certain dependencies, so thought of putting this up.

To start with I have created a console application and added the Unity [nuget package](https://www.nuget.org/packages/Unity/). You could directly add the configurations in the app.config file, but I prefer to keep the configurations separately in a different file, *unity.config*, and have it referred in the app.config(or web.config). Also make sure that the unity.config file gets copied to the build directory(setting build properties as content and copy always should help) so that it is available to the application.

``` xml app.config
<configSections>
  <section name="unity" type="Microsoft.Practices.Unity.Configuration.UnityConfigurationSection, Microsoft.Practices.Unity.Configuration"/>
</configSections>
<unity configSource="unity.config" />
```
In the unity.config we need to specify the assemblies and namespaces that we will be injecting the dependencies from. Inside the container is where we register the dependencies.

``` xml unity.config
<unity xmlns="http://schemas.microsoft.com/practices/2010/unity">
<!-- Define Assemblies-->
<assembly name="ConfiguringUnity" />
<!-- End Assemblies-->
<!-- Define Namespaces-->
<namespace name="ConfiguringUnity" />
<!-- End Namespaces-->
<container>
  
</container>
</unity>
```

Now that we have the basic infrastructure set up to start using the container, lets take a look at some common dependency injection scenarios that we come across. The [Unity Configuration Schema](http://msdn.microsoft.com/en-us/library/ff660914(v=pandp.20\).aspx) is worth  taking a look, to understand about the configuration elements and their attributes.

**Simple Class and Interface**  
``` csharp C#
this.unityContainer.RegisterType<NormalClass>();
this.unityContainer.RegisterType<INormalInterface, NormalInterfaceImplementation>();
``` 
``` xml unity.config
<register type="NormalClass" />
<register type="INormalInterface" mapTo="NormalInterfaceImplementation" />
``` 
Since we have only given the interface name while registering the type, specifying the assembly and namespace names of the type is important.Unity will look through these elements to find the type specified, whenever the specified type is not a full type name. This mechanism is also referred to as [Automatic Type Lookup](http://msdn.microsoft.com/en-us/library/ff660933(v=pandp.20\).aspx#_Automatic_Type_Lookup)

**Generic Interface**
``` csharp C#
this.unityContainer.RegisterType(typeof(IGenericInterface<>), typeof(GenericInterfaceImplementation<>));
this.unityContainer.RegisterType(typeof(IGenericInterfaceWithTwoParameter<,>), typeof(GenericInterfaceWithTwoParametersImplementation<,>));
```
``` xml unity.config
<register type="IGenericInterface`1" mapTo="GenericInterfaceImplementation`1" />
<register type="IGenericInterfaceWithTwoParameter`2" mapTo="GenericInterfaceWithTwoParametersImplementation`2" />
// or
<register type="IGenericInterface[]" mapTo="GenericInterfaceImplementation[]" />
<register type="IGenericInterfaceWithTwoParameter[,]" mapTo="GenericInterfaceWithTwoParametersImplementation[,]" />
```
As shown above registering [generic types](http://msdn.microsoft.com/en-us/library/ff660933(v=pandp.20\).aspx#_Generic_Types) in config can either use the CLR notation of `N, where N is the number of generic parameters or use square brackets with commas to indicate the number of parameters. Examples using one and two parameters are shown above.   
For a generic interface, the parameters can have typed parameter associated with it, something like *IComplexGenericInterface<ComplexGenericClass<GenericClass>>*. In these cases we cannot directly register this using either of the notation above, as the configuration does not allow recursive formats of those notation. We can use [Aliases](http://msdn.microsoft.com/en-us/library/ff660933(v=pandp.20\).aspx#_Type_Aliases) for specifying the parameter type names and then refer the alias for registering the interface.
``` csharp C#
this.unityContainer.RegisterType<IComplexGenericInterface<ComplexGenericClass<GenericClass>>, ComplexGenericInterfaceImplementation>();
```
```xml unity.config
 <alias alias="ComplexGenericInterfaceType" 
         type="ConfiguringUnity.ComplexGenericClass`1[[ConfiguringUnity.GenericClass, ConfiguringUnity, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]], ConfiguringUnity, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
<container>
  <register type="IComplexGenericInterface[ComplexGenericInterfaceType]" mapTo="ComplexGenericInterfaceImplementation" />
</container>
```
As shown above alias is nothing but a shorthand name that will be replaced with the full type name when the configuration is loaded. This is only available at configuration time and not at runtime.

**Conflicting Interfaces**   
When you have conflicting interface names , probably from two different assemblies then you can create aliases or use full names to register the types. For the example I have created a class library project, ExternalLibrary and added it as a reference to the Console Application.
``` csharp C#
this.unityContainer.RegisterType<IConflictingInterface, ConflictingInterfaceImplementation>();
            this.unityContainer.RegisterType<ExternalLibrary.IConflictingInterface, ExternalLibrary.ConflictingInterfaceImplementation>();
```
``` xml unity.config
<register type="IConflictingInterface" mapTo="ConflictingInterfaceImplementation" />
<register type="ExternalLibrary.IConflictingInterface, ExternalLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" mapTo="ExternalLibrary.ConflictingInterfaceImplementation, ExternalLibrary, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
```

#### ***Code and Config*** instead of ***Code Vs Config***  
 
Now that we have seen some of the common usage scenarios in registering types with containers, one main thought would be '[Should Unity be configured in code or configuration file?](http://stackoverflow.com/questions/5418392/should-unity-be-configured-in-code-or-configuration-file)'. Xml configurations are anytime a pain for the developer as it more prone to errors and configuration complexities. But then there are scenarios where dependencies would have to be plugged in at runtime, for which xml configuration is really helpful. Unity does allow to specify both together, making the best use of both worlds. You can choose to have only your dependencies that are Late Bound in the config and have all others in the code. You could also override an already registered dependency. 
``` csharp C#
this.unityContainer.RegisterType<IOverridableDependency, OverridableCodeImplementation>();
```
``` xml unity.config
<register type="IOverridableDependency" mapTo="OverridableConfigImplementation" />
```
As shown above we have a different mapping in code and config for the same interface and I am loading the configuration into the container after all the  code registrations are done. In this case the dependency that is registered last will take precedence. So you could use this feature to override any dependencies specified in the code.

There surely are a lot more cases that you would have come across while registering dependencies, do drop in a comment on the missing ones. The sample for this can be found [here](https://github.com/rahulpnath/Blog/tree/master/ConfiguringUnity)
<a href="http://www.codeproject.com" style="display:none" rel="tag">CodeProject</a>