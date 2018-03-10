+++
title = "Dependency Injection"
weight = 5
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-09
+++

## Introduction

```cs
using System.Collections.Generic;
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

        public async Task<IReadOnlyList<Customer>> GetAllCustomersAsync()
        {
            // Simulate some long running, async work (e.g. web request)
            await Task.Delay(100);

            return Repository.Customers;
        }
    }
}
```

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
