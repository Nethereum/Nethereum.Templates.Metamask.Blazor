using System.Net.Http.Headers;

namespace ExampleProjectSiwe.Server.Authentication
{
    public class User
    {
        public string EthereumAddress { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
