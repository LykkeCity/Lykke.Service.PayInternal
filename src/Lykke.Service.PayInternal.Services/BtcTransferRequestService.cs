﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Bitcoin.Api.Client.AutoGenerated.Models;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Domain.Wallet;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Services.Domain;
using NBitcoin;
using Newtonsoft.Json;


namespace Lykke.Service.PayInternal.Services
{
    public class BtcTransferRequestService : ITransferRequestService
    {

        private readonly ITransferRepository _transferRepository;
        private readonly IWalletRepository _walletRepository;
        private readonly IBitcoinApiClient _bitcointApiClient;
        private readonly ITransferRequestPublisher _transferRequestPublisher;
        private readonly ILog _log;


        #region .ctors
        public BtcTransferRequestService(
            ITransferRepository transferRepository,
            IWalletRepository walletRepository,
            IBitcoinApiClient bitcointApiClient,
            ITransferRequestPublisher transferRequestPublisher,
            ILog log)
        {
            _transferRepository = transferRepository ?? throw new ArgumentNullException(nameof(transferRepository));
            _walletRepository = walletRepository ?? throw new ArgumentNullException(nameof(walletRepository));
            _bitcointApiClient = bitcointApiClient ?? throw new ArgumentNullException(nameof(bitcointApiClient));
            _transferRequestPublisher = transferRequestPublisher ?? throw new ArgumentNullException(nameof(transferRequestPublisher));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }
        #endregion

        #region public

        /// <summary>
        /// Tries to execute a crosswise transfer request from many sources to many destinations.
        /// </summary>
        /// <param name="transferRequest">The transfer request.</param>
        /// <returns>The updated transfer request OR transfer request with error.</returns>
        public async Task<ITransferRequest> CreateTransferCrosswiseAsync(ITransferRequest transferRequest)
        {
            // We need an independent copy of the request here, for some methods in current implementation change the inner state of the given object.
            // For instance, the BtcTransferRequest class' constructor reuses the transaction list it gets.
            var transferRequestUpdated = transferRequest.DeepCopy();

            // First of all, we need to verify the source addresses to belong to the Merchant.
            var sourcesCheckupResult = await CheckupSourcesAndWalletsAsync(transferRequestUpdated);
            if (sourcesCheckupResult != null)
                return sourcesCheckupResult;

            // Here would be no additional validations. If transaction fails, we will know it from the underlying BitcoinApi service.

            // Save the initial state of transfer
            await _transferRepository.SaveAsync(transferRequestUpdated);
            await _transferRequestPublisher.PublishAsync(transferRequestUpdated);

            // The main work
            foreach (ITransactionRequest tran in transferRequestUpdated.TransactionRequests)
            {
                var sourceAddresses = (from s in tran.SourceAmounts
                                       select new ToOneAddress(s.Address, s.Amount)).ToList();
                // Please, note: feeRate = 0 and fixedFee = 0 here
                var currentResult = await _bitcointApiClient.TransactionMultipleTransfer(null, tran.DestinationAddress, tran.Currency, 0, 0, sourceAddresses);

                if (currentResult == null || currentResult.HasError)
                {
                    // Please, note: after the first faulty transaction we abort the further execution.
                    // BUT money that we had already transfered will not go back to source address(es).
                    // To make the whole transfer request atomic we need to implement such feature in
                    // BitcoinApi service at first.
                    return await PackFaultyResult(currentResult, tran, transferRequestUpdated);
                }
                else
                {
                    tran.TransactionHash = currentResult.Transaction.Hash;
                }

                await _transferRepository.SaveAsync(transferRequestUpdated);
                await _transferRequestPublisher.PublishAsync(transferRequestUpdated);
            }

            transferRequestUpdated.TransferStatus = TransferStatus.InProgress;
            transferRequestUpdated.TransferStatusError = TransferStatusError.NotError;
            await _transferRepository.SaveAsync(transferRequestUpdated);
            await _transferRequestPublisher.PublishAsync(transferRequestUpdated);
            return transferRequestUpdated;
        }

