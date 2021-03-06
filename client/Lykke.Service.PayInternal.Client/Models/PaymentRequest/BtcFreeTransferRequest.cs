﻿using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Client.Models.PaymentRequest
{
    public class BtcFreeTransferRequest
    {
        public IEnumerable<BtcTransferSourceInfo> Sources { get; set; }
        public string DestAddress { get; set; }
    }
}
