using FluentValidation;
using Nethereum.UI.Validation;

namespace Nethereum.Erc20.Blazor
{
    public class ERC20ContractValidator : AbstractValidator<ERC20ContractModel>
    {
        public ERC20ContractValidator()
        {
            RuleFor(t => t.Address).IsEthereumAddress();
            RuleFor(t => t.DecimalPlaces).GreaterThan(0).WithMessage("Decimal Places must be greater than 0");
        }
    }
}
