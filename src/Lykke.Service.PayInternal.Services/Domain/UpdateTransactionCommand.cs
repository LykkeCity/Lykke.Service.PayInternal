﻿using System;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transaction;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class UpdateTransactionCommand : IUpdateTransactionCommand
    {
        public string TransactionId { get; set; }

        public BlockchainType Blockchain { get; set; }

        public string WalletAddress { get; set; }
        public decimal Amount { get; set; }

        public int Confirmations { get; set; }

        public string BlockId { get; set; }

        public DateTime? FirstSeen { get; set; }

        public bool IsPayment()
        {
            return !string.IsNullOrEmpty(WalletAddress);
        }
    }
}
