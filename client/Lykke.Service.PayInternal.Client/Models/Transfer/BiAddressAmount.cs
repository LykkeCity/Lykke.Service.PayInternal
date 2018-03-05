using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// Represents the object containing the info about the source address, destination address and the amount to transfer between.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class BiAddressAmount
    {
        /// <summary>
        /// Source address
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public string SourceAddress { get; set; }
        /// <summary>
        /// Destination address
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public string DestinationAddress { get; set; }
        /// <summary>
        /// Amount of asset to transfer
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public decimal Amount { get; set; }
    }
}
