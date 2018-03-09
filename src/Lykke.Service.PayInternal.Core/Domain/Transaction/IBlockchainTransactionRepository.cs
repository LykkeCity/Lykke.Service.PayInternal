﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transaction
{
    public interface IBlockchainTransactionRepository
    {
        Task<IReadOnlyList<IBlockchainTransaction>> GetAsync(string walletAddress);
        Task<IBlockchainTransaction> GetAsync(string walletAddress, string transactionId);
        Task<IEnumerable<IBlockchainTransaction>> GetNotExpiredAsync(IReadOnlyList<string> paymentRequestIdList, int minConfirmationsCount);
        Task AddAsync(IBlockchainTransaction blockchainTransaction);
        Task UpdateAsync(IBlockchainTransaction blockchainTransaction);
    }
}
