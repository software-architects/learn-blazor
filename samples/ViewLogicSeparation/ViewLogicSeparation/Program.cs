using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;
using System;
using ViewLogicSeparation.Logic;

namespace ViewLogicSeparation
{
    class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(configure =>
            {
                configure.Add(ServiceDescriptor.Singleton<IDataAccess, DataAccess>());
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
