using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// The inner (service) representation of a transfer request.
    /// </summary>
    [UsedImplicitly]
    public class TransferRequest
    {
        /// <summary>
        /// ID of the transfer.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string TransferId { get; set; }
        /// <summary>
        /// List of transfer-related transactions.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public List<TransactionRequest> TransactionRequests { get; set; }
        /// <summary>
        /// Current aggregate transfer state info.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public TransferStatus TransferStatus { get; set; }
        /// <summary>
        /// Current aggregate status error info (if there is an error).
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public TransferStatusError TransferStatusError { get; set; }
        /// <summary>
        /// Transfer creation date.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public DateTime CreateDate { get; set; }
        /// <summary>
        /// ID of the merchant who initiated the transfer.
        /// </summary>
        // ReSharper disable once UnusedMember.Global
        public string MerchantId { get; set; }
    }
}
