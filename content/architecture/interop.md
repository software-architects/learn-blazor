+++
title = "JavaScript Interop" 
weight = 30 
lastModifierDisplayName = "rainer@timecockpit.com, christian.schwendtner@gmx.net" 
date = 2018-10-02
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

Blazor is built upon [Mono and WebAssembly (short *Wasm*)](http://www.mono-project.com/news/2017/08/09/hello-webassembly/). Currently, WebAssembly and therefore Mono and Blazor have no direct access to the browser's DOM API. But given that Blazor is a frontend framework, it needs to access the DOM to build the user interface. Additionally, as web developers we want to access browser APIs and existing JavaScript functions, too.

WebAssembly programs can call JavaScript functions and those in turn can use all available browser APIs. Because Mono is compiled to WebAssembly and Blazor runs on Mono, Blazor can indirectly call any JavaScript functions with the aid of Mono. The following image shows how JavaScript and Wasm work together when handling a button click. The DOM event is handled in JavaScript. The event is passed on to Wasm where it is handled by your C# code. The resulting DOM changes are described in the form of a so called *Render Batch*. JavaScript is taking this data and changes the DOM accordingly.

![Click handling via JS and Wasm](/images/architecture/click-flow.png)

## Behind the scenes

Let's look at the case when Blazor wants to update the UI.

To update the UI, Blazor has to execute the required DOM operations by calling a JavaScript function (`renderBatch`). Blazor uses the global scope (the `window` object) to find methods. For a JavaScript function to be called by Blazor it has to be assigned to the the `window` object by its name.

When Blazor needs to update the UI it calls `UpdateDisplay` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/master/src/Microsoft.AspNetCore.Blazor.Browser/Rendering/BrowserRenderer.cs)) which calls `MonoWebAssemblyJSRuntime.InvokeUnmarshalled` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/master/src/Mono.WebAssembly.Interop/MonoWebAssemblyJSRuntime.cs)). To invoke the JavaScript function `renderBatch` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/master/src/Microsoft.AspNetCore.Blazor.Browser.JS/src/Rendering/Renderer.ts)) Blazor uses the method `InvokeJSUnmarshalled` declared in `Mono.WebAssembly.Interop.InternalCalls`. If you take a look at [their source code on GitHub](https://github.com/aspnet/Blazor/blob/master/src/Mono.WebAssembly.Interop/InternalCalls.cs), you will see that the methods are declared as `extern` (and use the attribute [`MethodImpl(MethodImplOptions.InternalCall)]`) which means that they are not implemented within Blazor but within the runtime (Mono).

When Blazor wants to invoke such a function (e.g. `renderBatch` to update the UI) it passes the function name (and the arguments) to `InvokeJSUnmarshalled` of the Mono runtime which in turn invokes the specified JavaScript function.

## Calling a JavaScript function from C\#

To use an JavaScript function from our Blazor app, we have to assign it it on the JavaScript side to the global scope (the `window` object). On the C# side we can call this function by using `JSRuntime.Current.InvokeAsync`. Here is a code example with comments describing some details:

```html
<!DOCTYPE html>
<html>
<head>...</head>
<body>
    <app>Loading...</app>

    ...
    <script src="_framework/blazor.webassembly.js"></script>
    <script>
        // Define a very simple JavaScript function that just prints
        // the input parameter to the browser's console
        window.say = (data) => {
            console.dir(data);

            // Your function currently has to return something. For demo
            // purposes, we just return `true`.
            return true;
        };
    </script>
    ...
</body>
</html>
```

```cs
@page "/interop-basics"
@using Microsoft.JSInterop;
@using RestApi.Shared
@inject HttpClient Http

<h1>Interop Basics</h1>

<button onclick=@CallJS>Call JavaScript</button>

@functions {
    private async void CallJS()
    {
        // Simple function call with a basic data type
        if (await JSRuntime.Current.InvokeAsync<bool>("say", "Hello"))
        {
            // This line will be reached as our `say` function returns true
            Console.WriteLine("Returned true");
        }

        // Call our function with an object. It will be serialized (JSON),
        // passed to JS-part of Blazor and deserialized into a JavaScript
        // object again.
        await JSRuntime.Current.InvokeAsync<bool>("say", new { greeting = "Hello" });

        // Get some demo data from a web service and pass it to our function.
        // Again, it will be turned to JSON and back during the function call.
        var customers = await Http.GetJsonAsync<List<Customer>>("/api/Customer");
        await JSRuntime.Current.InvokeAsync<bool>("say", customers);
    }
}
```

The first parameter to `JSRuntime.Current.InvokeAsync` is the name of the function identifier relative to the global scope (`window`)  we want to invoke, followed by the arguments of the target JavaScript function. The type parameter of `JSRuntime.Current.InvokeAsync` specifies the return type of the called function.

*There used to be two ways to call a *registered* JavaScript function from Blazor: `RegisteredFunction.Invoke` and `RegisteredFunction.InvokeUnmarshalled`. Since `v0.5.0`, these methods have been retired in favour of the [IJsRuntime](https://blazor.net/api/Microsoft.JSInterop.IJSRuntime.html) interface, which only exposes `InvokeAsync` for invoking JavaScript methods. The `RegisteredFunction` class is no longer available in the API.*

JavaScript interop of Blazor does not change the casing of members when turning data into JSON. Therefore a C# property called `SomeValue` is *not* turned into `someValue` in JavaScript and vice versa.

## Calling a C#/.NET method from JavaScript

Let's have a look at the other way around. Now we want to call a C# method from JavaScript.

First of all we need to define the C# method that we want to invoke. Note the `JSInvokable` attribute applied to the method.

```cs
using Microsoft.JSInterop;
using System.Threading.Tasks;

namespace RestApi.Client.Pages
{
    public static class StringUtil
    {
        [JSInvokable]
        public static Task<string> Concat(string str1, string str2, string str3)
        {
            return Task.FromResult(string.Concat(str1, str2, str3));
        }
    }
}
```

In older versions of Blazor, calling the C# method required quite a bit of code. It has become much simpler. Just call `DotNet.invokeMethod` or `DotNet.invokeMethodAsync`:

```js
// Define a very simple JavaScript function that just prints
// the input parameter to the browser's console
window.say = async (data) => {
    // Demonstrate how to call a C# method from JavaScript
    console.log(await DotNet.invokeMethodAsync('RestApi.Client', 'Concat', 'Recieved call ', 'from C#. ', 'Here is the received parameter:'));

    console.dir(data);

    // Your function currently has to return something. For demo
    // purposes, we just return `true`.
    return true;
};
```

{{% notice note %}}
Note that if you want to add javascript event listener registrations (e.g. window.addEventListener("orientationchange", function () { //do something }))  you will need to wrap the event listener registrations in a javascript function and call the function from Program.cs using Javascrip Interop described above. This will make sure .NET is loaded before trying to find the C# method via Blazor.platform.findMethod().
{{% /notice %}}

## Integrated Sample

Let's take a look at an example that brings together both directions (JS -> C#, C# -> JS). The use case in this example is integrating an existing JavaScript UI library ([Telerik Kendo UI](https://www.telerik.com/kendo-ui)) with Blazor. There is no native Blazor library for Kendo UI. Therefore, we have to use it's JavaScripit API. The example uses Kendo's [*Autocomplete* control](https://demos.telerik.com/kendo-ui/autocomplete/index).

{{% notice note %}}
Note that ([Telerik Kendo UI](https://www.telerik.com/kendo-ui)) consists of an open source part that is free and some more advanced controls which are liable to cost. [Read more...](https://www.telerik.com/kendo-ui/open-source-core)
{{% /notice %}}

First, let's start with *index.html*:

```html
<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1, shrink-to-fit=no">

    <title>REST API</title>

    <base href="/" />

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css" integrity="sha384-Gn5384xqQ1aoWXA+058RXPxPg6fy4IWvTNh0E263XmFcJlSAwiGgFAW/dAiS6JXm" crossorigin="anonymous">
    <link href="css/site.css" rel="stylesheet" />
    <link rel="stylesheet" href="https://kendo.cdn.telerik.com/2018.1.221/styles/kendo.common.min.css" />
    <link rel="stylesheet" href="https://kendo.cdn.telerik.com/2018.1.221/styles/kendo.blueopal.min.css" />
</head>
<body>
    <app>Loading...</app>

    <script src="https://kendo.cdn.telerik.com/2018.1.221/js/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.12.9/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/js/bootstrap.min.js"></script>
    <script src="https://kendo.cdn.telerik.com/2018.1.221/js/kendo.all.min.js"></script>
    <script src="_framework/blazor.webassembly.js"></script>
    <script>
        // Define a very simple JavaScript function that just prints
        // the input parameter to the browser's console
        window.say = async (data) => {
            // Demonstrate how to call a C# method from JavaScript
            console.log(await DotNet.invokeMethodAsync('RestApi.Client', 'Concat', 'Recieved call ', 'from C#. ', 'Here is the received parameter:'));

            console.dir(data);

            // Your function currently has to return something. For demo
            // purposes, we just return `true`.
            return true;
        };
    </script>
    <script>
        window.fillAutocomplete = (customers) => {
            // Fill Kendo Autocomplete
            $("#customers").kendoAutoComplete({
                dataSource: customers,
                select: (e) => {
                    // Call C# when user selects an item in Kendo's Autocomplete
                    DotNet.invokeMethod('RestApi.Client', 'SelectCustomer', e.dataItem);
                },
                filter: "startswith",
                placeholder: "Select customer...",
                separator: ", "
            });

            return true;
        };
    </script>
</body>
</html>
```

Next, let's look at the C# part demonstrating the JavaScript interop:

```cs
@page "/auto-complete"
@using RestApi.Shared
@using Microsoft.JSInterop
@using System.Linq
@inject HttpClient Http

<h1>Autocomplete Demo</h1>

<input id="customers" style="width: 300px" />

@functions {
    protected override async Task OnInitAsync()
    {
        await RefreshCustomerList();
    }

    private async Task RefreshCustomerList()
    {
        // Call a web api to get some data
        var customers = await Http.GetJsonAsync<List<Customer>>("/api/Customer");

        // Here you can execute any C# business logic. For demo purposes, we 
        // project the result into a list of strings.
        var projectedCustomers = customers
            .Select(c => $"{c.LastName}, {c.FirstName}")
            .ToList();

        await JSRuntime.Current.InvokeAsync<bool>("fillAutocomplete", projectedCustomers);
    }

    [JSInvokable]
    public static void SelectCustomer(string customerName)
    {
        Console.WriteLine($"Select customer '{customerName}'");
    }
}
```
