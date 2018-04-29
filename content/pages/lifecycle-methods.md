+++
title = "Lifecycle Methods"
weight = 10
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-04-29
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

If you create a Blazor page (*.cshtml*), Blazor will generate a C# class (see *./obj/Debug/netstandard2.0/Pages*) from it. It is derived from `Microsoft.AspNetCore.Blazor.Components.BlazorComponent` ([source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Components/BlazorComponent.cs)). The class offers a bunch of virtual methods that you can override.

{{% notice note %}}
The fact that Blazor turns your *.cshtml* templates into C# classes is important. The templates do not exist at runtime. Blazor just deals with ordinary .NET types.
{{% /notice %}}

Some lifecycle hooks exist in a synchronous and an asynchronous version (e.g. `OnInit` and `OnInitAsync`). Make sure to use the asynchronous version if you need to perform asynchronous initialization tasks. This is important because Blazor will re-render the page once your asynchronous initialization finished and it will handle potential exceptions correctly.

## Methods to Override

### `OnInit` and `OnInitAsync`

`OnInit` and `OnInitAsync` are invoked when the component is ready to start, having received its initial parameters from its parent in the render tree.

### `OnParametersSet` and `OnParametersSetAsync`

`OnParametersSet` and `OnParametersSetAsync` is invoked when the component has received parameters from its parent in the render tree, and the incoming values have been assigned to properties. Note that these functions are also called during the component's first initialization (after `OnInit`).

If you need to perform some tasks *before* parameters are set, you can override the virtual `SetParameters` method. However, make sure to call the base class' implementation of `Microsoft.AspNetCore.Blazor.Components.BlazorComponent`.

### `ShouldRender`

You can override `ShouldRender` to suppress refreshing of the UI. If your implementation returns `true`, UI is refreshed. Otherwise, changes are not propagated to the UI. Note that an *initial* rendering is always performed, independent of the return value of `ShouldRender`.

## Implementing `IDisposable`

Blazor components can implement `IDisposable`. If they do, the [router](../router) disposes the component when the user navigates away from the component. If you implement a component [purely in C#](../dynamic-content/#dynamic-component), you can implement `IDisposable` as usually. If you use a Blazor template, you can use `@implements IDisposable`:

```cs
...
@using System;
@implements IDisposable
...

@functions {
    public void Dispose()
    {
        ...
    }
}
```

## Sample

The following sample defines a Blazor page that overrides lifecycle methods and adds log messages whenever they are called. Additionally, it defines a parameter that can be set by parent components.

*Initialization.cshtml*

```cs
<p>Hello, @Name!</p>

<ul>
    @foreach(var item in log)
    {
        <li>@item</li>
    }
</ul>

@functions {
    // Parameter that can be set by parent component
    public string Name { get; set; }

    private List<string> log = new List<string>();
    private void Log(string message) => log.Add($"{DateTime.UtcNow.ToString("O")} - {message}");

    protected override void OnInit() => Log("OnInit");

    protected override async Task OnInitAsync()
    {
        Log("OnInitAsync starting");

        // Simulate async initialization work
        await Task.Delay(1000);

        Log("OnInitAsync finished");
    }

    protected override void OnParametersSet() => Log("OnParametersSet");
}
```

To test our code, we can write the following parent component. Note that it changes the parameter `Greeting` whenever you click on the button. Note that `Name` is bound to `Initialization.Name`.

*InitializationParent.cshtml*

```cs
@page "/initialization-parent"

<h1>Page with Parameters</h1>

<button type="button" onclick=@SwitchName>Switch Name</button>

<Initialization Name=@Name/>

@functions {
    private string Name { get; set; } = "Tom";
    private void SwitchName() => Name = Name == "Tom" ? "John" : "Tom";
}
```

If you run the code shown above and click a few times on the button, you will see the following output:

![Demo output](/images/pages/demo-lifecycle.png)
