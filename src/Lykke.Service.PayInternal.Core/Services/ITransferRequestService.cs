using Lykke.Service.PayInternal.Core.Domain.Transfer;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Services
{
    /// <summary>
    /// Transfer Request Service. Make Transfers. Update States. Get Transfer Status.
    /// </summary>
    public interface ITransferRequestService
    {
        /// <summary>
        /// Create crosswise transfer using transfer request
        /// </summary>
        /// <param name="transferRequest">transfer request</param>
        /// <returns></returns>

        Task<ITransferRequest> CreateTransferCrosswiseAsync(ITransferRequest transferRequest);

        /// <summary>
        /// Update transfer status. Other fields will be ignored
        /// </summary>
        /// <param name="transfer">Transfer</param>
        /// <returns></returns>

        Task<ITransferRequest> UpdateTransferStatusAsync(ITransferRequest transfer);

        /// <summary>
        /// Update / insert transfer entity
        /// </summary>
        /// <param name="transfer">transfer entity</param>
        /// <returns></returns>

        Task<ITransferRequest> UpdateTransferAsync(ITransferRequest transfer);

        /// <summary>
        /// Get transfer entity
        /// </summary>
        /// <param name="transferId">shord transfer structure</param>
        /// <returns></returns>

        Task<ITransferRequest> GetTransferInfoAsync(string transferId);

    }
}
