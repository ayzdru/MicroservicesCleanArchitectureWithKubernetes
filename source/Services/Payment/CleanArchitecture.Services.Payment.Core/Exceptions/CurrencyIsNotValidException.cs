using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Payment.Core.Exceptions
{
    public class CurrencyIsNotValidException : Exception
    {
        public CurrencyIsNotValidException() : base("Currency isn't valid.")
        {
        }
    }
}
