using System;
using System.Collections.Generic;
using System.Text;

namespace CleanArchitecture.Services.Payment.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
    public class IgnoreMemberAttribute : Attribute
    {
    }
}
