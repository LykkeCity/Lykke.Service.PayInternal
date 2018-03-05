using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.Transfer;

namespace Lykke.Service.PayInternal.Models
{
    public class UpdateTransferStatusModel
    {
        [Required]
        public string TransferId { get; set; }
        [Required]
        public TransferStatus TransferStatus { get; set; }
        [Required]
        public TransferStatusError TransferStatusError { get; set; }

        public ITransferRequest ToTransferRequest()
        {
            return new TransferRequest
            {
                TransferId = TransferId,
                TransferStatus = TransferStatus,
                TransferStatusError = TransferStatusError
            };
        }
    }
}
