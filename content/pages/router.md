+++
title = "Router"
weight = 40
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-12
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

Blazor comes with a client-side router (`Microsoft.AspNetCore.Blazor.Routing.Router`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/dev/src/Microsoft.AspNetCore.Blazor/Routing/Router.cs). At the time or writing, the router is quite limited compared to e.g. Angular's router. However, it already contains all you need to create basic web apps that consist of multiple pages.

If you create a new Blazor app, the router is configured in *App.cshtml*. Note in particular the property `DefaultComponentName`. You can define the default page with it.

```html
<!--
    Configuring this stuff here is temporary. Later we'll move the app config
    into Program.cs, and it won't be necessary to specify AppAssembly.
-->
<c:Router AppAssembly=@(typeof(Program).Assembly)
          PagesNamespace=@("Router.Pages")
          DefaultComponentName=@("HelloWorld")/>
```

## Class Resolution

The router looks for classes in the `Pages` namespace whose name fits to the query string. Therefore, `http://localhost:<port>/HelloUniverse` loads the component (=class) `HelloUniverse` in the namespace `Router.Pages` assuming that `Router` is the base namespace of the web app. `http://localhost:<port>/OuterSpace/HelloMoon` would load the component `Router.Pages.HelloMoon`.

## Links

You can define *relative* links as usual with the HTML element `a`. The default Blazor templates adds a [`base` element](https://developer.mozilla.org/en-US/docs/Web/HTML/Element/base) to `index.html`: `<base href="/" />`. Therefore, relative links will not reload the entire page but will be handled by Blazor's router on the client-side.

Blazor also comes with a helper class `NavLink` that can be used as an alternative. It automatically sets the `active` CSS class if the `href` matches the current URI.

Here is an example of a simple menu with client-side links:

```html
<p>
    <a href="/HelloUniverse">Hello Universe</a><br />
    <a href="/HelloWorld">Hello World</a><br />
    <a href="/HelloWorld?type=beautiful">Hello beautiful World</a><br />
    <a href="/OuterSpace/HelloMoon">Hello Moon</a>
</p>

<p>
    <c:NavLink href=@("/HelloUniverse")>Hello Universe</c:NavLink><br />
</p>
```

Note that Blazor does not recreate a component if only the URL parameters change. If a user is in the sample shown above already on the page *HelloWorld* and clicks on the link *Hello beautiful World*, no new instance of the `HelloWorld` class is created. The existing class is reused instead.

## Accessing Query Parameters

You can add query parameters using the `IUriHelper` [default service](http://localhost:1313/architecture/dependency-injection/#default-services). The following example shows how this can be done. Note that it requires referencing the NuGet package `Microsoft.AspNetCore.WebUtilities` in your *.csproj* file.

```cs
@using Microsoft.AspNetCore.Blazor.Services
@using Microsoft.AspNetCore.WebUtilities;
@using System;
@(Implements<IDisposable>())
@inject IUriHelper UriHelper

<h1>Hello @Type World!</h1>

<c:Menu />

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
        Type = QueryHelpers.ParseQuery(uri.Query).TryGetValue("type", out var type) ? type.First() : "";
        StateHasChanged();
    }

    public void Dispose()
    {
        UriHelper.OnLocationChanged -= OnLocationChanges;
    }
}
```