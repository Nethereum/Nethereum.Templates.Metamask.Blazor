using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Nethereum.Siwe.Core;
using Nethereum.UI;
using Nethereum.Util;
using Nethereum.Blazor;
using ExampleProjectSiwe.Server.Services;

namespace ExampleProjectSiwe.Server.Authentication
{
    public class SiweAuthenticationServerStateProvider<TUser> : EthereumAuthenticationStateProvider where TUser : User
    {
        private readonly NethereumSiweAuthenticatorService nethereumSiweAuthenticatorService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IUserService<TUser> _userService;

        public SiweAuthenticationServerStateProvider(NethereumSiweAuthenticatorService nethereumSiweAuthenticatorService,
            IAccessTokenService accessTokenService, SelectedEthereumHostProviderService selectedHostProviderService, IUserService<TUser> userService) : base(selectedHostProviderService)
        {

            this.nethereumSiweAuthenticatorService = nethereumSiweAuthenticatorService;
            _accessTokenService = accessTokenService;
            _userService = userService;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var currentUser = await GetUserAsync();

            if (currentUser != null && currentUser.EthereumAddress != null)
            {
                var claimsPrincipal = GenerateSiweClaimsPrincipal(currentUser);
                return new AuthenticationState(claimsPrincipal);
            }
            await _accessTokenService.RemoveAccessTokenAsync();
            return await base.GetAuthenticationStateAsync();
        }

        public async Task AuthenticateAsync(string address = null)
        {
            
            if (EthereumHostProvider == null  || !EthereumHostProvider.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
            }

            if (string.IsNullOrEmpty(address))
            {
                address = await EthereumHostProvider.GetProviderSelectedAccountAsync();
            }
            var siweMessage = new DefaultSiweMessage();
            siweMessage.Address = address.ConvertToEthereumChecksumAddress();
            siweMessage.SetExpirationTime(DateTime.Now.AddMinutes(10));
            siweMessage.SetNotBefore(DateTime.Now);
            var fullMessage = await nethereumSiweAuthenticatorService.AuthenticateAsync(siweMessage);
            await _accessTokenService.SetAccessTokenAsync(SiweMessageStringBuilder.BuildMessage(fullMessage));
            await MarkUserAsAuthenticated();
        }


        public async Task MarkUserAsAuthenticated()
        {
            var user = await GetUserAsync();
            var claimsPrincipal = GenerateSiweClaimsPrincipal(user);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task LogOutUserAsync()
        {
            var token = await _accessTokenService.GetAccessTokenAsync();
            await _accessTokenService.RemoveAccessTokenAsync();
            try
            {
                var siweMessage = SiweMessageParser.Parse(token);
                nethereumSiweAuthenticatorService.LogOut(siweMessage);
            }
            catch
            {
                // remove from the server but catch gracefully as the token is gone from the state
            }
            //retrieving the overall authentication state
            var authenticationState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        }

        public async Task<TUser> GetUserAsync()
        {
            var token = await _accessTokenService.GetAccessTokenAsync();
            if (token == null) return null;
            var siweMessage = SiweMessageParser.Parse(token);
            return await _userService.GetUserAsync(siweMessage.Address);
        }

        private ClaimsPrincipal GenerateSiweClaimsPrincipal(User currentUser)
        {
            //create a claims
            var claimName = new Claim(ClaimTypes.Name, currentUser.UserName);
            var claimEthereumAddress = new Claim(ClaimTypes.NameIdentifier, currentUser.EthereumAddress);
            var claimEthereumConnectedRole = new Claim(ClaimTypes.Role, "EthereumConnected");
            var claimSiweAuthenticatedRole = new Claim(ClaimTypes.Role, "SiweAuthenticated");

            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimEthereumAddress, claimName, claimEthereumConnectedRole, claimSiweAuthenticatedRole }, "siweAuth");
            //create claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }
    }
}
