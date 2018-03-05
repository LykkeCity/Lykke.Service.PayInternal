using System;

namespace Lykke.Service.PayInternal.Core.Exceptions
{
    public class BtcTransferException : Exception
    {
        public BtcTransferException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; set; }
    }
}
