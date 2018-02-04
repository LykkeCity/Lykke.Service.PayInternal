﻿using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequest;

namespace Lykke.Service.PayInternal.Core.Services
{
    public interface IPaymentRequestService
    {
        Task<IReadOnlyList<IPaymentRequest>> GetAsync(string merchantId);
        
        Task<IPaymentRequest> GetAsync(string merchantId, string paymentRequestId);

        Task<IPaymentRequest> CreateAsync(IPaymentRequest paymentRequest);
    }
}
