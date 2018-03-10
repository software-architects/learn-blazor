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
