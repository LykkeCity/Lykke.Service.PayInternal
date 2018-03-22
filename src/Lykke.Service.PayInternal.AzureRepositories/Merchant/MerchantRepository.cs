﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using Lykke.Service.PayInternal.Core.Domain.Merchant;

namespace Lykke.Service.PayInternal.AzureRepositories.Merchant
{
    public class MerchantRepository : IMerchantRepository
    {
        private readonly INoSQLTableStorage<MerchantEntity> _storage;

        public MerchantRepository(
            INoSQLTableStorage<MerchantEntity> storage)
        {
            _storage = storage;
        }

        public async Task<IReadOnlyList<IMerchant>> GetAsync()
        {
            IEnumerable<MerchantEntity> entities =
                await _storage.GetDataAsync(GetPartitionKey());

            return entities.ToList();
        }
        
        public async Task<IMerchant> GetAsync(string merchantName)
        {
            return await _storage.GetDataAsync(GetPartitionKey(), GetRowKey(merchantName));
        }

        public async Task<IMerchant> InsertAsync(IMerchant merchant)
        {
            var entity = new MerchantEntity(GetPartitionKey(), GetRowKey(merchant.Name));
            entity.Map(merchant);

            await _storage.InsertAsync(entity);

            return entity;
        }
        
        public async Task ReplaceAsync(IMerchant merchant)
        {
            var entity = new MerchantEntity(GetPartitionKey(), GetRowKey(merchant.Name));
            entity.Map(merchant);
            
            entity.ETag = "*";
            
            await _storage.ReplaceAsync(entity);
        }

        public async Task DeleteAsync(string merchantName)
        {
            await _storage.DeleteAsync(GetPartitionKey(), GetRowKey(merchantName));
        }

        private static string GetPartitionKey()
            => "M"; // TODO: Change to 'Merchant'

        private static string GetRowKey(string merchantName)
            => merchantName;
    }
}
