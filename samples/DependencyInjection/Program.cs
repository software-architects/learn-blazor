using DependencyInjection.Services;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection
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
