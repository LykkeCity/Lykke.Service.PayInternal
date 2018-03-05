﻿using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Domain.Transfer;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Extensions;
using Lykke.Service.PayInternal.Models;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Net;
using System.Threading.Tasks;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api/transfers")]
    public class TransferReportController : Controller
    {
        private readonly ITransferRequestService _transferRequestService;
        public TransferReportController(ITransferRequestService transferRequestService)
        {
            _transferRequestService = transferRequestService;
        }

        /// <summary>
        /// Update transfer status.
        /// </summary>
        /// <param name="model">Transfer model.</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("updateStatus")]
        [SwaggerOperation("TransferUpdateStatus")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> UpdateTransferStatusAsync([FromBody] UpdateTransferStatusModel model)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            return Ok(await _transferRequestService.UpdateTransferStatusAsync(model.ToTransferRequest()));
        }

        /// <summary>
        /// Get transfer status.
        /// </summary>
        /// <param name="transferId">The ID of the transfer to check.</param>
        /// <returns>The Transfer Info.</returns>
        /// <response code="200">The Transfer Info.</response>
        /// <response code="400">Invalid model.</response>
        [HttpGet]
        [Route("{transferId}/getStatus")]
        [SwaggerOperation("TransferGetStatus")]
        [ProducesResponseType(typeof(ITransferRequest), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        public async Task<IActionResult> GetTransferStatusAsync(string transferId)
        {
            if (!ModelState.IsValid)
                return BadRequest(new ErrorResponse().AddErrors(ModelState));

            var result = await _transferRequestService.GetTransferInfoAsync(transferId);

            if (result == null)
                return NotFound("Can't find the request with the given ID.");

            return Ok(result);
        }
    }
}
