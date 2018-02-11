# Wire <img src="https://raw.githubusercontent.com/SperoSophia/Wire/master/icon.png" width="24"> 


[![Build status](https://ci.appveyor.com/api/projects/status/2e55pfc8xbehpg33?svg=true)](https://ci.appveyor.com/project/SperoSophia/wire)
[![nuget](https://img.shields.io/nuget/v/Wire.NET.svg)](https://www.nuget.org/packages/Wire.NET/)

Wire is a quick, easy & extremely light-weight WebAPI framework. As an alternative to ASP.NET MVC, NancyFx, Nina, Sinatra and others. .NET based ofc.

## Setup

```cs
PM> Install-Package Wire.NET
```


## Code Example

Wire Supports all the standard http requests, GET, POST, PUT, DELETE, OPTIONS, PATCH. and setting up a request is fast too.
```cs
API.GET("/info/{message}", x => new { Message = x.Parameters.message });
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

I like that the core essential need of applications to have a web endpoint. so a WinForms app can have a single json endpoint as sampled above
without the need of a huge and heavy framework as ASP.NET MVC.
I liked how nancy decorated the reqests in to action parameters. It gave an illution of control.
Modules and bootstraps.
I needed a neat solution where i just throw a request to an object as above. which can be incrementally added.

```cs
for(int i = 0; i < 10; i++) {
   API.GET("/"+i, x => new { Count = ${i} });
}
```

And later...

```cs
API.Behaviours[HttpMethod.GET].FindMatch(new Uri("http://localhost/2")).Function = x => new { Changed = true };
```

If one wanted to.
