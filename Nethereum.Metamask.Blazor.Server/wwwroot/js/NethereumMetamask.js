window.NethereumMetamaskInterop = {
    EnableEthereum: async () => {
        try {
            await ethereum.enable();
            ethereum.autoRefreshOnNetworkChange = false;
            ethereum.on("accountsChanged",
                function(accounts) {

                });
            ethereum.on("networkChanged",
                function (networkId) {

                });
            return true;
        } catch (error) {
            return false;
        }
    },
    IsMetamaskAvailable: () => {
        if (window.ethereum) return true;
        return false;
    },
    GetSelectedAddress: () => {
        return ethereum.selectedAddress;
    },

    Send: async (message) => {
        return new Promise(function (resolve, reject) {
            ethereum.send(JSON.parse(message), function (error, result) {
                console.log(result);
                resolve(JSON.stringify(result));


            });
        });
    }





}