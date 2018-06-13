﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using JetBrains.Annotations;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.MarketProfile.Client;
using Lykke.Service.MarketProfile.Client.Models;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.AssetPair;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Services
{
    public class AssetRatesService : IAssetRatesService
    {
        private readonly IAssetPairRateRepository _assetPairRateRepository;
        private readonly IReadOnlyList<AssetPairSetting> _assetPairLocalStorageSettings;
        private readonly ILykkeMarketProfile _marketProfileServiceClient;
        private readonly IAssetsLocalCache _assetsLocalCache;

        public AssetRatesService(
            [NotNull] IAssetPairRateRepository assetPairRateRepository, 
            [NotNull] IReadOnlyList<AssetPairSetting> assetPairLocalStorageSettings, 
            [NotNull] ILykkeMarketProfile marketProfileServiceClient, 
            [NotNull] IAssetsLocalCache assetsLocalCache)
        {
            _assetPairRateRepository = assetPairRateRepository ?? throw new ArgumentNullException(nameof(assetPairRateRepository));
            _assetPairLocalStorageSettings = assetPairLocalStorageSettings ?? throw new ArgumentNullException(nameof(assetPairLocalStorageSettings));
            _marketProfileServiceClient = marketProfileServiceClient ?? throw new ArgumentNullException(nameof(marketProfileServiceClient));
            _assetsLocalCache = assetsLocalCache ?? throw new ArgumentNullException(nameof(assetsLocalCache));
        }

        public Task<IAssetPairRate> AddAsync(AddAssetPairRateCommand cmd)
        {
            if (!_assetPairLocalStorageSettings.HasAssetPair(cmd.BaseAssetId, cmd.QuotingAssetId))
                throw new AssetPairRateStorageNotSupportedException(cmd.BaseAssetId, cmd.QuotingAssetId);

            var newRate = Mapper.Map<AssetPairRate>(cmd);

            return _assetPairRateRepository.AddAsync(newRate);
        }

        public async Task<IAssetPairRate> GetCurrentRate(string baseAssetId, string quotingAssetId)
        {
            if (_assetPairLocalStorageSettings.HasAssetPair(baseAssetId, quotingAssetId))
            {
                IReadOnlyList<IAssetPairRate> allRates =
                    await _assetPairRateRepository.GetAsync(baseAssetId, quotingAssetId);

                return allRates
                    .Where(x => x.CreatedOn <= DateTime.UtcNow)
                    .OrderByDescending(x => x.CreatedOn)
                    .FirstOrDefault();
            }

            AssetPair assetPair = await _assetsLocalCache.GetAssetPairAsync(baseAssetId, quotingAssetId);

            if (assetPair == null)
                throw new AssetPairUnknownException(baseAssetId, quotingAssetId);

            AssetPairModel assetPairRate = await InvokeMarketProfileServiceAsync(assetPair.Id);

            return new AssetPairRate
            {
                BaseAssetId = baseAssetId,
                QuotingAssetId = quotingAssetId,
                AskPrice = (decimal) assetPairRate.AskPrice,
                BidPrice = (decimal) assetPairRate.BidPrice,
                CreatedOn = DateTimeUtils.Largest(assetPairRate.AskPriceTimestamp, assetPairRate.BidPriceTimestamp)
            };
        }

        private async Task<AssetPairModel> InvokeMarketProfileServiceAsync(string assetPairId)
        {
            object response = await _marketProfileServiceClient.ApiMarketProfileByPairCodeGetAsync(assetPairId);

            if (response is ErrorModel error)
            {
                throw new Exception(error.Message);
            }

            if (response is AssetPairModel assetPairRate)
            {
                return assetPairRate;
            }

            throw new UnrecognizedApiResponse("Unknown MarketProfile API response");
        }
    }
}
