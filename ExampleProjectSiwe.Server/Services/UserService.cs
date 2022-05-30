using ExampleProjectSiwe.Server.Authentication;
using System.Threading.Tasks;

namespace ExampleProjectSiwe.Server.Services
{
    public class UserService<TUser> : IUserService<TUser> where TUser : User
    {
        public Task<TUser> GetUserAsync(string address)
        {
            var user = new User()
            {
                UserName = "Vitalik",
                Email = "test@ExampleProject.com",
                EthereumAddress = address
            };
            return Task.FromResult((TUser)user);
        }
    }
}