        public async Task<ITransferRequest> UpdateTransferStatusAsync(ITransferRequest transfer)
        {
            var t = await GetTransferInfoAsync(transfer.TransferId);
            if (t == null)
            {
                return null;
            }
            t.TransferStatus = transfer.TransferStatus;
            t.TransferStatusError = transfer.TransferStatusError;

            var result =  await _transferRepository.SaveAsync(t) ?? BtcTransferRequest.CreateErrorTransferRequest(transfer.MerchantId,
                              TransferStatusError.InternalError, transfer.TransactionRequests);
            await _transferRequestPublisher.PublishAsync(result);

            return result;

        }
        
        public async Task<ITransferRequest> UpdateTransferAsync(ITransferRequest transfer)
        {
            var result = await _transferRepository.SaveAsync(transfer) ?? BtcTransferRequest.CreateErrorTransferRequest(transfer.MerchantId,
                       TransferStatusError.InternalError, transfer.TransactionRequests);

            await _transferRequestPublisher.PublishAsync(result);

            return result;
        }

        public async Task<ITransferRequest> GetTransferInfoAsync(string transferId)
        {
            return await _transferRepository.GetAsync(transferId);
        }

        [Obsolete("This public method is a part of obsolete API. May be removed in newest versions.")]
        public async Task<ITransferRequest> CreateTransferAsync(ITransferRequest transferRequest)
        {
            var wallets = (await _walletRepository.GetByMerchantAsync(transferRequest.MerchantId)).ToList();
            var transactions = new List<ITransactionRequest>();

            foreach (var transaction in transferRequest.TransactionRequests)
            {
                List<IAddressAmount> sources = new List<IAddressAmount>();
                if (transaction.SourceAmounts == null || !transaction.SourceAmounts.Any())
                {
                    sources.AddRange(CalculateSources(
                        from w in wallets
                        select new SourceAmount
                        {
                            Address = w.Address,
                            Amount = 0
                        }, 0, wallets));
                }
                else
                {
                    sources.AddRange(CalculateSources(transaction.SourceAmounts, 0, wallets));
                }

                if (!sources.Any())
                {
                    await _log.WriteWarningAsync(nameof(BtcTransferRequestService), nameof(CreateTransferAsync),
                        transferRequest.ToJson(),
                        "Source addresses don't found or amount is not enought.");
                    return BtcTransferRequest.CreateErrorTransferRequest(transferRequest.MerchantId,
                        TransferStatusError.InvalidAmount, transferRequest.TransactionRequests);
                }
                var transferWallets = new List<string>(sources.Select(s => s.Address))
                {
                    transaction.DestinationAddress
                };

                var isAddressesValid = CheckAddressesValid(transferWallets);
                if (!isAddressesValid)
                {
                    await _log.WriteWarningAsync(nameof(BtcTransferRequestService), nameof(CreateTransferAsync),
                        transferRequest.ToJson(),
                        "Source addresses don't found or amount is not enought.");
                    return BtcTransferRequest.CreateErrorTransferRequest(transferRequest.MerchantId,
                        TransferStatusError.InvalidAddress, transferRequest.TransactionRequests);
                }
                var result = await CreateBtcTransfer(sources, transaction.DestinationAddress);
                if (result.Item2 != TransferStatusError.NotError)
                {
                    await _log.WriteWarningAsync(nameof(BtcTransferRequestService), nameof(CreateTransferAsync),
                        transferRequest.ToJson(),
                        "Create transfer error occurs.");
                    return BtcTransferRequest.CreateErrorTransferRequest(transferRequest.MerchantId,
                        result.Item2, transferRequest.TransactionRequests);
                }
                transactions.Add(result.Item1);
            }

            return BtcTransferRequest.CreateTransferRequest(transferRequest.MerchantId, transactions);
        }

        #endregion

        #region private

