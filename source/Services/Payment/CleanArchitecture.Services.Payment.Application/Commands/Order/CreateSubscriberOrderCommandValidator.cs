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
            RuleFor(v => v.SubscriberOrder.TotalAmountCurrency).MaximumLength(Constants.Money.CurrencyMaximumLength).MinimumLength(Constants.Money.CurrencyMinimumLength).NotEmpty();
            RuleFor(v => v.SubscriberOrder.TotalAmount).GreaterThan(0);
        }
    }
}
