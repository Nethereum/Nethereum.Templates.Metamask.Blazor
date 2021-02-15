window.NethereumMetamaskInterop = {
    EnableEthereum: async () => {
        try {
            const accounts = await ethereum.request({ method: 'eth_requestAccounts' });
            ethereum.autoRefreshOnNetworkChange = false;
            ethereum.on("accountsChanged",
                function (accounts) {
                    DotNet.invokeMethodAsync('Nethereum.Metamask.Blazor', 'SelectedAccountChanged', accounts[0]);
                });
            ethereum.on("networkChanged",
                function (networkId) {

                });
            return accounts[0];
        } catch (error) {
            return null;
        }
    },
    IsMetamaskAvailable: () => {
        if (window.ethereum) return true;
        return false;
    },
    GetSelectedAddress: () => {
        return ethereum.selectedAddress;
    },

    Request: async (message) => {
        try {
            
            let parsedMessage = JSON.parse(message);
            console.log(parsedMessage);
            const response = await ethereum.request(parsedMessage);
            let rpcResonse = {
                jsonrpc: "2.0",
                result: response,
                id: parsedMessage.id,
                error: null
            }
            console.log(rpcResonse);

            return JSON.stringify(rpcResonse);
        } catch (e) {
            let rpcResonseError = {
                jsonrpc: "2.0",
                id: parsedMessage.id,
                error: {
                    message: e,
                }
            }
            return JSON.stringify(rpcResonseError);
        }
    },

    Send: async (message) => {
        return new Promise(function (resolve, reject) {
            console.log(JSON.parse(message));
            ethereum.send(JSON.parse(message), function (error, result) {
                console.log(result);
                console.log(error);
                resolve(JSON.stringify(result));
            });
        });
    },

    Sign: async (utf8HexMsg) => {
        return new Promise(function (resolve, reject) {
            const from = ethereum.selectedAddress;
            const params = [utf8HexMsg, from];
            const method = 'personal_sign';
            ethereum.send({
                method,
                params,
                from,
            }, function (error, result) {
                if (error) {
                    reject(error);
                } else {
                    console.log(result.result);
                    resolve(JSON.stringify(result.result));
                }

            });
        });
    }

}