namespace Nethereum.Erc20.Blazor
{
    public class ERC20ContractModel
    {
        public const int DEFAULT_DECIMALS = 18;
        public const string DAI_SMART_CONTRACT = "0x6b175474e89094c44da98b954eedeac495271d0f";
        public ERC20ContractModel() { }

        public string Address { get; set; } = DAI_SMART_CONTRACT;
        public int DecimalPlaces { get; set; } = DEFAULT_DECIMALS;
        public string Name { get; set; }
    }
}
