using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Nethereum.Signer;

namespace Nethereum.UI
{
    public class NethereumAuthenticator
    {
        private readonly IEthereumHostProvider _host;

        public NethereumAuthenticator(IEthereumHostProvider host)
        {
            _host = host;
        }
        private string _currentChallenge;
        private readonly string _prefix = "Please sign this one time message to Authenticate: ";

        public string GetNewChallenge(string message = null)
        {
            if (message == null) message = _prefix;
            var currentChallenge = DateTime.Now.ToString("O") + "- Challenge";
            var key = EthECKey.GenerateKey();
            var currentKey = key.GetPrivateKey(); // random key to sign message
            var signer = new MessageSigner();
            _currentChallenge =  signer.HashAndSign(currentChallenge, currentKey);
            return message + _currentChallenge;
        }

        public async Task<string> RequestNewChallengeSignatureAndRecoverAccountAsync(string message = null)
        {
            if (_host.Available)
            {
                throw new Exception("Cannot authenticate user, an Ethereum host is not available");
            }

            var challenge = GetNewChallenge(message);
            var signedMessage = await _host.SignMessageAsync(challenge);

            return new EthereumMessageSigner().EncodeUTF8AndEcRecover(challenge, signedMessage);
        }

    }
}
