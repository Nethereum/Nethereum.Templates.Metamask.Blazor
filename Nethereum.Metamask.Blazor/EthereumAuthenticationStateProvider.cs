using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Authorization;
using Nethereum.UI;
using System.Security.Claims;

namespace Nethereum.Metamask.Blazor
{
    public class EthereumAuthenticationStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly IEthereumHostProvider _ethereumHostProvider;

        public EthereumAuthenticationStateProvider(IEthereumHostProvider ethereumHostProvider)
        {
            _ethereumHostProvider = ethereumHostProvider;
            _ethereumHostProvider.SelectedAccountChanged += SelectedAccountChanged;
        }

        private async Task SelectedAccountChanged(string ethereumAddress)
        {
            if(string.IsNullOrEmpty(ethereumAddress))
            {
                await NotifyAuthenticationStateAsEthereumDisconnected();
            }
            else
            {
                await NotifyAuthenticationStateAsEthereumConnected(ethereumAddress);
            }
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            if (_ethereumHostProvider.Available)
            {
                var currentAddress = await _ethereumHostProvider.GetProviderSelectedAccountAsync();
                if (currentAddress != null)
                {
                    var claimsPrincipal = GetClaimsPrincipal(currentAddress);
                    return new AuthenticationState(claimsPrincipal);
                }
            }
           
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
           
        }

        public async Task NotifyAuthenticationStateAsEthereumConnected()
        {
            var currentAddress = await _ethereumHostProvider.GetProviderSelectedAccountAsync();
            await NotifyAuthenticationStateAsEthereumConnected(currentAddress);
        }

        public async Task NotifyAuthenticationStateAsEthereumConnected(string currentAddress)
        {
            var claimsPrincipal = GetClaimsPrincipal(currentAddress);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task NotifyAuthenticationStateAsEthereumDisconnected()
        {
           
            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        private ClaimsPrincipal GetClaimsPrincipal(string ethereumAddress)
        {
            var claimEthereumAddress = new Claim(ClaimTypes.NameIdentifier, ethereumAddress);
            var claimEthereumConnectedRole = new Claim(ClaimTypes.Role, "EthereumConnected");

            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimEthereumAddress, claimEthereumConnectedRole }, "ethereumConnection");
            //create claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }

        public void Dispose()
        {
            _ethereumHostProvider.SelectedAccountChanged -= SelectedAccountChanged;
        }
    }
}