        /// <summary>
        /// Looks through the source addresses and check em up to belong to the Merchant.
        /// </summary>
        /// <param name="transferRequest">Transfer request.</param>
        /// <returns>Error transfer request OR null if validation ended up with success.</returns>
        private async Task<ITransferRequest> CheckupSourcesAndWalletsAsync(ITransferRequest transferRequest)
        {
            // Not the very best algorythm for different transactions may have the same source address(es).
            var wallets = (await _walletRepository.GetByMerchantAsync(transferRequest.MerchantId))?.ToList();
            if (wallets == null)
            {
                await _log.WriteWarningAsync(
                            nameof(BtcTransferRequestService),
                            nameof(CreateTransferCrosswiseAsync),
                            transferRequest.ToJson(),
                            $"Error while creating transfer: the merchant with ID \"{transferRequest.MerchantId}\" does not exist.");

                return BtcTransferRequest.CreateErrorTransferRequest(
                    transferRequest.MerchantId,
                    TransferStatusError.MerchantNotFound,
                    transferRequest.TransactionRequests);
            }
            if (!wallets.Any())
            {
                await _log.WriteWarningAsync(
                            nameof(BtcTransferRequestService),
                            nameof(CreateTransferCrosswiseAsync),
                            transferRequest.ToJson(),
                            $"Error while creating transfer: the merchant with ID \"{transferRequest.MerchantId}\" does not have any wallets.");

                return BtcTransferRequest.CreateErrorTransferRequest(
                    transferRequest.MerchantId,
                    TransferStatusError.MerchantHasNoWallets,
                    transferRequest.TransactionRequests);
            }

            foreach (ITransactionRequest tran in transferRequest.TransactionRequests)
            {
                foreach (IAddressAmount item in tran.SourceAmounts)
                {
                    if (!wallets.Any(w => w.Address == item.Address))
                    {
                        await _log.WriteWarningAsync(
                            nameof(BtcTransferRequestService),
                            nameof(CreateTransferCrosswiseAsync),
                            transferRequest.ToJson(),
                            $"Error while creating transfer: the source address \"{item.Address}\" is incorrect or does not belong to Merchant with ID = \"{transferRequest.MerchantId}\"");

                        return BtcTransferRequest.CreateErrorTransferRequest(
                            transferRequest.MerchantId,
                            TransferStatusError.InvalidAddress,
                            transferRequest.TransactionRequests);

                    }
                }
            }

            return null; // Everything's fine
        }

        /// <summary>
        /// Packs the result of faulty transaction into new transfer request object, supplying the error info for clients.
        /// </summary>
        /// <param name="btcApiResponce">The response of Bitcoin Api Service on the task of transaction execution.</param>
        /// <param name="faultyTransaction">The faulty transaction itself.</param>
        /// <param name="theTransfer">The transfer request from wich everything had started.</param>
        /// <returns></returns>
        private async Task<ITransferRequest> PackFaultyResult(OnchainResponse btcApiResponce, ITransactionRequest faultyTransaction, ITransferRequest theTransfer)
        {
            var errMessage = btcApiResponce?.Error?.Message ?? $"Transaction to destination address \"{faultyTransaction.DestinationAddress}\" with amount of \"{faultyTransaction.Amount} {faultyTransaction.Currency.ToString()}\" failed by unknown reason.";
            var errCode = btcApiResponce?.Error?.Code;

            await _log.WriteWarningAsync(
                nameof(BtcTransferRequestService),
                nameof(CreateTransferAsync),
                faultyTransaction.ToJson(),
                $"Transaction to destination address \"{faultyTransaction.DestinationAddress}\" with amount of \"{faultyTransaction.Amount} {faultyTransaction.Currency.ToString()}\" failed with code {errCode} and message \"{errMessage}\".");
            
            return BtcTransferRequest.CreateErrorTransferRequest(
                theTransfer.MerchantId,
                TransferStatusError.InternalError,
                theTransfer.TransactionRequests);
        }

