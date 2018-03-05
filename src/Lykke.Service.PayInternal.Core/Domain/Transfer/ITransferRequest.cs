﻿using System;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Base transfer object
    /// </summary>
    public interface ITransferRequest
    {
        /// <summary>
        /// Id of transfer
        /// </summary>
        string TransferId { get; set; }
        /// <summary>
        /// List of transaction requests
        /// </summary>
        List<ITransactionRequest> TransactionRequests { get; set; }
        /// <summary>
        /// Transfer Status
        /// </summary>
        TransferStatus TransferStatus { get; set; }
        /// <summary>
        /// Transfer Error Description is transaction fail
        /// </summary>
        TransferStatusError TransferStatusError { get; set; }
        /// <summary>
        /// Date of the transfer was created
        /// </summary>
        DateTime CreateDate { get; set; }
        /// <summary>
        /// Merchant Id
        /// </summary>
        string MerchantId { get; set; }
        /// <summary>
        /// Creates a deep copy of this transfer request
        /// </summary>
        /// <returns>A new transfer request object.</returns>
        ITransferRequest DeepCopy();
    }
}
