using System.ComponentModel.DataAnnotations;
using JetBrains.Annotations;

namespace Lykke.Service.PayInternal.Client.Models.Transfer
{
    /// <summary>
    /// Represents the data related to the transfer update process.
    /// </summary>
    [UsedImplicitly]
    public class UpdateTransferStatusModel
    {
        /// <summary>
        /// ID of the transfer to update.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public string TransferId { get; set; }
        /// <summary>
        /// The new state of the transfer.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public TransferStatus TransferStatus { get; set; }
        /// <summary>
        /// The new error info of the transfer.
        /// </summary>
        [Required]
        // ReSharper disable once UnusedMember.Global
        public TransferStatusError TransferStatusError { get; set; }

       
    }

}
