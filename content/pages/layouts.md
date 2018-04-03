+++
title = "Layouts"
weight = 50
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-04-03
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

Blazor apps typically contain more than one page. Certain layout elements like menus, copyright messages, logos, etc. must be present on all pages. Copying the code of these layout elements onto all pages would not be a good solution. The app would become hard to maintain and probably inconsistent over time. Blazor Layouts solve this problem.

## What are Blazor layouts?

Technically, a Blazor layout is just another Blazor component. It is defined in a Razor template or in C# code, it can contain data binding, dependency injection is supported, etc. Two additional aspects are special and turn a Blazor component into a Blazor layout:

* The layout component has to implement `Microsoft.AspNetCore.Blazor.Layouts.ILayoutComponent`. This interface adds a property `Body` to the component which contains the content to be rendered inside the layout.
* The layout component contains the Razor keyword `@Body`. During rendering, it is replaced by the content of the layout.

The following code sample shows the Razor template of a layout component. Note the use of `ILayoutComponent` and `@Body`:

```csharp
@implements ILayoutComponent

<header>
    <h1>ERP Master 3000</h1>
    <p>Current user: @UserName</p>
</header>

<nav>
    <a href="/master-data">Master Data Management</a>
    <a href="/invoicing">Invoicing</a>
    <a href="/accounting">Accounting</a>
</nav>

@Body

<footer>
    (c) by Acme Corp
</footer>

@functions {
    public string UserName { get; set; }

    public RenderFragment Body { get; set; }
}
```

## Using a layout in a Blazor component

Use the Razor keyword `@layout` to apply a layout to a Blazor component. The Blazor compiler will turn this keyword into the `Microsoft.AspNetCore.Blazor.Layouts.LayoutAttribute` attribute which is applied to the Blazor component class.

Here is a code sample demonstrating the concept. The content of this component will be inserted into the *MasterLayout* at the position of `@Body`.

```csharp
@layout MasterLayout

@page "/master-data"

<h2>Master Data Management</h2>
...
```

## Centralized layout selection

Every folder of a Blazor app can optionally contain a template file named *_ViewImports.cshtml*. The Blazor compiler includes the content of the file in all Razor templates in the same folder and recursively in all subfolders. Therefore, a *_ViewImports.cshtml* file containing `@layout MainLayout` ensures that all Blazor components in this folder use the *MainLayout* layout. There is no need to repeatedly add `@layout` to all *.cshtml* files.

Note that the default template for Blazor apps uses the *_ViewImports.cshtml* mechanism for layout selection. A newly created Blazor app contains the relevant *_ViewImports.cshtml* file in the *Pages* folder.

## Nested layouts

Blazor apps can consist of nested layouts. That means that a Blazor component can reference a layout which in turn references another layout. This nesting can be used to e.g. reflect a multi-level menu structure.

The following code samples show how nesting of layouts look like in the code. The *.cshtml* file of the Blazor component we want to display looks like this. Note that it references the layout `MasterDataLayout`:

```csharp
@layout MasterDataLayout

@page "/master-data/customers"

<h1>Customer Maintenance</h1>
...
```

The *.cshtml* file of `MasterDataLayout` has all the characteristics of a Blazor layout. Additionally, it references another layout `MainLayout` in which it is going to be embedded:

```csharp
@layout MainLayout
@implements ILayoutComponent

<nav>
    <!-- Menu structure of master data module -->
    ...
</nav>

@Body
@functions {
    ...
    public RenderFragment Body { get; set; }
}
```

Finally, `MainLayout` contains the top-level layout elements like header, footer, and main menu:

```csharp
@implements ILayoutComponent

<header>...</header>
<nav>...</nav>

@Body

@functions {
    public RenderFragment Body { get; set; }
}
```
