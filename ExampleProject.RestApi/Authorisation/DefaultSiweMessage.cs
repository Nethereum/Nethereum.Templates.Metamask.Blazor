using Nethereum.Siwe.Core;

namespace ExampleProject.RestApi.Authorisation
{
    public class DefaultSiweMessage: SiweMessage
    {
        public DefaultSiweMessage()
        {
            this.Domain = "login.xyz"; 
            this.Statement = "Sign-In With Ethereum ExampleProject";
            this.Uri = "https://login.xyz";
            this.Version = "1";
            this.ChainId = "1";
        }
    }
}
