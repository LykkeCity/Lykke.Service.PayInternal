﻿using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IDepositValidationService
    {
        Task<bool> ValidateDepositTransferAsync(ValidateDepositTransferCommand cmd);
    }
}