using JetBrains.Annotations;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// Represents info about what amount of money should be transfered to\from the address.
    /// </summary>
    [UsedImplicitly]
    public class AddressAmount
    {
        /// <summary>
        /// The address.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public string Address { get; set; }
        /// <summary>
        /// The amount
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public decimal Amount { get; set; }
    }
}
