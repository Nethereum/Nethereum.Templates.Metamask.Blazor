using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Nethereum.Siwe.Core;
using Nethereum.UI;
using Nethereum.Util;
using ExampleProject.Server.Services;

namespace ExampleProject.Server.Authentication
{
    public class User
    {
        public string EthereumAddress { get; set; }
        public string UserName { get; set; } = "Vitalik";
        public string Email { get; set; } = "test@ExampleProject.com";
    }

    public class SiweAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly NethereumSiweAuthenticatorService nethereumSiweAuthenticatorService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IEthereumHostProvider _ethereumHostProvider;

        public SiweAuthenticationStateProvider(NethereumSiweAuthenticatorService nethereumSiweAuthenticatorService,
            IAccessTokenService accessTokenService, IEthereumHostProvider ethereumHostProvider)
        {
 
            this.nethereumSiweAuthenticatorService = nethereumSiweAuthenticatorService;
            _accessTokenService = accessTokenService;
            _ethereumHostProvider = ethereumHostProvider;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            User currentUser = await GetUserAsync();

            if (currentUser != null && currentUser.EthereumAddress != null)
            {
                //create claimsPrincipal
                var claimsPrincipal = GetClaimsPrincipal(currentUser);
                return new AuthenticationState(claimsPrincipal);
            }
            else
            {
                await _accessTokenService.RemoveAccessTokenAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task AuthenticateAsync(string address)
        {
            if (!_ethereumHostProvider.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
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
            var claimsPrincipal = GetClaimsPrincipal(user);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task LogOutUser()
        {
            var token = await _accessTokenService.GetAccessTokenAsync();
            await _accessTokenService.RemoveAccessTokenAsync();

            var identity = new ClaimsIdentity();
            var user = new ClaimsPrincipal(identity);
            try
            {
                var siweMessage = SiweMessageParser.Parse(token);
                nethereumSiweAuthenticatorService.LogOut(siweMessage);
            }
            catch
            {
                // remove from the server but catch gracefully as the token is gone from the state
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task<User> GetUserAsync()
        {
            var token = await _accessTokenService.GetAccessTokenAsync();
            if (token == null) return null;
            var siweMessage = SiweMessageParser.Parse(token);
            //This needs to go to a service.. 
            return new User() { EthereumAddress = siweMessage.Address };

        }
       

        private ClaimsPrincipal GetClaimsPrincipal(User currentUser)
        {
            //create a claims
            var claimNameIdentifier = new Claim(ClaimTypes.Name, currentUser.UserName);
            var claimEmailAddress = new Claim(ClaimTypes.NameIdentifier, Convert.ToString(currentUser.Email));
            //var claimRole = new Claim(ClaimTypes.Role, currentUser.Role == null ? "" : currentUser.Role);

            //create claimsIdentity
            var claimsIdentity = new ClaimsIdentity(new[] { claimEmailAddress, claimNameIdentifier }, "serverAuth");
            //create claimsPrincipal
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            return claimsPrincipal;
        }


    }
}
