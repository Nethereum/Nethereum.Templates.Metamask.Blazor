using ExampleProjectSiwe.Server.Authentication;
using System.Threading.Tasks;

namespace ExampleProjectSiwe.Server.Services
{
    public interface IUserService<TUser> where TUser : User
    {
        Task<TUser> GetUserAsync(string address);
    }
}