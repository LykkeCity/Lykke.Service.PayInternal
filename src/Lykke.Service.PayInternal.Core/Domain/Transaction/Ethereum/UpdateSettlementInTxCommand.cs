﻿using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction.Ethereum
{
    /// <summary>
    /// Command to update incoming settlement transaction
    /// </summary>
    public class UpdateSettlementInTxCommand : IUpdateTransactionCommand
    {
        public string Hash { get; set; }
        public BlockchainType Blockchain { get; set; }
        public string WalletAddress { get; set; }
        public decimal Amount { get; set; }
        public int Confirmations { get; set; }
        public string BlockId { get; set; }
        public DateTime? FirstSeen { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public bool IsPayment()
        {
            return false;
        }
    }
}
