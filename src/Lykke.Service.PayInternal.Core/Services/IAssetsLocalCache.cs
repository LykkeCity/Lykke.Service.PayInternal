﻿using System.Threading.Tasks;
using Lykke.Service.Assets.Client.Models;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IAssetsLocalCache
    {
        Task<Asset> GetAssetByIdAsync(string assetId);

        Task<AssetPair> GetAssetPairByIdAsync(string assetPairId);
    }
}
