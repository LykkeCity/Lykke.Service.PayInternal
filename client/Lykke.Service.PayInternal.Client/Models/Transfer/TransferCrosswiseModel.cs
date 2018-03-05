using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// Represents the minimal set of data for creation of the crosswise transfer request.
    /// </summary>
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TransferCrosswiseModel
    {
        /// <summary>
        /// ID of the Merchant who is responsible for money transfer.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public string MerchantId { get; set; }
        /// <summary>
        /// The side who pays the fee. If Merchant pays, then the Client receives the full sum
        /// of money as it is pointed in request for the transfer, and the fee is additionaly 
        /// paid from Merchant's wallets(s). Alternatively, the Client gets his (money - fee),
        /// and the Merchant is not responsible for anything.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public List<AddressAmount> Sources { get; set; }
        /// <summary>
        /// The list of destination addresses, (supposably) belonging to the Client.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public List<AddressAmount> Destinations { get; set; }
    }
}

