using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.UI;
using Nethereum.UI.Validation;
using Microsoft.AspNetCore.Components;

namespace Nethereum.Erc20.Blazor
{
    public class TransferValidator : AbstractValidator<TransferViewModel>
    {
        public TransferValidator()
        {
            RuleFor(t => t.To).IsEthereumAddress();
            RuleFor(t => t.Value).NotEmpty().WithMessage("Amount must be greater than 0");
            RuleFor(t => t.Value).GreaterThan(0).WithMessage("Amount must be greater than 0");
            RuleFor(t => t.ERC20Contract).SetValidator(new ERC20ContractValidator());
        }
    }

    public class ERC20ContractValidator : AbstractValidator<ERC20Contract>
    {
        public ERC20ContractValidator()
        {
            RuleFor(t => t.Address).IsEthereumAddress();
            RuleFor(t => t.DecimalPlaces).NotEmpty().WithMessage("DecimalPlaces must be greater than 0");
            RuleFor(t => t.DecimalPlaces).GreaterThan(0).WithMessage("DecimalPlaces must be greater than 0");
        }
    }

    public class ERC20Contract
    {
        public const int DEFAULT_DECIMALS = 18;
        public ERC20Contract(string address, string name, int decimalPlaces = DEFAULT_DECIMALS) => (Address, Name, DecimalPlaces) = (address, name, decimalPlaces);

        public string Address { get; set; }
        public int DecimalPlaces { get; set; } = DEFAULT_DECIMALS;
        public string Name { get; set; }
    }

    public class TransferViewModel 
    {
        public ERC20Contract ERC20Contract { get; set; }
        public string To { get; set; }  
        public decimal Value { get; set; }

        [Inject] protected IEthereumHostProvider Host { get; set; }

        public TransferViewModel()
        {

        }

        public List<ERC20Contract> SearchERC20Contracts(string text)
        {
            return new List<ERC20Contract>()
            {
                //random tokens check https://etherscan.io/tokens
                new ERC20Contract("0xc02aaa39b223fe8d0a0e5c4f27ead9083c756cc2","WETH", 18),
                new ERC20Contract("0x6b175474e89094c44da98b954eedeac495271d0f","DAI", 18),
                new ERC20Contract("0xdac17f958d2ee523a2206206994597c13d831ec7","Tether USD", 6),
                new ERC20Contract("0x2260fac5e5542a773aa44fbcfedf7c193bc2c599","WBTC", 8),
                new ERC20Contract("0x1f9840a85d5af5bf1d1762f925bdaddc4201f984","Uniswap", 18),
                new ERC20Contract("0x9f8f72aa9304c8b593d555f12ef6589cc3a579a2","Maker", 18),
                new ERC20Contract("0x42d6622dece394b54999fbd73d108123806f6a18","Spank", 18)
            };
        }
    }
}
