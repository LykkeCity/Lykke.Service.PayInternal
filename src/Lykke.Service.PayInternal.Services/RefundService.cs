﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using QBitNinja.Client;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core.Domain.Refund;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
// ReSharper disable once RedundantUsingDirective
using NBitcoin;
using TransactionNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.TransactionNotFoundException;

namespace Lykke.Service.PayInternal.Services
{
    [UsedImplicitly]
    public class RefundService : IRefundService
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly QBitNinjaClient _qBitNinjaClient;
        private readonly ITransferService _transferService;
        private readonly ITransactionsService _transactionService;
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IRefundRepository _refundRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly TimeSpan _expirationTime;

        public RefundService(
            QBitNinjaClient qBitNinjaClient,
            ITransferService transferService,
            ITransactionsService transactionService,
            IPaymentRequestService paymentRequestService,
            IRefundRepository refundRepository,
            IWalletRepository walletRepository,
            TimeSpan expirationTime)
        {
            _qBitNinjaClient =
                qBitNinjaClient ?? throw new ArgumentNullException(nameof(qBitNinjaClient));
            _transferService =
                transferService ?? throw new ArgumentNullException(nameof(transferService));
            _transactionService =
                transactionService ?? throw new ArgumentNullException(nameof(transactionService));
            _paymentRequestService =
                paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _refundRepository =
                refundRepository ?? throw new ArgumentNullException(nameof(refundRepository));
            _walletRepository =
                walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
            _expirationTime = expirationTime;
        }

        public async Task<IRefund> ExecuteAsync(IRefundRequest refund)
        {
            // Initial checkup
            // TODO: remove this check after multi-directional refunds are enabled.
            if (string.IsNullOrWhiteSpace(refund.SourceAddress) ||
                string.IsNullOrWhiteSpace(refund.DestinationAddress))
                throw new NotImplementedException("Multi-directional refunds are not currently supported. Please, specify both Source and Destination addresses.");

            var paymentRequest = await _paymentRequestService.FindAsync(refund.SourceAddress);
            if (paymentRequest == null)
                throw new PaymentRequestNotFoundException("The payment request for the specified wallet address does not exist.");

            if (!paymentRequest.MerchantId.Equals(refund.MerchantId))
                throw new ArgumentException("Payment request found, but it seems to belong to another merchant.");

            var walletsCheckResult = await _checkupMerchantWallets(refund);
            if (!walletsCheckResult)
                throw new ArgumentException("The source (and/or destination wallet) belongs to another merchant.");
            
            // Initial requests fullfill
            var balance = await _qBitNinjaClient.GetBalanceSummary(BitcoinAddress.Create(refund.SourceAddress));
            var refundAmount = balance.Spendable.Amount.ToDecimal(MoneyUnit.BTC);

            var newRefund = new Refund
            {
                PaymentRequestId = paymentRequest.Id,
                DueDate = DateTime.UtcNow.Add(_expirationTime),
                MerchantId = refund.MerchantId,
                RefundId = Guid.NewGuid().ToString(),
                Amount = refundAmount
                // TODO: what about settlement ID?
            };

            var newTransfer = new MultipartTransfer
            {
                PaymentRequestId = paymentRequest.Id,
                AssetId = paymentRequest.PaymentAssetId,
                CreationDate = DateTime.UtcNow,
                FeeRate = 0, // TODO: make sure this is correct
                FixedFee = (decimal)paymentRequest.MarkupFixedFee,
                MerchantId = refund.MerchantId,
                TransferId = Guid.NewGuid().ToString(),
                Parts = new List<TransferPart>()
            };

            var result = new RefundResponse
            {
                MerchantId = refund.MerchantId,
                PaymentRequestId = paymentRequest.Id,
                RefundId = newRefund.RefundId,
                DueDate = newRefund.DueDate,
                Amount = refundAmount
                // TODO: what about settlement ID?
            };

            // The main work below:

            await _refundRepository.AddAsync(newRefund); // Save the refund itself first

            // The simpliest case: we have both source and destionation addresses. Create a new transfer for the whole volume of money from the source.
            if (!string.IsNullOrWhiteSpace(refund.DestinationAddress))
            {
                newTransfer.Parts.Add(
                    new TransferPart
                    {
                        Destination = new AddressAmount
                        {
                            Address = refund.DestinationAddress,
                            Amount = refundAmount
                        },
                        Sources = new List<AddressAmount>
                        {
                            new AddressAmount
                            {
                                Address = refund.SourceAddress,
                                Amount = refundAmount
                            }
                        }
                    }
                );

                var executionResult = await _transferService.ExecuteMultipartTransferAsync(newTransfer, TransactionType.Refund); // Execute the transfer for single transaction and check
                if (executionResult.State == TransferExecutionResult.Fail)
                    throw new Exception(executionResult.ErrorMessage);
            }
            // And another case: we have only source, so, we need to reverse all the transactions from the payment request.
            else
            {
                // ATTENTION: currently this code is unreachable due to pre-check of DestinationAddress presense.
                var transactions = await _transactionService.GetConfirmedAsync(paymentRequest.WalletAddress);
                if (transactions == null)
                    throw new TransactionNotFoundException("There are (still) no confirmed transactions for the payment request with the specified wallet address.");

                // ReSharper disable once UnusedVariable
                foreach (var tran in transactions)
                {
                   // TODO: Implement transaction reversing with use of QBitNinja client.
                }
                
                var executionResult = await _transferService.ExecuteMultipartTransferAsync(newTransfer, TransactionType.Refund); // Execute the transfer for multiple transactions and check
                if (executionResult.State != TransferExecutionResult.Success)
                    throw new Exception(executionResult.ErrorMessage);
            }

            // Additionally, process the payment request itself.
            await _paymentRequestService.ProcessAsync(refund.SourceAddress);

            return result;
        }

        public async Task<IRefund> GetStateAsync(string merchantId, string refundId)
        {
            return await _refundRepository.GetAsync(merchantId, refundId);
        }

        private async Task<bool> _checkupMerchantWallets(IRefundRequest refund)
        {
            var wallets = (await _walletRepository.GetByMerchantAsync(refund.MerchantId))?.ToList();

            if (wallets == null || !wallets.Any()) return false;

            // Currently we check up only the source address. But is may be useful to check the destination either.
            return wallets.Any(w => w.Address == refund.SourceAddress);
        }
    }
}
