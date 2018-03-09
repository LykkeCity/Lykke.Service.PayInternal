﻿using System;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.Refunds;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    public class RefundsController : Controller
    {
        // ReSharper disable once NotAccessedField.Local
        private readonly ITransferService _transferService;
        // ReSharper disable once NotAccessedField.Local
        private readonly ILog _log;

        public RefundsController(ITransferService transferService, ILog log)
        {
            _transferService = transferService ?? throw new ArgumentNullException(nameof(transferService));
            _log = log ?? throw new ArgumentNullException(nameof(log));
        }

        /// <summary>
        /// Creates a new refund request for the specified payment request and (optionally) wallet address.
        /// </summary>
        /// <param name="paymentRequestId">The payment request ID.</param>
        /// <param name="walletAddress">The wallet address.</param>
        /// <returns></returns>
        [HttpPost]
        [Route("refunds/Refund")]
        [SwaggerOperation("Refund")]
        [ProducesResponseType(typeof(RefundResponse), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> CreateRefundRequestAsync(string paymentRequestId, string walletAddress)
        {
            await Task.Delay(50);

            if (string.IsNullOrWhiteSpace(paymentRequestId))
                return BadRequest(ErrorResponse.Create("Payment request ID can not be null."));

            return Ok(new RefundResponse());
        }

        /// <summary>
        /// Gets the current state of the specified refund request.
        /// </summary>
        /// <param name="refundId">The refund request ID.</param>
        /// <returns></returns>
        [HttpGet]
        [Route("refunds/GetRefund")]
        [SwaggerOperation("GetRefund")]
        [ProducesResponseType(typeof(RefundResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.InternalServerError)]
        public async Task<IActionResult> GetRefundAsync(string refundId)
        {
            await Task.Delay(50);

            if (string.IsNullOrWhiteSpace(refundId))
                return BadRequest(ErrorResponse.Create("Refund request ID can not be null."));

            return Ok(new RefundResponse());
        }
    }
}