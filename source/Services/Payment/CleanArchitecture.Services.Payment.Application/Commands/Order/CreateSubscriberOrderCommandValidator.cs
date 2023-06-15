using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Application.Commands
{
    public class CreateSubscriberOrderCommandValidator : AbstractValidator<CreateSubscriberOrderCommand>
    {
        public CreateSubscriberOrderCommandValidator()
        {
            RuleFor(v => v.SubscriberOrder.CurrencyName).MaximumLength(Constants.Currency.NameMaximumLength).MinimumLength(Constants.Currency.NameMinimumLength).NotEmpty();
            RuleFor(v => v.SubscriberOrder.CurrencySymbol).MaximumLength(Constants.Currency.SymbolMaximumLength).MinimumLength(Constants.Currency.SymbolMinimumLength).NotEmpty();
            RuleFor(v => v.SubscriberOrder.TotalAmount).GreaterThan(0);
        }
    }
}
