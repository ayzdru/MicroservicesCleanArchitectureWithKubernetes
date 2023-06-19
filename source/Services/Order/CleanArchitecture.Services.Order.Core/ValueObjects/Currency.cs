using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Order.Core.ValueObjects
{
    public class Currency : ValueObject
    {
        public static Currency USD = new("USD", "$");

        public Currency(string name, string symbol)
        {
            ArgumentNullException.ThrowIfNull(name);
            ArgumentNullException.ThrowIfNull(symbol);

            Name = name;
            Symbol = symbol;
        }

        public string Name { get; }
        public string Symbol { get; }

        public override string ToString() => Symbol;
        

        public static implicit operator string(Currency currency) => currency.ToString();
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Name;
            yield return Symbol;
        }
    }
}
