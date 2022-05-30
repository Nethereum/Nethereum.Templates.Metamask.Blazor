using System.Threading.Tasks;

namespace ExampleProject.Server.Services;

public interface IAccessTokenService
{
    Task<string> GetAccessTokenAsync();

    Task SetAccessTokenAsync(string tokenValue);

    Task RemoveAccessTokenAsync();
}