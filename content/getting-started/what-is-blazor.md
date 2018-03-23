+++
title = "What is Blazor?"
weight = 10
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-09
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## WebAssembly Changes the Game

In the past, JavaScript had a monopoly in client-side web development. As developers, we had the choice of frameworks (e.g. Angular, React, etc.) but at the end it always boiled down to JavaScript. [WebAssembly](http://webassembly.org/) changes that.

> It is a low-level assembly-like language with a compact binary format that provides a way to run code written in multiple languages on the web at near native speed.

Covering WebAssembly in details is out-of-scope of this website. If you want to learn more, here are some important links to additional material:

* [webassembly.org](http://webassembly.org/)
* [WebAssembly on MDN](https://developer.mozilla.org/en-US/docs/WebAssembly)
* [WebAssembly on GitHub](https://github.com/webassembly)
* [WebAssembly Web API](https://www.w3.org/TR/wasm-web-api-1/)

<!-- markdownlint-disable MD003 MD022 -->
## WebAssembly and C\#
<!-- markdownlint-disable MD003 MD022 -->

JavaScript is a powerful language but it has its disadvantages. Some are fixed by [TypeScript](https://www.typescriptlang.org/). However, using C# for client-side web development is compelling for many people because of reasons like the following:

* C# is a very robust and feature-rich language that has proven to be successful for projects and teams of all sizes
* Existing C# code could be re-used
* [ASP.NET Core](https://docs.microsoft.com/en-us/aspnet/core/) is a powerful programming framework for server-side web development. Enabling C# on the client would allow teams to use a common technology stack on server and client.

[Mono](http://www.mono-project.com/) is an open source implementation of Microsoft's .NET Framework based on the ECMA standards for C# and the *Common Language Runtime* (CLR). In 2017, the Mono-team has [published first results](http://www.mono-project.com/news/2017/08/09/hello-webassembly/) of their attempts to bring Mono - and with it C#, the CLR, and the .NET Framework - to WebAssembly.

> At the time of writing, Mono's C runtime is compiled into WebAssembly, and then Mono’s IL interpreter is used to run managed code.

A prototype for statically compiling managed code into one [*.wasm* file](https://developer.mozilla.org/en-US/docs/WebAssembly/Text_format_to_wasm) [already exists](http://www.mono-project.com/news/2018/01/16/mono-static-webassembly-compilation/). It is possible, if not likely, that Blazor will move away from interpreting IL towards the statically compiled model over time.

The following images illustrates the boot process of a Blazor app in Chrome. The app (*counter*) includes Blazor's JavaScript (*blazor.js*). It uses Mono's JavaScript library (*mono.js*) to bootstrap the Mono runtime (*mono.wasm*) in WebAssembly. It then loads the app's DLL (*WebApplication2.dll*) and the DLLs of the .NET Framework.

![Loading Blazor app in Chrome](/images/getting-started/chrome-load-dlls.png)

As you can see, Blazor is **not** just a new [Silverlight](https://en.wikipedia.org/wiki/Microsoft_Silverlight). The biggest difference is that it does not require a plugin. You will learn about other differences later.

## Razor

> [Razor](https://github.com/aspnet/Razor) is a template engine that combines C# with HTML to create dynamic web content.

Razor has its roots on the server where it is typically used to dynamically generate HTML. In Blazor, Razor is used on the client. To be more specific, the Razor engine runs during compilation to generate C# Code from Razor templates.

The following image illustrates the process. On the right side, you see the Razor template. On the left side, you see the C# code that is generated from this template.

![Razor template compilation](/images/getting-started/vs-razor-compilation.png)

## HTML Output

### Overview

Unlike former platforms like Silverlight, it does **not** bring its own rendering engine to paint pixels on the screen.

> Blazor uses the Browser's DOM to display data.

However, the C# code running in WebAssembly cannot access the DOM directly. It has to go through JavaScript. At the time of writing, the process works like this:

{{<mermaid>}}
sequenceDiagram
    participant CSharp
    participant JavaScript
    participant DOM
    CSharp->>JavaScript: Render Tree
    JavaScript->>DOM: Change DOM
    Note left of DOM: User triggers event (e.g. click)
    JavaScript->>CSharp: Event
    Note left of CSharp: Process event
    CSharp->>JavaScript: UI differences
    JavaScript->>DOM: Change DOM
{{< /mermaid >}}

1. The C#-part of Blazor creates a [*Render Tree*](https://github.com/aspnet/Blazor/tree/dev/src/Microsoft.AspNetCore.Blazor/RenderTree) which is a tree of UI items.

1. The render tree is passed from WebAssembly to the [*Rendering*](https://github.com/aspnet/Blazor/tree/dev/src/Microsoft.AspNetCore.Blazor.Browser.JS/src/Rendering) in the JavaScript-part of Blazor. It executes the corresponding DOM changes.

1. Whenever the user interacts with the DOM (e.g. mouse click, enter text, etc.), the JavaScript-part of Blazor [dispatches an event to C#](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Rendering/BrowserRendererEventDispatcher.cs).

1. The event is processed by the C#-code of the web app.

1. If the DOM changes, a [*Render Batch*](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Rendering/RenderBatch.cs) with all the UI tree **differences** (**not** the entire UI tree) is built in C# and given to a JavaScript Blazor method that applies the DOM changes.

Because Blazor is using the regular browser DOM, all usual DOM mechanisms including CSS work keep working.

### Renderer

In Blazor, *renderers* (classes derived from the abstract class `Microsoft.AspNetCore.Blazor.Rendering.Renderer`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Rendering/Renderer.cs)) provide mechanisms for rendering hierarchies of *components* (classes implementing `Microsoft.AspNetCore.Blazor.Components.IComponent`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Components/IComponent.cs)), dispatching events to them, and notifying when the user interface is being updated.

For running in the browser, Blazor comes with a *browser renderer* (`Microsoft.AspNetCore.Blazor.Browser.Rendering.BrowserRenderer`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Rendering/BrowserRenderer.cs)).

For unit tests, Blazor currently uses a *test renderer* (`Microsoft.AspNetCore.Blazor.Test.Helpers`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/test/shared/TestRenderer.cs))
