using System.Threading.Tasks;

namespace ExampleProject.Wasm.Services;

public interface IAccessTokenService
{
    Task<string> GetAccessTokenAsync();

    Task SetAccessTokenAsync(string tokenValue);

    Task RemoveAccessTokenAsync();
}