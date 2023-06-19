using System;
using System.Collections.Generic;
using System.Text;

public partial class Constants
{
    public static class Product
    {
        public const int NameMinimumLength = 3;
        public const int NameMaximumLength = 500;
        public const bool NameRequired = true;

        public const int DescriptionMinimumLength = 3;
        public const int DescriptionMaximumLength = 500;
        public const bool DescriptionRequired = true;

        public const bool PriceRequired = true;
    }
}
