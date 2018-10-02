using DependencyInjection.Pages;
using DependencyInjection.Services;
using Microsoft.AspNetCore.Blazor.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace DependencyInjection
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddTransient<IRepository, Repository>();
            services.AddTransient<MyTransientService, MyTransientService>();
            services.AddScoped<MyScopedService, MyScopedService>();
            services.AddSingleton<MySingletonService, MySingletonService>();
        }

        public void Configure(IBlazorApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
