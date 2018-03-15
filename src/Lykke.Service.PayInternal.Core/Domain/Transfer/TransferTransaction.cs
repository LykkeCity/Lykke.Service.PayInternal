﻿namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferTransaction
    {
        public string Hash { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string Error { get; set; }
    }
}
