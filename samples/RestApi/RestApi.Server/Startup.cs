using Microsoft.AspNetCore.Blazor.Server;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Serialization;
using RestApi.Server.Data;
using System.Linq;
using System.Net.Mime;

namespace RestApi.Server
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            // Use Entity Framework in-memory provider for this sample
            services.AddDbContext<CustomerContext>(options => options.UseInMemoryDatabase("Customers"));

            services.AddMvc().AddJsonOptions(options =>
            {
                options.SerializerSettings.ContractResolver = new DefaultContractResolver();
            });

            services.AddResponseCompression(options =>
            {
                options.MimeTypes = ResponseCompressionDefaults.MimeTypes
                    .Concat(new[] { MediaTypeNames.Application.Octet, WasmMediaTypeNames.Application.Wasm });
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();

            app.UseBlazor<Client.Startup>();
        }
    }
}
