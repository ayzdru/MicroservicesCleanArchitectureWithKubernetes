using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Application.Commands
{
    public class CreateSubscriberUserCommandValidator : AbstractValidator<CreateSubscriberUserCommand>
    {
        public CreateSubscriberUserCommandValidator()
        {
            RuleFor(v => v.SubscriberUser.UserName).MaximumLength(Constants.User.UserNameMaximumLength).MinimumLength(Constants.User.UserNameMinimumLength).NotEmpty();
            RuleFor(v => v.SubscriberUser.Email).MaximumLength(Constants.User.EmailMaximumLength).MinimumLength(Constants.User.EmailMinimumLength).NotEmpty();
        }
    }
}
