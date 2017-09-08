# Wire

Wire is a quick, easy & extremely light-weight WebAPI framework. As an alternative to ASP.NET MVC, NancyFx, Nina, Sinatra and others. .NET based ofc.

## Code Example

```cs
API.GET("/info/{message}", x => new { Message = "OK" });
```

would return a json reponse as { Message: "OK" }

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
API._APIBehaviours["GET"].FindMatch(new Uri("http://localhost/2")).Function = x => new { Changed = true };
```

If one wanted to. (will clean up the API alot, still prototyping.)
