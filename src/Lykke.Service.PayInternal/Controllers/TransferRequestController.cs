using Common;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class TransferRequestController : Controller
    {
        private readonly ITransferRequestService _transferRequestService;
        public TransferRequestController(ITransferRequestService transferRequestService)
        {
            _transferRequestService = transferRequestService;
        }

        /// <summary>
        /// Request transfer from a list of some source address(es) to a list of destination address(es) with amounts specified.
        /// </summary>
        /// <param name="requestModel">The data containing serialized model object.</param>
        /// <returns>The transfer info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model (description is also provided).</response>
        [HttpPost]
        [Route("merchants/transferCrosswise")]
        [SwaggerOperation("TransferCrosswise")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferCrosswiseAsync([FromBody] TransferRequestCrosswiseModel requestModel)
        {
            // Block of common validation. Note: numeric values and enums are "0" by default even if they are not presented in given request object.
            if (!ModelState.IsValid)
                return BadRequest(
                    new ErrorResponse().AddErrors(ModelState));

            if (!requestModel.Sources.Any())
                return BadRequest(
                    ErrorResponse.Create("List of source addresses can not be empty."));

            if (!requestModel.Destinations.Any())
                return BadRequest(
                    ErrorResponse.Create("List of destination addresses can not be empty."));

            var requestValidationError = requestModel.CheckAmountsValidity();
            if (!string.IsNullOrEmpty(requestValidationError))
                return BadRequest(
                    ErrorResponse.Create(requestValidationError));

            var transferRequest = requestModel.ToTransferRequest();
            if (transferRequest == null)
                return BadRequest(
                    ErrorResponse.Create("Transfer model is malformed. Checkup list of sources and destinations."));

            // The main work
            var transferRequestResult = await _transferRequestService.CreateTransferCrosswiseAsync(transferRequest);
            if (transferRequestResult.TransferStatus == TransferStatus.Error)
                return BadRequest(
                    ErrorResponse.Create("Execution of the transfer was terminated for some reasons. Please, note, that some of its transactions may have passed with success. The transfer data is attached: " +
                        transferRequestResult.ToJson()));

            return Ok(transferRequestResult);
        }

        /// <summary>
        /// Request transfer consistent of a list of signle-source and single-destination transactions with amounts specified for every address pair.
        /// </summary>
        /// <param name="requestModel">The data containing serialized model object.</param>
        /// <returns>The transfer info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model (description is also provided).</response>
        [HttpPost]
        [Route("merchants/transferMultiBijective")]
        [SwaggerOperation("TransferMultiBijective")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> TransferMultiBijectiveAsync([FromBody] TransferRequestMultiBijectiveModel requestModel)
        {
            // Block of common validation. Note: numeric values and enums are "0" by default even if they are not presented in given request object.
            if (!ModelState.IsValid)
                return BadRequest(
                    new ErrorResponse().AddErrors(ModelState));

            var requestValidationError = requestModel.CheckAmountsValidity();
            if (!string.IsNullOrEmpty(requestValidationError))
                return BadRequest(
                    ErrorResponse.Create(requestValidationError));

            var transferRequest = requestModel.ToTransferRequest();
            if (transferRequest == null)
                return BadRequest(
                    ErrorResponse.Create("Transfer model is malformed. Checkup list of sources and destinations."));

            // The main work
            var transferRequestResult = await _transferRequestService.CreateTransferCrosswiseAsync(transferRequest);
            if (transferRequestResult.TransferStatus == TransferStatus.Error)
                return BadRequest(
                    ErrorResponse.Create("Execution of the transfer was terminated for some reasons. Please, note, that some of its transactions may have passed with success. The transfer data is attached: " +
                        transferRequestResult.ToJson()));

            return Ok(transferRequestResult);
        }
    }
}
