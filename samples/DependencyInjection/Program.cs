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
            var serviceProvider = new BrowserServiceProvider(services =>
            {
                services.AddTransient<IRepository, Repository>();
                services.AddTransient<MyTransientService, MyTransientService>();
                services.AddScoped<MyScopedService, MyScopedService>();
                services.AddSingleton<MySingletonService, MySingletonService>();
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
