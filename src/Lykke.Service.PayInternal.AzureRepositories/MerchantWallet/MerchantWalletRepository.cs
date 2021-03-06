﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using AzureStorage;
using AzureStorage.Tables.Templates.Index;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Domain.MerchantWallet;
using KeyNotFoundException = Lykke.Service.PayInternal.Core.Exceptions.KeyNotFoundException;

namespace Lykke.Service.PayInternal.AzureRepositories.MerchantWallet
{
    public class MerchantWalletRepository : IMerchantWalletRespository
    {
        private readonly INoSQLTableStorage<MerchantWalletEntity> _tableStorage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByIdStorage;
        private readonly INoSQLTableStorage<AzureIndex> _indexByAddressStorage;

        public MerchantWalletRepository(
            [NotNull] INoSQLTableStorage<MerchantWalletEntity> tableStorage, 
            [NotNull] INoSQLTableStorage<AzureIndex> indexByIdStorage, 
            [NotNull] INoSQLTableStorage<AzureIndex> indexByAddressStorage)
        {
            _tableStorage = tableStorage ?? throw new ArgumentNullException(nameof(tableStorage));
            _indexByIdStorage = indexByIdStorage ?? throw new ArgumentNullException(nameof(indexByIdStorage));
            _indexByAddressStorage = indexByAddressStorage ?? throw new ArgumentNullException(nameof(indexByAddressStorage));
        }

        public async Task<IMerchantWallet> CreateAsync(IMerchantWallet src)
        {
            MerchantWalletEntity entity = MerchantWalletEntity.ByMerchant.Create(src);

            await _tableStorage.InsertAsync(entity);

            AzureIndex indexById = MerchantWalletEntity.IndexById.Create(entity);

            await _indexByIdStorage.InsertAsync(indexById);

            AzureIndex indexByAddress = MerchantWalletEntity.IndexByAddress.Create(entity);

            await _indexByAddressStorage.InsertAsync(indexByAddress);

            return Mapper.Map<Core.Domain.MerchantWallet.MerchantWallet>(entity);
        }

        public async Task<IMerchantWallet> GetByAddressAsync(BlockchainType network, string walletAddress)
        {
            AzureIndex index = await _indexByAddressStorage.GetDataAsync(
                MerchantWalletEntity.IndexByAddress.GeneratePartitionKey(network, walletAddress),
                MerchantWalletEntity.IndexByAddress.GenerateRowKey());

            if (index == null) 
                throw new KeyNotFoundException();

            MerchantWalletEntity entity = await _tableStorage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.MerchantWallet.MerchantWallet>(entity);
        }

        public async Task<IMerchantWallet> GetByIdAsync(string id)
        {
            AzureIndex index = await _indexByIdStorage.GetDataAsync(
                MerchantWalletEntity.IndexById.GeneratePartitionKey(id),
                MerchantWalletEntity.IndexById.GenerateRowKey());

            if (index == null)
                throw new KeyNotFoundException();

            MerchantWalletEntity entity = await _tableStorage.GetDataAsync(index);

            return Mapper.Map<Core.Domain.MerchantWallet.MerchantWallet>(entity);
        }

        public async Task<IReadOnlyList<IMerchantWallet>> GetByMerchantAsync(string merchantId)
        {
            IEnumerable<MerchantWalletEntity> wallets =
                await _tableStorage.GetDataAsync(MerchantWalletEntity.ByMerchant.GeneratePartitionKey(merchantId));

            return Mapper.Map<IReadOnlyList<Core.Domain.MerchantWallet.MerchantWallet>>(wallets);
        }

        public async Task UpdateAsync(IMerchantWallet src)
        {
            MerchantWalletEntity updatedEntity = await _tableStorage.MergeAsync(
                MerchantWalletEntity.ByMerchant.GeneratePartitionKey(src.MerchantId),
                MerchantWalletEntity.ByMerchant.GenerateRowKey(src.Network, src.WalletAddress),
                entity =>
                {
                    if (!string.IsNullOrEmpty(src.DisplayName))
                        entity.DisplayName = src.DisplayName;

                    if (src.IncomingPaymentDefaults != null)
                        entity.IncomingPaymentDefaults = src.IncomingPaymentDefaults;

                    if (src.OutgoingPaymentDefaults != null)
                        entity.OutgoingPaymentDefaults = src.OutgoingPaymentDefaults;

                    return entity;
                });

            if (updatedEntity == null)
                throw new KeyNotFoundException();
        }

        public async Task DeleteAsync(string merchantId, BlockchainType network, string walletAddress)
        {
            MerchantWalletEntity deletedEntity = await _tableStorage.DeleteAsync(
                MerchantWalletEntity.ByMerchant.GeneratePartitionKey(merchantId),
                MerchantWalletEntity.ByMerchant.GenerateRowKey(network, walletAddress));

            if (deletedEntity == null)
                throw new KeyNotFoundException();

            await _indexByIdStorage.DeleteAsync(
                MerchantWalletEntity.IndexById.GeneratePartitionKey(deletedEntity.Id),
                MerchantWalletEntity.IndexById.GenerateRowKey());

            await _indexByAddressStorage.DeleteAsync(
                MerchantWalletEntity.IndexByAddress.GeneratePartitionKey(deletedEntity.Network,
                    deletedEntity.WalletAddress),
                MerchantWalletEntity.IndexByAddress.GenerateRowKey());
        }

        public async Task DeleteAsync(string merchantWalletId)
        {
            AzureIndex index = await _indexByIdStorage.GetDataAsync(
                MerchantWalletEntity.IndexById.GeneratePartitionKey(merchantWalletId),
                MerchantWalletEntity.IndexById.GenerateRowKey());

            if (index == null)
                throw new KeyNotFoundException();

            MerchantWalletEntity deletedEntity =
                await _tableStorage.DeleteAsync(index.PrimaryPartitionKey, index.PrimaryRowKey);

            await _indexByIdStorage.DeleteAsync(index);

            await _indexByAddressStorage.DeleteAsync(
                MerchantWalletEntity.IndexByAddress.GeneratePartitionKey(deletedEntity.Network,
                    deletedEntity.WalletAddress),
                MerchantWalletEntity.IndexByAddress.GenerateRowKey());
        }
    }
}
