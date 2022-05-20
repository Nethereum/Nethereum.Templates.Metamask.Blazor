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
using ExampleProject.Wasm.Authentication;
using ExampleProject.Wasm.Services;
using Nethereum.Metamask.Blazor;
using Nethereum.Metamask;
using FluentValidation;
using Microsoft.AspNetCore.Components.Authorization;
using Nethereum.Siwe;

namespace ExampleProject.Wasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddAuthorizationCore();

            //builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5047") });

            var inMemorySessionNonceStorage = new InMemorySessionNonceStorage();
            builder.Services.AddSingleton<ISessionStorage>(x => inMemorySessionNonceStorage);
            builder.Services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
            builder.Services.AddSingleton<MetamaskInterceptor>();
            builder.Services.AddSingleton<MetamaskHostProvider>();
            builder.Services.AddSingleton<IEthereumHostProvider>(serviceProvider =>
            {
                return serviceProvider.GetService<MetamaskHostProvider>();
            });
            builder.Services.AddSingleton<NethereumSiweAuthenticatorService>();
            builder.Services.AddSingleton<IAccessTokenService, LocalStorageAccessTokenService>();
            builder.Services.AddSingleton<SiweApiUserLoginService>();
            builder.Services.AddSingleton<AuthenticationStateProvider, SiweAuthenticationStateProvider>();

            builder.Services.AddValidatorsFromAssemblyContaining<Nethereum.Erc20.Blazor.Erc20Transfer>();

            await builder.Build().RunAsync();
        }
    }
}
