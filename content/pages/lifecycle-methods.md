+++
title = "Lifecycle Methods"
weight = 10
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-09
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

If you create a Blazor page (*.cshtml*), Blazor will generate a C# class (see *obj/Debug/netstandard2.0/BlazorRazorComponents.g.cs*) from it. It is derived from `Microsoft.AspNetCore.Blazor.Components.BlazorComponent` ([source on GitHub](https://github.com/aspnet/Blazor/blob/dev/src/Microsoft.AspNetCore.Blazor/Components/BlazorComponent.cs)).

The class offers a bunch of virtual methods that you can override.

## Methods to Override

### `OnInit` and `OnInitAsync`

`OnInit` and `OnInitAsync` are invoked when the component is ready to start, having received its initial parameters from its parent in the render tree. Use the asynchronous version if you need to perform asynchronous initialization tasks.

### `OnParametersSet`

`OnParametersSet` is invoked when the component has received parameters from its parent in the render tree, and the incoming values have been assigned to properties.

### `ShouldRender`

You can override `ShouldRender` to suppress refreshing of the UI. If your implementation returns `true`, UI is refreshed. Otherwise, changes are not propagated to the UI.

## Sample

The following sample defines a Blazor page that overrides lifecycle methods and adds log messages whenever they are called. Additionally, it defines a parameter that can be set by parent components.

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

To test our code, we can write the following parent component. Note that it changes the parameter `Greeting` whenever you click on the button. Note that `Name` is bound to `Initialization.Greeting`.

```
<h1>Page with Parameters</h1>

<button type="button" @onclick(SwitchName) >Switch Name</button>

<c:Initialization Name=@Name/>

@functions {
    private string Name { get; set; } = "Tom";
    private void SwitchName() => Name = Name == "Tom" ? "John" : "Tom";
}
```

If you run the code shown above and click a few times on the button, you will see the following output:

![Demo output](/images/pages/demo-lifecycle.png)
