+++
title = "Getting Blazor"
weight = 20
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-09
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Getting the Blazor Bits

At the time of writing, Microsoft does neither offer an installer for Blazor nor a compiled version for download. You have to compile the code yourself. The [Blazor GitHub repository](https://github.com/aspnet/Blazor) contains a step-by-step description for building Blazor and the corresponding *Visual Studio* tools.

Once you have built the Blazor tools, you can find a *.vsix* file in the *artifacts\build* folder. Install it to get Blazor in Visual Studio. To verify you success, try creating a new *ASP.NET Core Web Application*. You should see Blazor templates in the *New ASP.NET Core Web Application* wizard:

![Blazor project templates in Visual Studio](/images/getting-started/vs-project-template.png)

## Get a Compatible Browser

All recent versions of modern browsers support [WebAssembly](http://webassembly.org/). Check [Can I Use](https://caniuse.com/#search=webassembly) for detailed information about availability of WebAssembly on desktop and mobile browsers.

At the time of writing, Blazor does not run in *Internet Explorer 11*. However, this might change as there is nothing fundamental to Blazor that won't work there (see also [this GitHub issue](https://github.com/aspnet/Blazor/issues/260)).
