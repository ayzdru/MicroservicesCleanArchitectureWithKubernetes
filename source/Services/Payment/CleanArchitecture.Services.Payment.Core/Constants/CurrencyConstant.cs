using System;
using System.Collections.Generic;
using System.Text;

public partial class Constants
{
    public static class Currency
    {
        public const int NameMinimumLength = 3;
        public const int NameMaximumLength = 500;
        public const bool NameRequired = true;

        public const int SymbolMinimumLength = 3;
        public const int SymbolMaximumLength = 500;
        public const bool SymbolRequired = true;
    }
}
