using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.UI;

namespace Nethereum.Erc20.Blazor
{
    public class ERC20TransferModel
    {
        public ERC20ContractModel ERC20Contract { get; set; } = new ERC20ContractModel();
        public string To { get; set; }  
        public decimal Value { get; set; }

        public TransferFunction GetTransferFunction()
        {
            return new TransferFunction()
            {
                To = To,
                Value = Web3.Web3.Convert.ToWei(Value, ERC20Contract.DecimalPlaces),
                AmountToSend = 0
            };
        }
    }
}
