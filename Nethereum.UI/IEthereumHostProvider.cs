using System;
using System.Threading.Tasks;

namespace Nethereum.UI
{
    public interface IEthereumHostProvider
    {
        string Name { get; }

        bool Available { get; }
        string SelectedAccount { get;}
        int SelectedNetwork { get; }
        bool Enabled { get; }
        
        event Func<string, Task> SelectedAccountChanged;
        event Func<int, Task> NetworkChanged;
        event Func<bool, Task> AvailabilityChanged;
        event Func<bool, Task> EnabledChanged;

        Task<bool> CheckProviderAvailabilityAsync();
        Task<Web3.Web3> GetWeb3Async();
        Task<string> EnableProviderAsync();
        Task<string> GetProviderSelectedAccountAsync();
        Task<int> GetProviderSelectedNetworkAsync();
        Task<string> SignMessageAsync(string message);
    }
}