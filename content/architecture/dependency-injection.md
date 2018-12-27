+++
title = "Dependency Injection"
weight = 10
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-10-02
+++

## Introduction

Blazor has [dependency injection (DI)](https://docs.microsoft.com/aspnet/core/fundamentals/dependency-injection) built in. Blazor apps can use built-in services by having them injected into components. Blazor apps can also define custom services and make them available via DI.

## What is dependency injection?

DI is a technique for achieving loose coupling between objects. The goal is to have high-level components depend on abstractions (for example, [interfaces](https://docs.microsoft.com/dotnet/csharp/programming-guide/interfaces/)) and not on low level components.

Imagine you want to create an application service, `DataAccess`, for data access. Blazor components use this service to obtain data from backend servers using web APIs. The component should *not* create an instance of `DataAccess`. Instead, the app should create an abstraction, `IDataAccess`, that `DataAccess` implements. Blazor components that require data access features use the `IDataAccess` abstraction. Components never directly reference `DataAccess`. Therefore, components that depend on data access features are independent of the concrete implementation, and they can work with different implementations of `IDataAccess` (for example, a [mock object](https://wikipedia.org/wiki/Mock_object) in a unit test).

Blazor's DI system is responsible for connecting component abstractions with their concrete implementations. To connect the abstractions with the implementations, DI is configured during startup of the app. An example is shown later in this topic.

## Use of existing .NET mechanisms

DI in Blazor is based on .NET's [System.IServiceProvider](https://docs.microsoft.com/dotnet/api/system.iserviceprovider) interface. It defines a generic mechanism for retrieving a service object in .NET apps.

Blazor's implementation of `System.IServiceProvider` obtains its services from an underlying [Microsoft.Extensions.DependencyInjection.IServiceCollection](https://docs.microsoft.com/dotnet/api/microsoft.extensions.dependencyinjection.iservicecollection).

## Add services to DI

After creating a new Blazor app, examine the `Startup` class in *Startup.cs*. If you know ASP.NET Core, this class should look familar to you.

```cs
using DependencyInjection.Pages;
using DependencyInjection.Services;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Add additional services here
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
```

Blazor comes with a service provider specifically for the browser (`Microsoft.AspNetCore.Blazor.Browser.Services.BrowserServiceProvider`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/master/src/Microsoft.AspNetCore.Blazor.Browser/Services/BrowserServiceProvider.cs)). The following is a code sample demonstrating how to add services to it:

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IRepository, Repository>();
    services.AddTransient<MyTransientService, MyTransientService>();
    services.AddScoped<MyScopedService, MyScopedService>();
    services.AddSingleton<MySingletonService, MySingletonService>();
}
```

`IServiceCollection` offers various overloads of three different functions that can be used to add services to Blazor's DI:

| Method      | Description |
| ----------- | ----------- |
| `AddSingleton` | Blazor creates a *single instance* of your service. All components requiring this service receive a reference to this instance. Server-side Blazor shares the instance with all user sessions. For client-side Blazor, the instance lives in the client and is therefore session private. |
| `AddTransient` | Whenever a component requires this service, it receives a *new instance* of the service. |
| `AddScoped`    | Server-side Blazor creates a separate service instance for each SignalR connection. It is therefore session private. A page reload creates a new SignalR connection and therefore a new service instance; so it will not live as long as the session. Client-side Blazor doesn't currently have the concept of DI scopes. `Scoped` behaves like `Singleton`, i.e. it is session private. |

## System services

Blazor provides default services that are automatically added to the service collection of a Blazor app. The following table shows a list of the default services currently provided by Blazor.

| Method       | Description |
| ------------ | ----------- |
| `IUriHelper` | Helpers for working with URIs and navigation state (singleton). |
| `HttpClient` | Provides methods for sending HTTP requests and receiving HTTP responses from a resource identified by a URI (singleton). Note that this instance of [System.Net.Http.HttpClient](https://docs.microsoft.com/dotnet/api/system.net.http.httpclient) is using the browser for handling the HTTP traffic in the background. Its [BaseAddress](https://docs.microsoft.com/dotnet/api/system.net.http.httpclient.baseaddress) is automatically set to the base URI prefix of the Blazor app. |

## Custom services

Take a look at the following simplified code sample. It defines a repository (e.g. a database) from which we can get customer data asynchronously. In order to decouple the user of the repository (e.g. Blazor component) from the implementation (`Repository` class shown below), we introduce an interface (`IRepository`). The decoupling enables us to use different repository implementations e.g. for unit tests.

```cs
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Architecture.Services
{
    public class Customer
    {
        public string FirstName { get; set; }

        public string LastName { get; set; }
    }

    public interface IRepository
    {
        Task<IReadOnlyList<Customer>> GetAllCustomersAsync();
    }

    public class Repository : IRepository
    {
        private static Customer[] Customers { get; set; } = new[]
        {
            new Customer { FirstName = "Foo", LastName = "Bar" },
            new Customer { FirstName = "John", LastName = "Doe" }
        };

        // Note that the constructor gets an HttpClient via dependency
        // injection. HttpClient is a default service offered by Blazor.
        // Note that additional parameters are ok if you specify default
        // value for them.
        public Repository(HttpClient client, string something = "dummy")
        {
            // In practice, we would store the HttpClient and use it
            // to get customers via e.g. a RESTful Web API
        }

        public async Task<IReadOnlyList<Customer>> GetAllCustomersAsync()
        {
            // Simulate some long running, async work (e.g. web request)
            await Task.Delay(100);

            return Repository.Customers;
        }
    }
}
```

## Injecting Services in components

To inject a service in a component, use the `@inject` keyword as shown in the following sample. Technically, this generates a property with the given name and type. The property is decorated with the attribute `Microsoft.AspNetCore.Blazor.Components.InjectAttribute` so that Blazor's component factory (`Microsoft.AspNetCore.Blazor.Components.ComponentFactory`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Components/ComponentFactory.cs)) knows that it has to fill it when creating the component. Multiple `@inject` statements can be used to inject different services.

```cs
@page "/"
@using DependencyInjection.Services
@inject IRepository Repository

<ul>
    @if (Customers != null)
    {
        @foreach (var customer in Customers)
        {
            <li>@customer.FirstName @customer.LastName</li>
        }
    }
</ul>

@functions {
    private IReadOnlyList<Customer> Customers;

    protected override async Task OnInitAsync()
    {
        Customers = await Repository.GetAllCustomersAsync();
    }
}
```

The `InjectAttribute` isn't typically used directly. If a base class is required for Blazor components and injected properties are also required for the base class, `InjectAttribute` can be manually added:

```csharp
public class ComponentBase : BlazorComponent
{
    // Note that Blazor's dependency injection works even if using the
    // InjectAttribute in a component's base class.
    [Inject]
    protected IDataAccess DataRepository { get; set; }
    ...
}
```

In components derived from the base class, the `@inject` directive isn't required. The `InjectAttribute` of the base class is satisfactory:

```csharp
@page "/demo"
@inherits ComponentBases

<h1>...</h1>
...
```

## Dependency injection in services

Complex services might require additional services. In the prior example, `DataAccess` might require Blazor's default service `HttpClient`. `@inject` or the `InjectAttribute` can't be used in services. *Constructor injection* must be used instead. Require services are added by adding parameters to the service's constructor. When dependency injection creates the service, it recognizes the services it requires in the constructor and provides them accordingly.

The following code sample demonstrates the concept:

```csharp
public class DataAccess : IDataAccess
{
    // Note that the constructor receives an HttpClient via dependency
    // injection. HttpClient is a default service offered by Blazor.
    public Repository(HttpClient client)
    {
        ...
    }
    ...
}
```

Note the following prerequisites for constructor injection:

* There must be one constructor whose arguments can all be fulfilled by dependency injection. Note that additional parameters not covered by DI are allowed if default values are specified for them.
* The applicable constructor must be *public*.
* There must only be one applicable constructor. In case of an ambiguity, DI throws an exception.

## Service lifetime

Note that Blazor doesn't automatically dispose injected services that implement `IDisposable`. Blazor components can implement `IDisposable`. Services are disposed when the user navigates away from the component.

The following code sample demonstrates how to implement `IDisposable` in a Blazor component:

```csharp
...
@using System;
@implements IDisposable
...

@functions {
    ...
    public void Dispose()
    {
        // Add code for disposing here
        ...
    }
}
```
