using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Nethereum.StandardTokenEIP20.ContractDefinition;
using Nethereum.UI.Validation;
using Microsoft.AspNetCore.Components;

namespace Nethereum.Erc20.Blazor
{
    public class ERC20TransferValidator : AbstractValidator<ERC20TransferModel>
    {
        public ERC20TransferValidator()
        {
            RuleFor(t => t.To).IsEthereumAddress();
            RuleFor(t => t.Value).GreaterThan(0).WithMessage("Amount must be greater than 0");
            RuleFor(t => t.ERC20Contract).SetValidator(new ERC20ContractValidator());
        }
    }
}
