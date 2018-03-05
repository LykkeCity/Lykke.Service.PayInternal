using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// The transfer request data for the case when we need to perform multiple bijective operations (i.e., from each of the sources we will transfer money to the single destination).
    /// </summary>
    [UsedImplicitly]
    public class TransferRequestMultiBijectiveModel
    {
        /// <summary>
        /// ID of the Merchant who is responsible for money transfer.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public string MerchantId { get; set; }
        /// <summary>
        /// The list of pair addresses for source-destination with the amount of transfer specified.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public List<BiAddressAmount> BiAddresses { get; set; }
    }
}
