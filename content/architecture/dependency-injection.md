+++
title = "Dependency Injection"
weight = 10
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-15
+++

## Introduction

Blazor has dependency injection built in. Take a look at the following simplified code sample. It defines a repository (e.g. a database) from which we can get customer data asynchronously. In order to decouple the user of the repository (e.g. Blazor component) from the implementation (`Repository` class shown below), we introduce an interface (`IRepository`). The decoupling enables us to use different repository implementations e.g. for unit tests.

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
        public Repository(HttpClient client)
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

## Setting up the Service Provider

We have to add the available application-level services in the program's `Main` method. When we create the `BrowserRenderer` ([more about *renderers*](/getting-started/what-is-blazor/#renderer)) to bootstrap our Blazor app, it expects a *service provider* (`System.IServiceProvider`). Blazor comes with a service provider specifically for the browser (`Microsoft.AspNetCore.Blazor.Browser.Services.BrowserServiceProvider`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/dev/src/Microsoft.AspNetCore.Blazor/Services/BrowserServiceProvider.cs)). It accepts a configuration functions in which we have to add services to Blazor's dependency injection system. If you are familiar with ASP.NET Core, the API should feel quite natural for

```cs
using Architecture.Services;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Architecture
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.Add(ServiceDescriptor.Singleton<IRepository, Repository>());
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
```

## Injecting Services

To inject a service in a component, use the `@inject` keyword as shown in the following sample. Technically, this generates a property with the given name and type. The property is decorated with the attribute `Microsoft.AspNetCore.Blazor.Components.InjectAttribute` so that Blazor's component factory (`Microsoft.AspNetCore.Blazor.Components.ComponentFactory`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/dev/src/Microsoft.AspNetCore.Blazor/Components/ComponentFactory.cs)) knows that it has to fill it when creating the component.

```cs
@using Architecture.Services
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

## Default Services

At the time of writing, Blazor offers two default services that you can request via dependency injection:

* `System.Net.Http.HttpClient` - HTTP client that you can use to access local web APIs. Note that the `BaseAddress` is already set on this HTTP client.

* `Microsoft.AspNetCore.Blazor.Services.IUriHelper` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/dev/src/Microsoft.AspNetCore.Blazor/Services/IUriHelper.cs) - helpers for working with URIs and navigation state
