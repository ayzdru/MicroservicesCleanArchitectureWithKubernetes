﻿using CleanArchitecture.Services.Order.Core.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.ValueObjects
{
    
    public class Money : ValueObject
    {
        public Money(decimal amount, string currency)
        {
            ArgumentNullException.ThrowIfNull(amount);
            ArgumentNullException.ThrowIfNull(currency);
            Amount = amount;
            Currency = currency;
        }   
        private Money()
        {
        }
        public Money(decimal amount)
        {
            Amount = amount;
        }

        public decimal Amount { get; set; }
        public string Currency { get; set; }

        public override string ToString() => $"{Amount}{Currency}";

        public static Money Zero(Enums.Currencies currency)
        {
            return new Money(0, currency.ToString());
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

        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Amount;
        }
    }
}
