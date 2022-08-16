# Wire <img src="https://raw.githubusercontent.com/SperoSophia/Wire/master/icon.png" width="24"> 

[![Build](https://github.com/theoistic/wire/actions/workflows/build.yml/badge.svg)](https://github.com/theoistic/wire/actions/workflows/build.yml)
[![nuget](https://img.shields.io/nuget/v/Wire.NET.svg)](https://www.nuget.org/packages/Wire.NET/)

Wire is a quick, easy & extremely light-weight WebAPI framework. As an alternative to ASP.NET MVC, NancyFx, Nina, Sinatra and others. .NET based ofc.

## Setup

```cs
PM> Install-Package Wire.NET
```


## Code Example

Wire Supports all the standard http requests, GET, POST, PUT, DELETE, OPTIONS, PATCH. and setting up a request is fast too.
```cs
using Wire;

// Start the server
var server = new WireHTTPServer();

// Sample API endpoint
API.GET("/info/{message}", x => new { Message = x.Parameters.message });

// Wait for key press to terminate program
server.Wait();
```
so in the example of http://localhost/info/OK
would return a json reponse as { Message: "OK" }


```cs
API.POST("/blog/post", x => BlogRepository.Save(x.Body.As<Post>()));
```
Save the body of the post to your post repositoty in this instance.


```cs
API.GET("/blog/posts", x => MyBlogPosts.Where(y => y.name.contains(x.QueryString["q"]))));
```
If we need to filter something out. in the case of http://localhost/blog/posts?q=first

## Motivation

I like that the core essential need of applications to have a web endpoint. so a console app can have a single json endpoint as sampled above
without the need of a huge and heavy framework as ASP.NET MVC.
I liked how nancy decorated the reqests in to action parameters. It gave an illution of control.
Modules and bootstraps.
I needed a neat solution where i just throw a request to an object as above. which can be incrementally added.

```cs
for(int i = 0; i < 10; i++) {
   API.GET("/"+i, x => new { Count = ${i} });
}
```

API bindings can be replaced runtime as well, so if something is mapped to /hello and you map another function to an existing mapping its replaced.


## Experimental

Experimental implimentation of an ASP.NET middleware

```cs
using Wire;
using Wire.ASPNET;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Attach the Wire ASP.NET middleware
app.UseWire();

// Declare an Endpoint
API.GET("/", x =>
{
    return new { Message = "Hello World" };
});

app.Run();
```
