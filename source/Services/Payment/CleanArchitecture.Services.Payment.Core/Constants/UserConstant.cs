using System;
using System.Collections.Generic;
using System.Text;

public partial class Constants
{
    public static class User
    {
        public const int UserNameMinimumLength = 3;
        public const int UserNameMaximumLength = 500;
        public const bool UserNameRequired = true;

        public const int EmailMinimumLength = 3;
        public const int EmailMaximumLength = 500;
        public const bool EmailRequired = true;
    }
}
