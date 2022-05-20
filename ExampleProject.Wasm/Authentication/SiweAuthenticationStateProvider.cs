using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using ExampleProject.Wasm.Services;
using Nethereum.Siwe.Core;
using Nethereum.UI;
using Nethereum.Util;

namespace ExampleProject.Wasm.Authentication
{
    public class User
    {
        public string EthereumAddress { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }

    public class SiweAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly SiweApiUserLoginService _siweUserLoginService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IEthereumHostProvider _ethereumHostProvider;

        public SiweAuthenticationStateProvider(SiweApiUserLoginService siweUserLoginService,
            IAccessTokenService accessTokenService, IEthereumHostProvider ethereumHostProvider)
        {
            _siweUserLoginService = siweUserLoginService;
            _accessTokenService = accessTokenService;
            _ethereumHostProvider = ethereumHostProvider;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            User currentUser = await GetUserAsync();

            if (currentUser != null && currentUser.EthereumAddress != null)
            {
                //create claimsPrincipal
                var claimsPrincipal = GetClaimsPrinciple(currentUser);
                return new AuthenticationState(claimsPrincipal);
            }
            else
            {
                await _accessTokenService.RemoveAccessTokenAsync();
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        public async Task Authenticate(string address)
        {
            if (!_ethereumHostProvider.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
            }

            var siweMessage = await _siweUserLoginService.GenerateNewSiweMessage(address);
            var signedMessage = await _ethereumHostProvider.SignMessageAsync(siweMessage);
            await Authenticate(SiweMessageParser.Parse(siweMessage), signedMessage);
        }

        public async Task Authenticate(SiweMessage siweMessage, string signature)
        {
            var authenticateResponse = await _siweUserLoginService.Authenticate(siweMessage, signature);
            if (authenticateResponse.Jwt != null && authenticateResponse.Address.IsTheSameAddress(siweMessage.Address))
            {
                await _accessTokenService.SetAccessTokenAsync(authenticateResponse.Jwt);
                await MarkUserAsAuthenticated();
            }
            else
            {
                throw new Exception("Invalid authentication response");
            }

        }

        public async Task<SiweMessage> GenerateNewSiweMessage(string adddress)
        { 
            var message = await _siweUserLoginService.GenerateNewSiweMessage(adddress);
            return SiweMessageParser.Parse(message);
        }

        public async Task MarkUserAsAuthenticated()
        {
            var user = await GetUserAsync();
            var claimsPrincipal = GetClaimsPrinciple(user);

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
                await _siweUserLoginService.Logout(token);
            }
            catch
            {
                // remove from the server but catch gracefully as the token is gone from the state
            }

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
        }

        public async Task<User> GetUserAsync()
        {
            //pulling the token from localStorage
            var jwtToken = await _accessTokenService.GetAccessTokenAsync();
            if (jwtToken == null) return null;

            return await _siweUserLoginService.GetUser(jwtToken);
        }

        private ClaimsPrincipal GetClaimsPrinciple(User currentUser)
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
