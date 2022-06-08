using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using Nethereum.Siwe.Core;
using Nethereum.UI;
using Nethereum.Util;
using Nethereum.Blazor;
using ExampleProjectSiwe.Wasm.Services;

namespace ExampleProjectSiwe.Wasm.Authentication
{

    public class SiweAuthenticationWasmStateProvider<TUser> : EthereumAuthenticationStateProvider where TUser : User
    {
        private readonly SiweApiUserLoginService<TUser> _siweUserLoginService;
        private readonly IAccessTokenService _accessTokenService;
 

        public SiweAuthenticationWasmStateProvider(SiweApiUserLoginService<TUser> siweUserLoginService,
            IAccessTokenService accessTokenService, SelectedEthereumHostProviderService selectedHostProviderService) : base(selectedHostProviderService)
        {
            _siweUserLoginService = siweUserLoginService;
            _accessTokenService = accessTokenService;
        }

        public async override Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var currentUser = await GetUserAsync();

            if (currentUser != null && currentUser.EthereumAddress != null)
            {
                //create claimsPrincipal
                var claimsPrincipal = GenerateSiweClaimsPrincipal(currentUser);
                return new AuthenticationState(claimsPrincipal);
            }
            else
            {
                await _accessTokenService.RemoveAccessTokenAsync();
                return await base.GetAuthenticationStateAsync();
            }
        }

        public async Task AuthenticateAsync(string address)
        {
            if (EthereumHostProvider == null || !EthereumHostProvider.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
            }

            var siweMessage = await _siweUserLoginService.GenerateNewSiweMessage(address);
            var signedMessage = await EthereumHostProvider.SignMessageAsync(siweMessage);
            await AuthenticateAsync(SiweMessageParser.Parse(siweMessage), signedMessage);
        }

        public async Task AuthenticateAsync(SiweMessage siweMessage, string signature)
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
            var claimsPrincipal = GenerateSiweClaimsPrincipal(user);

            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        public async Task LogOutUserAsync()
        {
            var token = await _accessTokenService.GetAccessTokenAsync();
            await _accessTokenService.RemoveAccessTokenAsync();
            try
            {
                await _siweUserLoginService.Logout(token);
            }
            catch
            {
                // remove from the server but catch gracefully as the token is gone from the state
            }

            var authenticationState = await GetAuthenticationStateAsync();
            NotifyAuthenticationStateChanged(Task.FromResult(authenticationState));
        }

        public async Task<TUser> GetUserAsync()
        {
            var jwtToken = await _accessTokenService.GetAccessTokenAsync();
            if (jwtToken == null) return null;
            return await _siweUserLoginService.GetUser(jwtToken);
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
