﻿using System;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface ICreateTransactionCommand
    {
        string Hash { get; set; }
        string WalletAddress { get; set; }
        string[] SourceWalletAddresses { get; set; }
        decimal Amount { get; set; }
        string AssetId { get; set; }
        int Confirmations { get; set; }
        string BlockId { get; set; }
        BlockchainType Blockchain { get; set; }
        DateTime? FirstSeen { get; set; }
        DateTime? DueDate { get; set; }
        TransactionType Type { get; set; }
        string TransferId { get; set; }
        TransactionIdentityType IdentityType { get; set; }
        string Identity { get; set; }
        string ContextData { get; set; }
    }
}
