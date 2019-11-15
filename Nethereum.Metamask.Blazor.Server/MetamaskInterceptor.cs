using System;
using System.Threading.Tasks;
using Nethereum.JsonRpc.Client;
using Nethereum.JsonRpc.Client.RpcMessages;
using Nethereum.RPC.Eth.DTOs;

namespace Nethereum.Metamask.Blazor
{
    public class MetamaskInterceptor : RequestInterceptor
    {
        private readonly IMetamaskInterop _metamaskInterop;
        private readonly MetamaskService _metamaskService;

        public MetamaskInterceptor(IMetamaskInterop metamaskInterop, MetamaskService metamaskService)
        {
            _metamaskInterop = metamaskInterop;
            _metamaskService = metamaskService;
        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<RpcRequest, string, Task<T>> interceptedSendRequestAsync, RpcRequest request,
            string route = null)
        {
            if (request.Method == "eth_sendTransaction")
            {
                var transaction = (TransactionInput)request.RawParameters[0];
                transaction.From = _metamaskService.SelectedAccount;
                request.RawParameters[0] = transaction;
                var response = await _metamaskInterop.SendAsync(new MetamaskRpcRequestMessage(request.Id, request.Method, _metamaskService.SelectedAccount,
                    request.RawParameters));
                return ConvertResponse<T>(response);
            }
            else
            {
                var response = await _metamaskInterop.SendAsync(new RpcRequestMessage(request.Id,
                    request.Method,
                    request.RawParameters));
                return ConvertResponse<T>(response); 
            }

        }

        public override async Task<object> InterceptSendRequestAsync<T>(
            Func<string, string, object[], Task<T>> interceptedSendRequestAsync, string method,
            string route = null, params object[] paramList)
        {
            if (method == "eth_sendTransaction")
            {
                var transaction = (TransactionInput)paramList[0];
                transaction.From = _metamaskService.SelectedAccount;
                paramList[0] = transaction;
                var response = await _metamaskInterop.SendAsync(new MetamaskRpcRequestMessage(route, method, _metamaskService.SelectedAccount, 
                    paramList));
                return ConvertResponse<T>(response);
            }
            else
            {
                var response = await _metamaskInterop.SendAsync(new RpcRequestMessage(route, _metamaskService.SelectedAccount, method,
                    paramList));
                return ConvertResponse<T>(response);
            }
          
        }

        protected void HandleRpcError(RpcResponseMessage response)
        {
            if (response.HasError)
                throw new RpcResponseException(new JsonRpc.Client.RpcError(response.Error.Code, response.Error.Message,
                    response.Error.Data));
        }

        private  T ConvertResponse<T>(RpcResponseMessage response,
            string route = null)
        {
            HandleRpcError(response);
            try
            {
                return response.GetResult<T>();
            }
            catch (FormatException formatException)
            {
                throw new RpcResponseFormatException("Invalid format found in RPC response", formatException);
            }
        }

    }
}