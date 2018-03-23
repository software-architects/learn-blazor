+++
title = "Consuming REST APIs"
weight = 20
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-15
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

Developers who are used to writing C# code in ASP.NET will find it very simple to consume web APIs with Blazor. All the usual classes (e.g. `System.Net.Http.HttpClient`) and language constructs (e.g. `async` and `await`) are available. Therefore, a reading data from a server and printing it in the console looks like this in Blazor:

```cs
@inject HttpClient Http

<button @onclick(async () => await PrintWebApiResponse())>Print Web API Response</button>

@functions {
    private async Task PrintWebApiResponse()
    {
        var response = await Http.GetStringAsync("/api/Customer");
        Console.WriteLine(response);
    }
}
```

Note that we are using [dependency injection](dependency-injection) to get an instance of the `System.Net.Http.HttpClient` class. Once we have it, it is business as usual.

## What Happens in the Background

The browser's firewall would not allow Blazor to directly do network communication. All network traffic has to go through the browser. Therefore, Blazor's C# implementation has to pass our HTTP request to Blazor's JavaScript-part. How is that possible if we use C# usual `System.Net.Http.HttpClient` class?

The solution is the abstract base class `System.Net.Http.HttpMessageHandler`. It represents an abstraction layer for sending HTTP requests as an asynchronous operation. `HttpClient` takes an instance of this class in one of its constructor. Blazor contains an implementation of `HttpMessageHandler` that hands over HTTP requests to JavaScript (`Microsoft.AspNetCore.Blazor.Browser.Http.BrowserHttpMessageHandler`, see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser/Http/BrowserHttpMessageHandler.cs)). If you get your `HttpClient` using Blazor's dependency injection, you will get an instance that is already wired up with `BrowserHttpMessageHandler`.

The real HTTP request is sent using the browser's [*fetch* API](https://developer.mozilla.org/en-US/docs/Web/API/WindowOrWorkerGlobalScope/fetch) (see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor.Browser.JS/src/Services/Http.ts)).

## What About JSON

Blazor comes with [*SimpleJson*](https://github.com/facebook-csharp-sdk/simple-json) embedded (source code is part of the Blazor DLLs, not a referenced NuGet package), a JSON library for portable .NET apps. The static class `Microsoft.AspNetCore.Blazor.HttpClientJsonExtensions` (see [source on GitHub](https://github.com/aspnet/Blazor/blob/release/0.1.0/src/Microsoft.AspNetCore.Blazor/Json/HttpClientJsonExtensions.cs)) contains extensions methods for `HttpClient` that make it easier to consume JSON-based web APIs in Blazor.

## Sample

The following sample demonstrates the use of a RESTful Web API implemented with ASP.NET Core and Entity Framework. You can find the complete sample [on GitHub](https://github.com/software-architects/learn-blazor/tree/master/samples/RestApi).

```cs
@using RestApi.Shared
@inject HttpClient Http

<h1>Customer List</h1>

<p>This component demonstrates fetching data from the server.</p>

<button @onclick(async () => await PrintWebApiResponse())>Print Web API Response</button>
<button @onclick(async () => await FillWithDemoData())>Fill with demo data</button>
<button @onclick(async () => await DeleteAllCustomers())>Delete all customers</button>

@if (Customers == null)
{
    <p><em>Loading...</em></p>
}
else
{
    <table class='table'>
        <thead>
            <tr>
                <th>ID</th>
                <th>First Name</th>
                <th>Last Name</th>
                <th>Department</th>
            </tr>
        </thead>
        <tbody>
            @foreach (var customer in Customers)
            {
                <tr>
                    <td>@customer.ID</td>
                    <td>@customer.FirstName</td>
                    <td>@customer.LastName</td>
                    <td>@customer.Department</td>
                </tr>
            }
        </tbody>
    </table>
}
@functions {
    private Customer[] Customers { get; set; }

    protected override async Task OnInitAsync()
    {
        await RefreshCustomerList();
    }

    private async Task RefreshCustomerList()
    {
        Customers = await Http.GetJsonAsync<Customer[]>("/api/Customer");
        StateHasChanged();
    }

    private async Task FillWithDemoData()
    {
        for (var i = 0; i < 10; i++)
        {
            await Http.SendJsonAsync(HttpMethod.Post, "/api/Customer", new Customer
            {
                FirstName = "Tom",
                LastName = $"Customer {i}",
                Department = i % 2 == 0 ? "Sales" : "Research"
            });
        }

        await RefreshCustomerList();
    }

    private async Task DeleteAllCustomers()
    {
        foreach (var c in Customers)
        {
            await Http.DeleteAsync($"/api/Customer/{c.ID}");
        }

        await RefreshCustomerList();
    }

    private async Task PrintWebApiResponse()
    {
        var response = await Http.GetStringAsync("/api/Customer");
        Console.WriteLine(response);
    }
}
```
