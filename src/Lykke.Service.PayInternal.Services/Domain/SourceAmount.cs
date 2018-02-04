﻿using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Services
{
    public class SourceAmount : ISourceAmount
    {
        public string SourceAddress { get; set; }
        public decimal Amount { get; set; }
    }
}
