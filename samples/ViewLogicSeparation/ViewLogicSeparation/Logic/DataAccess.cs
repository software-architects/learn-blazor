using Microsoft.AspNetCore.Blazor.Components;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ViewLogicSeparation.Logic
{
    public interface IDataAccess
    {
        Task<IEnumerable<Customer>> GetCustomersAsync(string customerFilter);
    }

    public class DataAccess : IDataAccess
    {
        private HttpClient Http { get; set; }

        public DataAccess(HttpClient http)
        {
            // We use constructor injection here because the `InjectAttribute` only works
            // on Blazor components, not recursively on injected services.
            Http = http;
        }

        public async Task<IEnumerable<Customer>> GetCustomersAsync(string customerFilter)
        {
            #region Check prerequisites
            if (Http == null)
            {
                // This should never happen. This check should demonstrate that
                // `Http` is filled by Blazor's dependency injection mechanism.
                throw new InvalidOperationException("There is something wrong with DI");
            }

            if (customerFilter == null)
            {
                // No customer filter was given. 
                throw new ArgumentNullException(nameof(customerFilter));
            }
            #endregion

            // Simulate async data access. In practice, this would be
            // e.g. a call to a web api using `Http`. The parameter
            // `customerFilter` would be used in the call.
            await Task.Delay(300);

            // Simulate a returned result
            return new[]
            {
                new Customer { FirstName = "Bruce", LastName = "Wayne" },
                new Customer { FirstName = "Clark", LastName = "Kent" }
            };
        }
    }
}
