using Nethereum.Siwe;
using Nethereum.Siwe.Core;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Nethereum.UI
{
    public class NethereumSiweAuthenticatorService
    {
        private readonly IEthereumHostProvider _host;
        private readonly SiweMessageService _siweMessageService;

        public NethereumSiweAuthenticatorService(IEthereumHostProvider host, ISessionStorage sessionStorage)
        {
            _host = host;
            _siweMessageService = new SiweMessageService(sessionStorage);
            
        }

        public string BuildMessageToSign(SiweMessage siweMessage)
        {
            return _siweMessageService.BuildMessageToSign(siweMessage);
        }

        public async Task<SiweMessage> RequestUserToSignAuthenticationMessageAsync(SiweMessage siweMessage)
        {
            if (!_host.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
            }

            var challenge = BuildMessageToSign(siweMessage);
            var signedMessage = await _host.SignMessageAsync(challenge);
            if (await _siweMessageService.IsMessageSignatureValid(siweMessage, signedMessage))
            {
                return siweMessage;
            }
            
            throw new Exception("SiweMessage signed with an invalid Address");
        }



    }
}
