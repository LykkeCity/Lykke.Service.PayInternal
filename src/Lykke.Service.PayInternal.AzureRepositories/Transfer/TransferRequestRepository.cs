using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using MoreLinq;

namespace Lykke.Service.PayInternal.AzureRepositories.Transfer
{
    public class TransferRequestRepository : ITransferRequestRepository
    {
        private const int BatchPieceSize = 15;
        private readonly INoSQLTableStorage<TransferEntity> _tableStorage;

        public TransferRequestRepository(INoSQLTableStorage<TransferEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<ITransferRequest>> GetAllAsync()
        {
            var transactions = await _tableStorage.GetDataAsync();
            return AggregateTransactions(transactions);
        }


        public async Task<ITransferRequest> GetAsync(string transferRequestId)
        {
            var transactions = await _tableStorage.GetDataAsync(transferRequestId);
            return AggregateTransactions(transactions.ToList()).FirstOrDefault();
        }

        public async Task<ITransferRequest> GetAsync(string transferRequestId, string transactionHash)
        {
            var transaction = await _tableStorage.GetDataAsync(transferRequestId, transactionHash);
            return AggregateTransactions(new List<TransferEntity> {transaction}).FirstOrDefault();
        }

        public async Task SaveAsync(ITransferRequest transferInfo)
        {
            var transfers = TransferEntity.Create(transferInfo);

            foreach (var batch in transfers.Batch(BatchPieceSize))
            {
                var tasks = batch.Select(x => _tableStorage.InsertOrMergeAsync(x));
                await Task.WhenAll(tasks);
            }
        }

        private List<ITransferRequest> AggregateTransactions(IList<TransferEntity> transactions)
        {
            var transferAgg = (from transaction in transactions
                group transaction by transaction.TransferId
                into transferAggregation
                select transferAggregation).ToList();

            return (from transfer in transferAgg
                let transferTransactions = transfer.ToList()
                select (ITransferRequest) new TransferRequest
                {
                    TransferId = transfer.Key,
                    TransferStatus = transferTransactions.First().TransferStatus,
                    TransferStatusError = transferTransactions.First().TransferStatusError,
                    CreateDate = transferTransactions.First().CreatedDate,
                    TransactionRequests = (from tt in transferTransactions
                        select (ITransactionRequest) new TransactionRequest
                        {
                            Amount = tt.Amount,
                            AssetId = tt.Currency,
                            DestinationAddress = tt.DestinationAddress,
                            TransactionHash = tt.TransactionHash,
                            CountConfirm = tt.CountConfirm,
                            SourceAmounts = new List<IAddressAmount>(tt.SourceAddresses)
                        }).ToList()
                }).ToList();
        }
    }
}
