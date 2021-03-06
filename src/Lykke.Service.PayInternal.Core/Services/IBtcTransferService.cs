﻿using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IBtcTransferService
    {
        Task<string> ExecuteAsync(BtcTransfer transfer);
    }
}
