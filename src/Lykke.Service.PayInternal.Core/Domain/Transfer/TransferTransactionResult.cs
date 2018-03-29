﻿using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferTransactionResult
    {
        public string Hash { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string Error { get; set; }

        public IEnumerable<string> Sources { get; set; }

        public IEnumerable<string> Destinations { get; set; }

        public bool HasError => !string.IsNullOrEmpty(Error);
    }
}
