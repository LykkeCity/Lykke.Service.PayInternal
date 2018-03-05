using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Core.Domain.Transfer
{
    /// <summary>
    /// Repository for transfer entity
    /// </summary>
    public interface ITransferRequestRepository
    {
        /// <summary>
        /// Get all transfers
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<ITransferRequest>> GetAllAsync();
        /// <summary>
        /// Get a transfer entity with all transactions
        /// </summary>
        /// <param name="transferRequestId">Transfer Id</param>
        /// <returns></returns>
        Task<ITransferRequest> GetAsync(string transferRequestId);
        /// <summary>
        /// et a transfer entity with a specify transaction
        /// </summary>
        /// <param name="transferRequestId">Transfer Id</param>
        /// <param name="transactionHash">PRC Transaction Hash</param>
        /// <returns></returns>
        Task<ITransferRequest> GetAsync(string transferRequestId, string transactionHash);
        /// <summary>
        /// Save transfer info
        /// </summary>
        /// <param name="transferInfo">Transfer info</param>
        Task SaveAsync(ITransferRequest transferInfo);
    }
}
