using System.Threading.Tasks;
using Microsoft.JSInterop;
using Nethereum.JsonRpc.Client.RpcMessages;
using Newtonsoft.Json;

namespace Nethereum.Metamask.Blazor
{
    public class MetamaskBlazorInterop : IMetamaskInterop
    {
        private readonly IJSRuntime _jsRuntime;

        public MetamaskBlazorInterop(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }
        public async ValueTask<bool> EnableEthereumAsync()
        {
            return await _jsRuntime.InvokeAsync<bool>("NethereumMetamaskInterop.EnableEthereum");
        }

        public async ValueTask<bool> CheckMetamaskAvailability()
        {
            return await _jsRuntime.InvokeAsync<bool>("NethereumMetamaskInterop.IsMetamaskAvailable");
        }

        public async ValueTask<RpcResponseMessage> SendAsync(RpcRequestMessage rpcRequestMessage)
        {
            var response = await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.Send", JsonConvert.SerializeObject(rpcRequestMessage));
            return JsonConvert.DeserializeObject<RpcResponseMessage>(response);
        }

        public async ValueTask<RpcResponseMessage> SendTransactionAsync(MetamaskRpcRequestMessage rpcRequestMessage)
        {
            var response = await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.Send", JsonConvert.SerializeObject(rpcRequestMessage));
            return JsonConvert.DeserializeObject<RpcResponseMessage>(response);
        }

        public async ValueTask<string> GetSelectedAddress()
        {
            return await _jsRuntime.InvokeAsync<string>("NethereumMetamaskInterop.GetSelectedAddress");
        }


        [JSInvokable()]
        public static async Task MetamaskAvailableChanged(bool available)
        {
            await MetamaskService.Current.ChangeMetamaskAvailableAsync(available);
        }

        [JSInvokable()]
        public static async Task SelectedAccountChanged(string selectedAccount)
        {
            await MetamaskService.Current.ChangeSelectedAccountAsync(selectedAccount);
        }
    }
}