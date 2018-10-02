+++
title = "Getting Blazor"
weight = 20
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-10-01
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Getting the Blazor Bits

At the time of writing, the latest version of Blazor is 0.5.1 and a preview of 0.6.0 is available ([read more](https://github.com/aspnet/Blazor/releases)). The [Blazor documentation](https://blazor.net/docs/get-started.html) contains a description for how to install the Blazor language services in Visual Studio and the templates for the dotnet CLI.

## Get a Compatible Browser

All recent versions of modern browsers support [WebAssembly](http://webassembly.org/). Check [Can I Use](https://caniuse.com/#search=webassembly) for detailed information about availability of WebAssembly on desktop and mobile browsers.

With the [server-side hosting model](https://blazor.net/docs/host-and-deploy/hosting-models.html#server-side-hosting-model), WebAssembly is no longer needed on the client as the C# code is executed on the server.
