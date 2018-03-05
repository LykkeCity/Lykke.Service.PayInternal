using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Services.Domain
{
    public class TransactionRequest : ITransactionRequest
    {
        public string TransactionHash { get; set; }
        public List<IAddressAmount> SourceAmounts { get; set; }
        public string DestinationAddress { get; set; }
        public int CountConfirm { get; set; }
        public decimal Amount { get; set; }
        public string AssetId { get; set; }
    }
}
