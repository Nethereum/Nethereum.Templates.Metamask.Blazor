using Microsoft.AspNetCore.Components.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Nethereum.Metamask.Blazor
{
    public class Startup
    {
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSingleton<IMetamaskInterop, MetamaskBlazorInterop>();
            services.AddSingleton<MetamaskService>();
            services.AddSingleton<MetamaskInterceptor>();
        }

        public void Configure(IComponentsApplicationBuilder app)
        {
            app.AddComponent<App>("app");
        }
    }
}
