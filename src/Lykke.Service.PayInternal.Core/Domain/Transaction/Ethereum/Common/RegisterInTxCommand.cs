﻿using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    /// <summary>
    /// Command to register any incoming transaction
    /// </summary>
    public class RegisterInTxCommand : IBlockchainTypeHolder
    {
        public string Hash { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public string OperationId { get; set; }
        public string BlockId { get; set; }
        public WorkflowType WorkflowType { get; set; }
        public BlockchainType Blockchain { get; set; }
        public TransactionIdentityType IdentityType { get; set; }
        public string Identity { get; set; }
        public DateTime? FirstSeen { get; set; }
    }
}
