using System.Collections.Generic;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Full transfer object. Contain full transaction fields
    /// </summary>
    public interface ITransferInfo
    {
        /// <summary>
        /// List of source amount pairs.
        /// </summary>
        IEnumerable<IAddressAmount> SourceAddresses { get; set; }
        /// <summary>
        /// Destination address
        /// </summary>
        string DestinationAddress { get; set; }
        /// <summary>
        /// TotalAmount
        /// </summary>
        decimal Amount { get; set; }
        /// <summary>
        /// AssetId
        /// </summary>
        string AssetId { get; set; }
    }
}
