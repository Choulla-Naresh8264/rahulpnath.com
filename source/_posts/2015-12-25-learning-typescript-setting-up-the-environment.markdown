---
layout: post
title: "Learning TypeScript: Setting up the Environment"
date: 2015-12-25 23:30:07 
comments: true
categories: 
- TypeScript
- JavaScript    
tags: 
keywords: 
description: A short post on setting up the environment for starting with TypeScript, so as to see generated JavaScript real-time. 
---

> TypeScript is  superset of JavaScript and compiles to clean JavaScript output.

[TypeScript](http://www.typescriptlang.org/) has been around for some time and is gaining more traction these days with more and more projects embracing it. The latest addition to the list [Angular 2](https://angular.io/), which is a very popular JavaScript framework. TypeScript brings in more structure to the way JavaScript is written and maintained. Being a compiled language, errors will be found as the code is written and not waiting till run time. Ease of refactoring and rich tooling support makes TypeScript a perfect choice for large-scale projects. TypeScript also provides improved code readability and organizing capabilities. The official documentation is a good starting point to get started with TypeScript and get some hands-on experience using [Playground](http://www.typescriptlang.org/Playground), which shows the compiled JavaScript real-time in browser. If you are like me, who would prefer a similar experience, but in you favourite editor then this post explains how to set up the environment for playing around with TypeScript and seeing the real-time compiled JavaScript.

- **Create folder** To start with lets first create a folder to work in. In this example I would be using npm(Node Package Manager) to get all the required packages. If you are new to Node, then head off to [here](https://nodejs.org/en/) to get started and install the runtime before continuing on. 


- **npm init** Once done with the node setup, run the [init](https://docs.npmjs.com/cli/init) command, within the project folder (created above), to initialize the node project. This prompts a series of questions and creates a *package.json* file with the entered options. 
- **npm install -g typescript** If you already have TypeScript installed (which comes in default with Visual Studio 2013 Update 2 onwards) then you can skip this step. If not, run 'npm install -g typescript', which will install it into the global scope.
- **Hello World from TypeScript** With the compiler setup, we are good to write out first hello world, for which we create a file with .ts extension.
``` javascript
function HelloWorld(name: string){
    alert('Hello World ' + name);
}

HelloWorld("TypeScript");
```
To compile this manually we need to run the TypeScript compiler (which we installed in the previous step). The below command will compile the TypeScript file into JavaScript and output into the same folder. 
``` text
tsc HelloWorld.ts
```
``` javascript
function HelloWorld(name) {
    alert('Hello ' + name);
}

HelloWorld("TypeScript");
```
- **Automating the compilation** To prevent running the above step every time we make a change to the typescript file, we can automate the build step. The tsc has a compiler switch to watch the file for changes and automatically compile every time a change happens. For this run the above command with a '*--watch*' command. 
``` text
tsc HelloWorld.ts --watch
``` 
<img class="center" alt="Visual Studio Code Coverage" src="{{ site.images_root}}/tsc_options.png" />

The watch switch is only available in the later versions of the compiler. To check whether your version supports it, run tsc alone which will show all the supported commands. If you do not see the watch switch as shown above, you will need to update the TypeScript compiler version. For this check the environment variables to see the path to your current compiler. If this has a path to an older version (possibly to one that Visual Studio installed at *Program Files (x86)\Microsoft SDKs\TypeScript*), then remove it. Now do a fresh install using npm, to install the latest version. 

Open up both the TypeScript file and the generated JavaScript file in your favorite editor and you will see real-time updates to the JavaScript code, when you update the TypeScript code.  
<img class="center" alt="Visual Studio Code Coverage" src="{{ site.images_root}}/TypeScript.gif" />  

Hope this helps you with setting up the development environment to learn TypeScript!