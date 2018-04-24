+++
title = "View and Logic"
weight = 40
lastModifierDisplayName = "rainer@software-architects.at"
date = 2018-03-28
+++

{{% notice note %}}
{{% siteparam "disclaimer" %}}
{{% /notice %}}

## Introduction

The default Blazor templates generate view logic code inside the Razor template using `@functions`. In the background, Blazor generates a single class containing C# code for generating the tree of view objects as well as the C# code representing the view logic.

```html
@page "/"

<!-- View (HTML) -->

@functions {
    // Logic (C#)
}
```

Many developers dislike mixing view and logic in a single file. In this article, we explore ways to separate view and logic.

## Dependency Injection

You should consider using application services and Blazor's dependency injection system to isolate logic that is independent of the view (e.g. core business logic, data access logic etc.). [Read more about dependency injection in Blazor...](../dependency-injection/).

## Partial Classes

At the time of writing, Blazor does *not* generate `partial` classes for its components ([related issue on GitHub](https://github.com/aspnet/Blazor/issues/278)). Therefore, you cannot just create a separate file and use the same class name as the component.

## Base Class

{{% notice note %}}
You can find the complete source code of the sample below [on GitHub](https://github.com/software-architects/learn-blazor/tree/master/samples/ViewLogicSeparation).
{{% /notice %}}

One way of separating view and logic is to create a base class. The base class contains all the view logic (C#). The Blazor component derives from this class and adds the view. Let us look at an example. Here is the view. Note that it does not have any code. It just uses properties and methods defined in its base class `InheritanceBase` (note the `@inherits` statement at the beginning of the file).

```html
@page "/"
@inherits InheritanceBase

<h1>Inheritance</h1>

<div class="filter">
    <label>Customer Filter:</label>
    <input type="text" id="customer-filter" bind=@CustomerFilter
           placeholder="Customer filter..."><br />
    <!-- Note that we show/hide button based on logic in the base class -->
    @if (CanGetCustomers)
    {
        <button onclick=@GetCustomers>Get Customer List...</button>
    }
</div>

<!-- Note that we show/hide result based on logic in the base class -->
@if (CustomersLoaded)
{
    <div class="result">
        <table>
            <thead>
                <tr>
                    <th>First Name</th>
                    <th>Last Name</th>
                </tr>
            </thead>
            <tbody>
                @foreach (var customer in Customers)
                {
                    <tr>
                        <td>@customer.FirstName</td>
                        <td>@customer.FirstName</td>
                    </tr>
                }
            </tbody>
        </table>
    </div>
}

<!-- Note that we do not have any code in the view -->
```

Now let us look the base class containing the view logic. Note that this class does also not contain any business or data access logic. It consists of *view logic* (i.e. logic that defines the *behavior* of the view, not its design). I added comments to make it easier for you to recognize the important concepts used in this example.

```cs
using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using ViewLogicSeparation.Logic;

namespace ViewLogicSeparation.Pages
{
    public class InheritanceBase : BlazorComponent
    {
        #region Injected properties
        // Note that Blazor's dependency injection works even if you use the
        // `InjectAttribute` in a component's base class.

        // Note that we decouple the component's base class from
        // the data access service using an interface.
        [Inject]
        protected IDataAccess DataAccess { get; set; }
        #endregion

        #region Properties used for data binding
        public IEnumerable<Customer> Customers { get; set; }

        public string CustomerFilter { get; set; }
        #endregion

        #region Status properties used to enable/disable/hide/show UI elements
        public bool CustomersLoaded => Customers != null;

        public bool IsInitialized => DataAccess != null;

        public bool CanGetCustomers => IsInitialized && !string.IsNullOrEmpty(CustomerFilter);
        #endregion

        // At the time of writing, `@onclickasync` does not exist. Therefore,
        // the return type has to be `void` instead of `Task`.
        public async void GetCustomers()
        {
            #region Check prerequisites
            if (DataAccess == null)
            {
                // This should never happen. This check should demonstrate that
                // `DataAccess` and `Http` are filled by Blazor's dependency injection.
                throw new InvalidOperationException("There is something wrong with DI");
            }

            if (!CanGetCustomers)
            {
                // This should never happen. The UI should prevent calling 
                // `GetCustomerAsync` if no customer filter has been set (e.g. by
                // disabling or hiding the button).
                throw new InvalidOperationException("Customer filter not set");
            }
            #endregion

            // Get the data using the injected data access service
            Customers = await DataAccess.GetCustomersAsync(CustomerFilter);

            // We have to manually call `StateHasChanged` because Blazor's `onclick`
            // does not yet support async handler methods.
            StateHasChanged();
        }
    }
}
```

To be continued...
