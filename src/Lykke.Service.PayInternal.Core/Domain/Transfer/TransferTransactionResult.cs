﻿using System.Collections.Generic;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    public class TransferTransactionResult
    {
        public string Hash { get; set; }

        public decimal Amount { get; set; }

        public string AssetId { get; set; }

        public string Error { get; set; }

        public TransactionErrorType ErrorType { get; set; }

        public IEnumerable<string> Sources { get; set; }

        public IEnumerable<string> Destinations { get; set; }

        public TransactionIdentityType IdentityType { get; set; }

        public string Identity { get; set; }

        public bool HasError => !string.IsNullOrEmpty(Error);
    }
}
