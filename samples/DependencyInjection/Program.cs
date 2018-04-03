using DependencyInjection.Pages;
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
                configure.Add(ServiceDescriptor.Transient<IRepository, Repository>());
                configure.Add(ServiceDescriptor.Transient<MyTransientService, MyTransientService>());
                configure.Add(ServiceDescriptor.Scoped<MyScopedService, MyScopedService>());
                configure.Add(ServiceDescriptor.Singleton<MySingletonService, MySingletonService>());
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
