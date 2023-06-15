using CleanArchitecture.Services.Payment.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.ValueObjects
{
    public class Money : ValueObject
    {
        public Money(decimal amount, Currency currency)
        {
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(currency);
            Amount = amount;
            Currency = currency;
        }

        public decimal Amount { get; private set; }
        public Currency Currency { get; private set; }

        public override string ToString() => $"{Amount}{Currency}";

        public static Money Zero(Currency currency)
        {
            return new Money(0, currency);
        }

        public static bool operator <(Money obj1, Money obj2)
        {
            ThrowIfCurrencyIsNotMatch(obj1, obj2);

            return obj1.Amount < obj2.Amount;
        }

        public static bool operator >(Money obj1, Money obj2)
        {
            ThrowIfCurrencyIsNotMatch(obj1, obj2);

            return obj1.Amount > obj2.Amount;
        }

        public static Money operator +(Money obj1, Money obj2)
        {
            ThrowIfCurrencyIsNotMatch(obj1, obj2);

            return new Money(obj1.Amount + obj2.Amount, obj1.Currency);
        }

        public static Money operator -(Money obj1, Money obj2)
        {
            ThrowIfCurrencyIsNotMatch(obj1, obj2);

            return new Money(obj1.Amount - obj2.Amount, obj1.Currency);
        }

        private static void ThrowIfCurrencyIsNotMatch(Money obj1, Money obj2)
        {
            if (obj1.Currency != obj2.Currency) throw new CurrencyIsNotValidException();
        }
    }
}
