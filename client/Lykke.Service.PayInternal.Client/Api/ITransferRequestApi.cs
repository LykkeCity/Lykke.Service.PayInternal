using Lykke.Service.PayInternal.Client.Models.Transfer;
using Refit;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Client.Api
{
    /// <summary>
    /// The public client interface for Transfer Request API
    /// </summary>
    public interface ITransferRequestApi
    {

        /// <summary>
        /// Request transfer from a list of some source address(es) to a list of destination address(es) with amounts specified.
        /// </summary>
        /// <param name="transfer">Basic information about the transfer request to be created.</param>
        /// <returns></returns>
        [Post("api/merchants/transferCrosswise")]
        Task<TransferRequest> TransferCrosswiseAsync([Body] TransferCrosswiseModel transfer);

        /// <summary>
        /// Request transfer consistent of a list of signle-source and single-destination transactions with amounts specified for every address pair.
        /// </summary>
        /// <param name="transfer">Basic information about the transfer request to be created.</param>
        /// <returns></returns>
        [Post("api/merchants/transferMultiBijective")]
        Task<TransferRequest> TransferMultiBijectiveAsync([Body] TransferRequestMultiBijectiveModel transfer);
    }
}
