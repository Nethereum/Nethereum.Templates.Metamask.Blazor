async function metamaskRequest(parsedMessage) {
    try {
        console.log(parsedMessage);
        const response = await ethereum.request(parsedMessage);
        let rpcResponse = {
            jsonrpc: "2.0",
            result: response,
            id: parsedMessage.id,
            error: null
        }
        console.log("the response:");
        console.log(rpcResponse);

        return rpcResponse;
    } catch (e) {
        console.log(e);
        let rpcResonseError = {
            jsonrpc: "2.0",
            id: parsedMessage.id,
            error: e
        }
        return rpcResonseError;
    }
}

async function getSelectedAddress() {
    let accountsReponse = await getAddresses();
    if (accountsReponse.error !== null) throw accountsReponse.error;
    return accountsReponse.result[0];
}

async function getAddresses() {
   return await metamaskRequest({ method: 'eth_requestAccounts' });
}

window.NethereumMetamaskInterop = {
    EnableEthereum: async () => {
        try {
            const selectedAccount = getSelectedAddress();
            ethereum.autoRefreshOnNetworkChange = false;
            ethereum.on("accountsChanged",
                function (accounts) {
                    DotNet.invokeMethodAsync('Nethereum.Metamask.Blazor', 'SelectedAccountChanged', accounts[0]);
                });
            ethereum.on("networkChanged",
                function (networkId) {
                    DotNet.invokeMethodAsync('Nethereum.Metamask.Blazor', 'SelectedNetworkChanged', chainId.toString());
                });
            return selectedAccount;
        } catch (error) {
            return null;
        }
    },
    IsMetamaskAvailable: () => {
        if (window.ethereum) return true;
        return false;
    },
    GetAddresses: async () => {
        const rpcResponse = await getAddresses();
        return JSON.stringify(rpcResponse);
    },

    Request: async (message) => {
        const parsedMessage = JSON.parse(message);
        const rpcResponse = await metamaskRequest(parsedMessage);
        return JSON.stringify(rpcResponse);
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
        try {
            const from = await getSelectedAddress();
            console.log(from);
            const params = [utf8HexMsg, from];
            const method = 'personal_sign';
            const rpcResponse = await metamaskRequest({
                method,
                params,
                from
            });
            return JSON.stringify(rpcResponse);
        } catch (e) {
            console.log(e);
            let rpcResponseError = {
                jsonrpc: "2.0",
                id: parsedMessage.id,
                error: e
            }
            return JSON.stringify(rpcResponseError);

        }
    }

}