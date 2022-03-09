using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;
using Nethereum.Siwe.Core;
using Nethereum.Util;

namespace Nethereum.UI
{
    public class NethereumSiweAuthenticatorService
    {
        private readonly IEthereumHostProvider _host;

        public NethereumSiweAuthenticatorService(IEthereumHostProvider host)
        {
            _host = host;
        }

        protected string CurrentNonce { get; set; }

        public string BuildMessageToSign(SiweMessage siweMessage)
        {
            if (string.IsNullOrEmpty(siweMessage.IssuedAt))
            {
                siweMessage.IssuedAt = DateTime.UtcNow.ToString("o");
            }

            if (string.IsNullOrEmpty(siweMessage.Version))
            {
                siweMessage.Version = "1";
            }
            BuildNewNonce(siweMessage);
            CurrentNonce = siweMessage.Nonce;
            return SiweMessageStringBuilder.BuildMessage(siweMessage);

        }

        public virtual void BuildNewNonce(SiweMessage siweMessage)
        {
            if (string.IsNullOrEmpty(siweMessage.Nonce))
            {
                var currentChallenge = DateTime.Now.ToString("O") + "- Challenge";
                //creating a random key, hashing, signing and hashing again to be truly random
                var key = EthECKey.GenerateKey();
                var currentKey = key.GetPrivateKey(); // random key to sign message
                var signer = new MessageSigner();
                siweMessage.Nonce = Util.Sha3Keccack.Current.CalculateHash(signer.HashAndSign(currentChallenge, currentKey));
            }
        }


        public async Task<SiweMessage> RequestUserToSignAuthenticationMessageAsync(SiweMessage siweMessage)
        {
            if (!_host.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
            }

            var challenge = BuildMessageToSign(siweMessage);
            var signedMessage = await _host.SignMessageAsync(challenge);
            siweMessage.Signature = signedMessage;
            var recoveredAddress = new EthereumMessageSigner().EncodeUTF8AndEcRecover(challenge, signedMessage);
            if (!string.IsNullOrEmpty(recoveredAddress) && recoveredAddress.IsTheSameAddress(siweMessage.Address))
            {
                return siweMessage;
            }
            else
            {
                throw new Exception("SiweMessage signed with an invalid Address");
            }
            
        }


    }
}
