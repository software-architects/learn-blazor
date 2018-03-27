+++
title = "JavaScript Interop" 
weight = 30 
lastModifierDisplayName = "christian.schwendtner@gmx.net, rainer@timecockpit.com" 
date = 2018-03-27
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

To update the UI, Blazor has to execute the required DOM operations by calling a JavaScript function (`renderBatch`). Blazor uses a *registered function* approach. For a JavaScript function to be called by Blazor it has to be registered upfront on the JavaScript side by its name.

When Blazor needs to update the UI it calls `UpdateDisplay` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Rendering/BrowserRenderer.cs)) which calls `RegisteredFunction.InvokeUnmarshalled` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Interop/RegisteredFunction.cs)). To invoke the JavaScript function `renderBatch` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser.JS/src/Rendering/Renderer.ts)) Blazor uses the method `InvokeJS` declared in `WebAssembly.Runtime`. If you take a look at [their source code on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Interop/WebAssembly.Runtime.cs), you will see that the methods are declared as `extern` (and use the attribute [`MethodImpl(MethodImplOptions.InternalCall)]`) which means that they are not implemented within Blazor but within the runtime (Mono).

When Blazor wants to invoke such a function (e.g. `renderBatch` to update the UI) it passes the function name (and the arguments) to `InvokeJS` of the Mono runtime which in turn invokes the specified JavaScript function.

## Calling a JavaScript function from C\#

Let's assume that we want to call a custom JavaScript function named `strlen` from C#.

To use an JavaScript function from our Blazor app, we have to register it on the JavaScript side by name using `Blazor.registerFunction` (of course you can use TypeScript to write your JavaScript function too). On the C# side we can call this function by using `RegisteredFunction.Invoke`. Here is a code example with comments describing some details:

```cs
@page "/interop-basics"
@using Microsoft.AspNetCore.Blazor.Browser.Interop
@using RestApi.Shared
@inject HttpClient Http

<h1>Interop Basics</h1>

<button @onclick(CallJS)>Call JavaScript</button>
<script>
    // Register a very simple JavaScript function that just prints
    // the input parameter to the browser's console
    Blazor.registerFunction('say', (data) => {
        console.dir(data);

        // Your function currently has to return something. For demo
        // purposes, we just return `true`.
        return true;
    });
</script>

@functions {
    private async void CallJS()
    {
        // Simple function call with a basic data type
        if (RegisteredFunction.Invoke<bool>("say", "Hello"))
        {
            // This line will be reached as our `say` function returns true
            Console.WriteLine("Returned true");
        }

        // Call our function with an object. It will be serialized (JSON),
        // passed to JS-part of Blazor and deserialized into a JavaScript
        // object again.
        RegisteredFunction.Invoke<bool>("say", new { greeting = "Hello" });

        // Get some demo data from a web service and pass it to our function.
        // Again, it will be turned to JSON and back during the function call.
        var customers = await Http.GetJsonAsync<List<Customer>>("/api/Customer");
        RegisteredFunction.Invoke<bool>("say", customers);
    }
}
```

The first parameter to `RegisteredFunction.Invoke` is the name of the JavaScript function we want to invoke, followed by the arguments of the target JavaScript function. The type parameter of `RegisteredFunction.Invoke` specifies the return type of the called function.

Remark: There are two ways to call a *registered* JavaScript function from Blazor: `RegisteredFunction.Invoke` and `RegisteredFunction.InvokeUnmarshalled` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Interop/RegisteredFunction.cs)). The first function passes the arguments (and the return value) using JSON and frees us from the need to deal with low level handling of memory and data structures on the JavaScript side, whereas the latter leaves this to us.

## Calling a C#/.NET method from JavaScript

Let's have a look at the other way around. Now we want to call a C# method from JavaScript.

First of all we need to define the C# method that we want to invoke:

```cs
namespace BlazorDemo.Client
{
    public static class StringUtil
    {
        public static string Concat(string str1, string str2, string str3) {
            return string.Concat(str1, str2, str3);
       }
    }
}
```

On the JavaScript side we have a little more work to do - there is currently no utility method that frees us from manually marshalling the arguments (and the return value). First we have to find the target method by calling `findMethod`. Then we have to convert the parameters from JavaScript string representation to .NET string representation using `toDotNetString`.

After that we can invoke the C# method by using `Blazor.platform.callMethod` passing the target method, the object instance (null in this case, because it is a static method) and the required arguments. Finally we have to convert the result of this method call (a .NET string) to JavaScript string representation. 

```js
const assemblyName = 'BlazorDemo.Client';
const namespace    = 'BlazorDemo.Client';
const typeName     = 'StringUtil';
const methodName   = 'Concat';

const concatMethod = Blazor.platform.findMethod(
    assemblyName,
    namespace,
    typeName,
    methodName
);

let arg1AsDotNetString = Blazor.platform.toDotNetString('Hello ');
let arg2AsDotNetString = Blazor.platform.toDotNetString('Blazor ');
let arg3AsDotNetString = Blazor.platform.toDotNetString('(from JS)!');

let result = Blazor.platform.callMethod(concatMethod, null, [
    arg1AsDotNetString,
    arg2AsDotNetString,
    arg3AsDotNetString
]);

var resultAsJavaScriptString = Blazor.platform.toJavaScriptString(result);
```

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

    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.0.0/css/bootstrap.min.css">
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
    <script type="blazor-boot">
    </script>
</body>
</html>
```

Next, let's look at the C# part demonstrating the JavaScript interop:

```cs
@page "/auto-complete"
@using RestApi.Shared
@using Microsoft.AspNetCore.Blazor.Browser.Interop
@using System.Linq
@inject HttpClient Http

<h1>Autocomplete Demo</h1>

<input id="customers" style="width: 300px" />
<script>
    Blazor.registerFunction('fillAutocomplete', (customers) => {
        // Fill Kendo Autocomplete
        $("#customers").kendoAutoComplete({
            dataSource: customers,
            select: (e) => {
                // Call C# when user selects an item in Kendo's Autocomplete
                const assemblyName = 'RestApi.Client';
                const namespace = 'RestApi.Client.Pages';
                const typeName = 'KendoAutocomplete';
                const methodName = 'SelectCustomer';

                const concatMethod = Blazor.platform.findMethod(
                    assemblyName,
                    namespace,
                    typeName,
                    methodName
                );

                let result = Blazor.platform.callMethod(concatMethod, null, [Blazor.platform.toDotNetString(e.dataItem)]);
            },
            filter: "startswith",
            placeholder: "Select customer...",
            separator: ", "
        });

        return true;
    });
</script>

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

        RegisteredFunction.Invoke<bool>("fillAutocomplete", projectedCustomers);
    }

    public static void SelectCustomer(string customerName)
    {
        Console.WriteLine($"Select customer '{customerName}'");
    }
}
```
