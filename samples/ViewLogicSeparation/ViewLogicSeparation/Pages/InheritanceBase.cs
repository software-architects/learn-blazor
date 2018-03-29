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
