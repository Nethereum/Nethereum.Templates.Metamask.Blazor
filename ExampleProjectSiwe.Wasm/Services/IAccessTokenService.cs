using System.Threading.Tasks;

namespace ExampleProjectSiwe.Wasm.Services;

public interface IAccessTokenService
{
    Task<string> GetAccessTokenAsync();

    Task SetAccessTokenAsync(string tokenValue);

    Task RemoveAccessTokenAsync();
}