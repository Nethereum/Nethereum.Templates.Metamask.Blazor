
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace ExampleProjectSiwe.Server.Services
{
    public class ProtectedSessionStorageAccessTokenService : IAccessTokenService
    {
        private readonly ProtectedSessionStorage _protectedSessionStore;
        private const string TokenName = "SiweToken";
        public ProtectedSessionStorageAccessTokenService(ProtectedSessionStorage protectedSessionStore)
        {
            _protectedSessionStore = protectedSessionStore;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var result = await _protectedSessionStore.GetAsync<string>(TokenName);
            if (result.Success)
            {
                return result.Value;
            }
            return null;
        }

        public async Task SetAccessTokenAsync(string tokenValue)
        {
            await _protectedSessionStore.SetAsync(TokenName, tokenValue);
        }

        public async Task RemoveAccessTokenAsync()
        {
            await _protectedSessionStore.DeleteAsync(TokenName);
        }
    }
}
