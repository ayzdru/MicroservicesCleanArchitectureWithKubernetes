﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CleanArchitecture.Services.Basket.Core.Interfaces
{
    public interface ITokenProvider
    {
        Task<string> GetTokenAsync();
    }
}
