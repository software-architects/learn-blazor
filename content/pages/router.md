+++
title = "Router"
weight = 40
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-23
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

Blazor comes with a client-side router (`Microsoft.AspNetCore.Blazor.Routing.Router`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Routing/Router.cs). At the time of writing, the router is quite limited compared to e.g. Angular's router. However, it already contains all you need to create basic web apps that consist of multiple pages.

If you create a new Blazor app, the router is configured in *App.cshtml*:

```html
<!--
    Configuring this stuff here is temporary. Later we'll move the app config
    into Program.cs, and it won't be necessary to specify AppAssembly.
-->
<Router AppAssembly=typeof(Program).Assembly />
```

## Route Templates

The router looks for all classes that implement `Microsoft.AspNetCore.Blazor.Components.IComponent` in the assembly specified in *App.cshtml* (see above). Each component class has to have a `Microsoft.AspNetCore.Blazor.Components.RouteAttribute` that specifies the *route template*. In *.cshtml*, the attribute is set using `@page` (e.g. `@page "/hello-planet/{Planet}"`). If you implement a component without a template in pure C#, you have to use the attribute (e.g. `[RouteAttribute("/hello-planet/{Planet}")]`). Note that `@page` directives are turned into `RouteAttribute` attributes in the background by the Blazor template compiler.

Here is an example for a simple template with a route template:

```cs
@page "/hello-universe"

<h1>Hello Universe!</h1>
```

Components can have multiple routes on which they are available. If you need that, just add multiple `@page` directives or `RouteAttribute` attributes:

```cs
@page "/"
@page "/Page1"
<h1>Page1</h1>
```

Route templates can contain parameters. In `@page "/hello-planet/{Planet}"`, `{Planet}` would be such a parameter. Parameters are assigned to properties of the component. Here is an example for a simple template with a route parameter:

```cs
@page "/hello-planet/{Planet}"

<h1>Hello @Planet!</h1>

@functions {
    public string Planet { get; set; }

    protected override void OnInit()
    {
        Console.WriteLine(Planet);
    }
}
```

## Links

You can define *relative* links as usual with the HTML element `a`. The default Blazor templates adds a [`base` element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/base) to `index.html`: `<base href="/" />`. Therefore, relative links will not reload the entire page but will be handled by Blazor's router on the client-side.

Blazor also comes with a helper class `NavLink` that can be used as an alternative. It automatically sets the `active` CSS class if the `href` matches the current URI.

Here is an example of a simple menu with client-side links:

```html
@page "/"
<p>
    <a href="/hello-universe">Hello Universe</a><br />
    <a href="/hello-world">Hello World</a><br />
    <a href="/hello-world?type=beautiful">Hello beautiful World</a><br />
    <a href="/outer-space/hello-moon">Hello Moon</a><br />
    <a href="/hello-planet/Epsilon-Eridani">Hello Planet Epsilon Eridani</a>
</p>

<p>
    <NavLink href="/hello-universe">Hello Universe</NavLink><br />
</p>
```

Note that Blazor does not recreate a component if only the URL parameters change. If a user is in the sample shown above already on the page *HelloWorld* and clicks on the link *Hello beautiful World*, no new instance of the `HelloWorld` class is created. The existing class is reused instead.

## Accessing Query Parameters

You can add query parameters using the `IUriHelper` [default service](https://learn-blazor.com/architecture/dependency-injection/#default-services). The following example shows how this can be done. Note that it requires referencing the NuGet package `Microsoft.AspNetCore.WebUtilities` in your *.csproj* file.

```cs
@page "/hello-world"
@implements IDisposable
@inject Microsoft.AspNetCore.Blazor.Services.IUriHelper UriHelper

<h1>Hello @Type World!</h1>

<MainMenu />

@functions {
    private string Type { get; set; }

    protected override void OnInit()
    {
        RefreshType();
        UriHelper.OnLocationChanged += OnLocationChanges;
    }

    private void OnLocationChanges(object sender, string location) => RefreshType();

    private void RefreshType()
    {
        var uri = new Uri(UriHelper.GetAbsoluteUri());
        Type = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(uri.Query).TryGetValue("type", out var type) ? type.First() : "";
        StateHasChanged();
    }

    public void Dispose()
    {
        UriHelper.OnLocationChanged -= OnLocationChanges;
    }
}
```

## Navigate in Code

`IUriHelper` can also be used to trigger navigation in code. The following example demonstrates how to navigate when the user clicks a button:

```cs
@page "/navigate-in-code"
@inject Microsoft.AspNetCore.Blazor.Services.IUriHelper UriHelper

<button @onclick(Navigate)>Click me to navigate to another page</button>

<MainMenu />

@functions {
    private void Navigate()
    {
        UriHelper.NavigateTo("/hello-world");
    }
}
```

## Future Enhancements

Note that the Blazor teams tracks future enhancements of the router [in this GitHub issue](https://github.com/aspnet/Blazor/issues/293).
