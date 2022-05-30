
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace ExampleProjectSiwe.Wasm.Services
{
    public class LocalStorageAccessTokenService : IAccessTokenService
    {
        private readonly IJSRuntime _jsRuntime;
        public const string JWTTokenName = "jwt_token";
        public LocalStorageAccessTokenService(IJSRuntime jSRuntime)
        {
            _jsRuntime = jSRuntime;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            return await _jsRuntime.InvokeAsync<string>("localStorage.getItem", JWTTokenName);
        }

        public async Task SetAccessTokenAsync(string tokenValue)
        {
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", JWTTokenName, tokenValue);
        }

        public async Task RemoveAccessTokenAsync()
        {
            await _jsRuntime.InvokeAsync<string>("localStorage.removeItem", JWTTokenName);
        }
    }
}
