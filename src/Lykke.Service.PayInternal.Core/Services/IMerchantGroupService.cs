﻿using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Groups;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IMerchantGroupService
    {
        Task<IMerchantGroup> CreateAsync(IMerchantGroup src);
        Task<IMerchantGroup> GetAsync(string id);
    }
}
