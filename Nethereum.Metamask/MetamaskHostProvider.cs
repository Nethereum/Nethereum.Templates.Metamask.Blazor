using System;
using System.Threading.Tasks;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.UI;

namespace Nethereum.Metamask
{
    public class MetamaskHostProvider: IEthereumHostProvider
    {
        private readonly IMetamaskInterop _metamaskInterop;
        public static MetamaskHostProvider Current { get; private set; }
        public string Name { get; } = "Metamask";
        public bool Available { get; private set; }
        public string SelectedAccount { get; private set; }
        public int SelectedNetwork { get; }
        public bool Enabled { get; private set; }

        private MetamaskInterceptor _metamaskInterceptor;

        public event Func<string, Task> SelectedAccountChanged;
        public event Func<int, Task> NetworkChanged;
        public event Func<bool, Task> AvailabilityChanged;
        public event Func<bool, Task> EnabledChanged;
        public async Task<bool> CheckProviderAvailabilityAsync()
        {
            var result = await _metamaskInterop.CheckMetamaskAvailability();
            await ChangeMetamaskAvailableAsync(result);
            return result;
        }

       

        public Task<Web3.Web3> GetWeb3Async()
        {
            var web3 = new Nethereum.Web3.Web3 {Client = {OverridingRequestInterceptor = _metamaskInterceptor}};
            return Task.FromResult(web3);
        }

        public async Task<string> EnableProviderAsync()
        {
            var selectedAccount = await _metamaskInterop.EnableEthereumAsync();
            Enabled = !string.IsNullOrEmpty(selectedAccount);

            if (Enabled)
            {
                SelectedAccount = selectedAccount;
                if (SelectedAccountChanged != null)
                {
                    await SelectedAccountChanged.Invoke(selectedAccount);
                }
                return selectedAccount;
            }

            return null;
        }

        public async Task<string> GetProviderSelectedAccountAsync()
        {
            var result = await _metamaskInterop.GetSelectedAddress();
            await ChangeSelectedAccountAsync(result);
            return result;
        }

        public Task<int> GetProviderSelectedNetworkAsync()
        {
            throw new NotImplementedException();
        }

        public async Task<string> SignMessageAsync(string message)
        {
            return await _metamaskInterop.SignAsync(message.ToHexUTF8());
        }

        public MetamaskHostProvider(IMetamaskInterop metamaskInterop)
        {
            _metamaskInterop = metamaskInterop;
            _metamaskInterceptor = new MetamaskInterceptor(_metamaskInterop, this);
            Current = this;
        }
        
        public async Task ChangeSelectedAccountAsync(string selectedAccount)
        {
            if (SelectedAccount != selectedAccount)
            {
                SelectedAccount = selectedAccount;
                if (SelectedAccountChanged != null)
                {
                    await SelectedAccountChanged.Invoke(selectedAccount);
                }
            }
        }

        public async Task ChangeMetamaskAvailableAsync(bool available)
        {
            Available = available;
            if (AvailabilityChanged != null)
            {
                await AvailabilityChanged.Invoke(available);
            }
        }

    }
}
