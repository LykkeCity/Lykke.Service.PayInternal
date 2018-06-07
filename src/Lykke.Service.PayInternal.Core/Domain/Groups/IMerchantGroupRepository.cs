﻿using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Groups
{
    public interface IMerchantGroupRepository
    {
        Task<IMerchantGroup> CreateAsync(IMerchantGroup src);

        Task<IMerchantGroup> GetAsync(string id);

        Task DeleteAsync(string id);
    }
}
