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
using Microsoft.AspNetCore.Components.Authorization;
using Nethereum.Siwe;
using ExampleProjectSiwe.Wasm.Authentication;
using ExampleProjectSiwe.Wasm.Services;

namespace ExampleProjectSiwe.Wasm
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.Services.AddAuthorizationCore();

            //builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddSingleton(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5048") });

            var inMemorySessionNonceStorage = new InMemorySessionNonceStorage();
            builder.Services.AddSingleton<ISessionStorage>(x => inMemorySessionNonceStorage);
            builder.Services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
            builder.Services.AddSingleton<MetamaskInterceptor>();
            builder.Services.AddSingleton<MetamaskHostProvider>();
            //Add metamask as the selected ethereum host provider
            builder.Services.AddSingleton(services =>
            {
                var metamaskHostProvider = services.GetService<MetamaskHostProvider>();
                var selectedHostProvider = new SelectedEthereumHostProviderService();
                selectedHostProvider.SetSelectedEthereumHostProvider(metamaskHostProvider);
                return selectedHostProvider;
            });

            builder.Services.AddSingleton<NethereumSiweAuthenticatorService>();
            builder.Services.AddSingleton<IAccessTokenService, LocalStorageAccessTokenService>();
            builder.Services.AddSingleton<SiweApiUserLoginService<User>>();
            builder.Services.AddSingleton<AuthenticationStateProvider, SiweAuthenticationWasmStateProvider<User>>();

            builder.Services.AddValidatorsFromAssemblyContaining<Nethereum.Erc20.Blazor.Erc20Transfer>();

            await builder.Build().RunAsync();
        }
    }
}
