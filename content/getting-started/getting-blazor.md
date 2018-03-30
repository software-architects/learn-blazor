+++
title = "Getting Blazor"
weight = 20
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-23
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Getting the Blazor Bits

At the time of writing, a [first public preview 0.1.0](https://github.com/aspnet/Blazor/releases/tag/0.1.0) is available. To get it:

* Install the [.NET Core 2.1 Preview 1 SDK](https://www.microsoft.com/net/download/dotnet-core/sdk-2.1.300-preview1).
* Install the latest preview of [Visual Studio 2017 (15.7)](https://www.visualstudio.com/vs/preview) with the ASP.NET and web development workload.
  * Note: You can install Visual Studio previews side-by-side with an existing Visual Studio installation without impacting your existing development environment.
* Install the [ASP.NET Core Blazor Language Services extension](https://go.microsoft.com/fwlink/?linkid=870389) from the Visual Studio Marketplace.


If you want to build the source code and tools yourself, the [Blazor GitHub repository](https://github.com/aspnet/Blazor) contains a step-by-step description for building Blazor and the corresponding *Visual Studio* tools. Once you have built the Blazor tools, you can find a *.vsix* file in the *artifacts/build* folder. Install it to get Blazor in Visual Studio.

To verify your success of either installation approach, try creating a new *ASP.NET Core Web Application*. You should see Blazor templates in the *New ASP.NET Core Web Application* wizard:

![Blazor project templates in Visual Studio](/images/getting-started/vs-project-template.png)

## Get a Compatible Browser

All recent versions of modern browsers support [WebAssembly](http://webassembly.org/). Check [Can I Use](https://caniuse.com/#search=webassembly) for detailed information about availability of WebAssembly on desktop and mobile browsers.

At the time of writing, Blazor does not run in *Internet Explorer 11* (see also [release notes for version 0.1.0](https://github.com/aspnet/Blazor/releases/tag/0.1.0). However, this might change as there is nothing fundamental to Blazor that won't work there (see also [this GitHub issue](https://github.com/aspnet/Blazor/issues/260)).