        [Obsolete("This internal method is used in oblolete public methods. May be removed in newest versions.")]
        private async Task<Tuple<ITransactionRequest, TransferStatusError>> CreateBtcTransfer(List<IAddressAmount> sources, string destination)
        {
            sources = sources.Where(s => s.Amount > 0).ToList();
            OnchainResponse request = null;
            var result = new TransactionRequest
            {
                DestinationAddress = destination,
                Amount = sources.Sum(s => s.Amount),
                Currency = LykkeConstants.BitcoinAssetId,
                SourceAmounts = sources,
                CountConfirm = 1
            };

            var store = new MultipleTransferRequest
            {
                Asset = LykkeConstants.BitcoinAssetId,

                Destination = destination,
                Sources = (from s in sources
                           select new ToOneAddress(s.Address, s.Amount)).ToList()
            };


            if (sources.Any())
            {
                request = await _bitcointApiClient.TransactionMultipleTransfer(null, store.Destination, store.Asset, 0, 0, store.Sources);
            }


            if (request == null || request.HasError)
            {

                if (request?.Error?.ErrorCode == ErrorCode.NotEnoughBitcoinAvailable)
                {
                    var errorCode = request.Error.Code;
                    var errorMessage = request.Error.Message;
                    await _log.WriteWarningAsync(nameof(BtcTransferRequestService), nameof(CreateBtcTransfer), store.ToJson(), $"Invalid amount. Error on TransactionMultipletransfer: {errorMessage} ({errorCode})");

                    return new Tuple<ITransactionRequest, TransferStatusError>(result, TransferStatusError.InvalidAddress);

                }

                await _log.WriteWarningAsync(nameof(BtcTransferRequestService), nameof(CreateBtcTransfer), store.ToJson(), "Transaction not confirmed.");


                return new Tuple<ITransactionRequest, TransferStatusError>(result, TransferStatusError.NotConfirmed);
            }

            result.TransactionHash = request.Transaction.Hash;
            await _log.WriteInfoAsync(nameof(BtcTransferRequestService), nameof(CreateBtcTransfer), result.ToJson(), "Transfer created");

            return new Tuple<ITransactionRequest, TransferStatusError>(result, TransferStatusError.NotError);


        }

        [Obsolete("This internal method is used in oblolete public methods. May be removed in newest versions.")]
        private List<IAddressAmount> CalculateSources(IEnumerable<IAddressAmount> sources, decimal amount, List<IWallet> wallets)
        {
            var result = new List<IAddressAmount>();
            var sourcesList = sources?.ToList() ?? new List<IAddressAmount>();
            var sourceToCalc = new List<IAddressAmount>();
            if (sourcesList.Count == 0)
            {
                sourceToCalc.AddRange(from w in wallets

                                      select new SourceAmount { Address = w.Address, Amount = w.Amount });

            }
            else if (sourcesList.All(s => s.Amount == 0))
            {
                sourceToCalc.AddRange(from s in sourcesList

                                      join w in wallets on s.Address equals w.Address
                                      select new SourceAmount { Address = s.Address, Amount = w.Amount });

            }
            else
            {
                sourceToCalc.AddRange(from s in sourcesList

                                      join w in wallets on s.Address equals w.Address
                                      where s.Amount <= w.Amount
                                      select new SourceAmount { Address = s.Address, Amount = w.Amount });

                if (result.Count != sourcesList.Count)
                {
                    result.Clear();
                }
            }

            if (amount == 0)
            {
                result = sourceToCalc;
            }
            else
            {
                var amounToPay = amount;
                while (amounToPay > 0)
                {
                    var sourceAmount = sourceToCalc.FirstOrDefault();
                    if (sourceAmount == null)
                    {
                        return null;
                    }
                    if (amounToPay > sourceAmount.Amount)
                    {
                        amounToPay -= sourceAmount.Amount;
                    }
                    else
                    {
                        sourceAmount.Amount = amounToPay;
                        amounToPay = 0;
                    }
                    result.Add(sourceAmount);
                    sourceToCalc.Remove(sourceAmount);
                }
            }

            return !result.Any() ? null : result;
        }

        [Obsolete("This internal method is used in oblolete public methods. May be removed in newest versions.")]
        private bool CheckAddressesValid(IReadOnlyList<string> addresses)
        {
            try
            {
                foreach (var address in addresses)
                {
                    BitcoinAddress.Create(address);

                }
                return true;
            }
            catch
            {
                return false;
            }

        }
        #endregion
    }
}