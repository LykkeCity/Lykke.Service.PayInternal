using Lykke.Service.PayInternal.Client.Models.Transfer;
using Refit;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Client.Api
{
    public interface ITransferReportApi
    {
        [Post("/api/transfers/updateStatus")]
        Task<TransferRequest> UpdateTransferStatusAsync([Body] UpdateTransferStatusModel model);

        [Get("/api/transfers/{transferId}/getStatus")]
        Task<TransferRequest> GetTransferStatusAsync(string transferId);
    }
}
