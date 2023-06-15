using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Services.Catalog.Core.ValueObjects
{
    public class ErrorValueObject : ValueObject
    {       
        public string Key { get; private set; }
        public string Message { get; private set; }
        public ErrorValueObject(string key, string message)
        {
            Key = key;
            Message = message;
        }
        protected override IEnumerable<object> GetEqualityComponents()
        {
            yield return Key;
            yield return Message;
        }
    }
}
