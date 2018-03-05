using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class TransactionsService : ITransactionsService
    {
        private readonly IBlockchainTransactionRepository _transactionRepository;
        private readonly IPaymentRequestRepository _paymentRequestRepository;

        public TransactionsService(
            IBlockchainTransactionRepository transactionRepository,
            IPaymentRequestRepository paymentRequestRepository)
        {
            _transactionRepository = transactionRepository;
            _paymentRequestRepository = paymentRequestRepository;
        }

        public async Task<IEnumerable<IBlockchainTransaction>> GetAsync(string walletAddress)
        {
            return await _transactionRepository.GetAsync(walletAddress);
        }

        public async Task Create(ICreateTransaction request)
        {
            IPaymentRequest paymentRequest = await _paymentRequestRepository.FindAsync(request.WalletAddress);

            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException(request.WalletAddress);

            var transactionEntity = new BlockchainTransaction
            {
                WalletAddress = request.WalletAddress,
                TransactionId = request.TransactionId,
                Amount =request.Amount,
                AssetId = request.AssetId,
                Confirmations = request.Confirmations,
                BlockId = request.BlockId,
                Blockchain = request.Blockchain,
                FirstSeen = request.FirstSeen,
                PaymentRequestId = paymentRequest.Id
            };

            await _transactionRepository.AddAsync(transactionEntity);
        }

        public async Task Update(IUpdateTransaction request)
        {
            IBlockchainTransaction transaction =
                await _transactionRepository.GetAsync(request.WalletAddress, request.TransactionId);

            if (transaction == null)
                throw new TransactionNotFoundException(request.TransactionId);

            transaction.Amount = request.Amount;
            transaction.BlockId = request.BlockId;
            transaction.Confirmations = request.Confirmations;

            await _transactionRepository.UpdateAsync(transaction);
        }
    }
}
