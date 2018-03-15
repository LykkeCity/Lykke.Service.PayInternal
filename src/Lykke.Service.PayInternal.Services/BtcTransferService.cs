﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Bitcoin.Api.Client.AutoGenerated.Models;
using Lykke.Bitcoin.Api.Client.BitcoinApi;
using Lykke.Bitcoin.Api.Client.BitcoinApi.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    //todo: reconsider this service existance
    [UsedImplicitly]
    public class BtcTransferService : IBtcTransferService
    {
        private readonly IBitcoinApiClient _bitcoinServiceClient;

        public BtcTransferService(IBitcoinApiClient bitcoinServiceClient,
            ITransferRepository transferRepository,
            ITransactionsService transactionServicey,
            ITransactionPublisher transactionPublisher,
            ExpirationPeriodsSettings expirationPeriods,
            ILog log)
        {
            _bitcoinServiceClient =
                bitcoinServiceClient ?? throw new ArgumentNullException(nameof(bitcoinServiceClient));
        }

        public async Task<string> ExecuteAsync(BtcTransfer transfer)
        {
            IEnumerable<ToOneAddress> sources = transfer.Sources
                .Where(x => x != null)
                .Select(x => new ToOneAddress(x.Address, x.Amount));
            
            OnchainResponse response = await _bitcoinServiceClient.TransactionMultipleTransfer(
                Guid.NewGuid(),
                transfer.DestAddress, 
                LykkeConstants.BitcoinAssetId, 
                transfer.FeeRate, 
                transfer.FixedFee, 
                sources);

            if (response.HasError)
                throw new TransferException(response.Error.Code, response.Error.Message);

            return response.Transaction?.TransactionId?.ToString();
        }
    }
}
