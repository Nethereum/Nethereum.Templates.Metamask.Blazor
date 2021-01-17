using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nethereum.UI;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Metamask.Blazor;
using Nethereum.Metamask;
using FluentValidation;

namespace ExampleProject.Wasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
            builder.Services.AddSingleton<MetamaskInterceptor>();
            builder.Services.AddSingleton<MetamaskHostProvider>();
            builder.Services.AddSingleton<IEthereumHostProvider>(serviceProvider =>
            {
                return serviceProvider.GetService<MetamaskHostProvider>();
            });
            builder.Services.AddSingleton<NethereumAuthenticator>();
            builder.Services.AddValidatorsFromAssemblyContaining<Nethereum.Erc20.Blazor.Erc20Transfer>();

            await builder.Build().RunAsync();
        }
    }
}
