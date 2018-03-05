using Lykke.AzureStorage.Tables;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferEntity : AzureTableEntity
    {
        public static List<TransferEntity> Create(ITransferRequest transferRequest)
        {
            return (from transferRequestTransaction in transferRequest.TransactionRequests
                    select new TransferEntity(transferRequest.TransferId,
                        transferRequest.TransferStatus,
                        transferRequest.TransferStatusError,
                        transferRequest.CreateDate,
                        transferRequestTransaction)).ToList();
        }

        public TransferEntity() : base()
        {
            ETag = "*";
        }

        private TransferEntity(
            string transferId,
            TransferStatus transferStatus,
            TransferStatusError transferStatusError,
            DateTime createdDate,
            ITransactionRequest transaction
            )
        {
            TransferId = transferId;
            TransferStatus = transferStatus;
            TransferStatusError = transferStatusError;
            CreatedDate = createdDate;
            TransactionHash = transaction.TransactionHash;
            SourceAddresses = transaction.SourceAmounts;
            DestinationAddress = transaction.DestinationAddress;
            Amount = transaction.Amount;
            Currency = transaction.AssetId;
        }

        public string TransferId { get => PartitionKey; set => PartitionKey = value; }
        public string TransactionHash { get => RowKey; set => RowKey = value; }
        [IgnoreProperty]
        public TransferStatus TransferStatus { get; set; }
        [IgnoreProperty]
        public TransferStatusError TransferStatusError { get; set; }
        [IgnoreProperty]
        public IEnumerable<IAddressAmount> SourceAddresses { get; set; }
        public DateTime CreatedDate { get; set; }
        public string DestinationAddress { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; }
        public int CountConfirm { get; set; }
    }
}
