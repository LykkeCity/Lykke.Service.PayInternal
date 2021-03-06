﻿using AutoMapper;
using Common.Log;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Models.PaymentRequests;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.Service.PayInternal.Core.Exceptions;
using Lykke.Service.PayInternal.Models;
using ErrorResponse = Lykke.Common.Api.Contract.Responses.ErrorResponse;
using Lykke.Service.PayInternal.Core;
using LykkePay.Common.Validation;
using Common;

namespace Lykke.Service.PayInternal.Controllers
{
    [Route("api")]
    //todo: rethink controller implementation
    //too much contracts having now
    //Probably, in most cases we should use PaymentRequestDetailsModel as response 
    public class PaymentRequestsController : Controller
    {
        private readonly IPaymentRequestService _paymentRequestService;
        private readonly IRefundService _refundService;
        private readonly IAssetSettingsService _assetSettingsService;
        private readonly IPaymentRequestDetailsBuilder _paymentRequestDetailsBuilder;
        private readonly ILog _log;

        public PaymentRequestsController(
            [NotNull] IPaymentRequestService paymentRequestService,
            [NotNull] IRefundService refundService,
            [NotNull] IAssetSettingsService assetSettingsService,
            [NotNull] ILogFactory logFactory,
            [NotNull] IPaymentRequestDetailsBuilder paymentRequestDetailsBuilder)
        {
            _paymentRequestService = paymentRequestService ?? throw new ArgumentNullException(nameof(paymentRequestService));
            _refundService = refundService ?? throw new ArgumentNullException(nameof(refundService));
            _assetSettingsService = assetSettingsService ?? throw new ArgumentNullException(nameof(assetSettingsService));
            _paymentRequestDetailsBuilder = paymentRequestDetailsBuilder ?? throw new ArgumentNullException(nameof(paymentRequestDetailsBuilder));
            _log = logFactory.CreateLog(this);
        }

        /// <summary>
        /// Gets payment requests by filter
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="processingErrors">The processing errors (e.g. ?processingErrors=one&amp;processingErrors=two)</param>
        /// <param name="dateFrom">The date from which to take</param>
        /// <param name="dateTo">The date until which to take</param>
        /// <param name="take">The number of records to take</param>
        /// <response code="200">A collection of invoices.</response>
        /// <response code="400">Problem occured.</response>
        [HttpGet]
        [Route("paymentrequests/paymentsFilter")]
        [SwaggerOperation(nameof(GetByPaymentsFilter))]
        [ProducesResponseType(typeof(GetByPaymentsFilterResponse), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> GetByPaymentsFilter(
            [Required] [RowKey] string merchantId,
            IEnumerable<string> statuses,
            IEnumerable<string> processingErrors,
            DateTime? dateFrom,
            DateTime? dateTo,
            [Range(1, int.MaxValue)] int? take
        )
        {

            var statusesConverted = new List<PaymentRequestStatus>();

            if (statuses != null)
            {
                foreach (var status in statuses)
                {
                    try
                    {
                        statusesConverted.Add(status.Trim().ParseEnum<PaymentRequestStatus>());
                    }
                    catch (Exception)
                    {
                        return BadRequest(ErrorResponse.Create(
                            $"PaymentRequestStatus <{status}> is not valid. " +
                            $"Valid values are: {string.Join(",", Enum.GetNames(typeof(PaymentRequestStatus)))}"));
                    }
                }
            }

            var processingErrorsConverted = new List<PaymentRequestProcessingError>();

            if (processingErrors != null)
            {
                foreach (var processingError in processingErrors)
                {
                    try
                    {
                        processingErrorsConverted.Add(processingError.Trim().ParseEnum<PaymentRequestProcessingError>());
                    }
                    catch (Exception)
                    {
                        return BadRequest(ErrorResponse.Create(
                            $"PaymentRequestProcessingError <{processingError}> is not valid. " +
                            $"Valid values are: {string.Join(",", Enum.GetNames(typeof(PaymentRequestProcessingError)))}"));
                    }
                }
            }

            var paymentRequests = await _paymentRequestService.GetByFilterAsync(new PaymentsFilter
            {
                MerchantId = merchantId,
                Statuses = statusesConverted,
                ProcessingErrors = processingErrorsConverted,
                DateFrom = dateFrom,
                DateTo = dateTo
            });

            var result = new GetByPaymentsFilterResponse();

            if (take.HasValue)
            {
                result.HasMorePaymentRequests = paymentRequests.Count > take.Value;
                paymentRequests = paymentRequests.OrderByDescending(x => x.Timestamp).Take(take.Value).ToList();
            }
            else
            {
                paymentRequests = paymentRequests.OrderByDescending(x => x.Timestamp).ToList();
            }

            result.PaymeentRequests = paymentRequests;

            return Ok(result);
        }

        /// <summary>
        /// Gets whether merchant has any payment request
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        [HttpGet]
        [Route("paymentrequests/hasAnyPaymentRequest/{merchantId}")]
        [SwaggerOperation(nameof(HasAnyPaymentRequest))]
        [ProducesResponseType(typeof(IEnumerable<bool>), (int)HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> HasAnyPaymentRequest([Required] [RowKey] string merchantId)
        {
            var result = await _paymentRequestService.HasAnyPaymentRequestAsync(merchantId);

            return Ok(result);
        }

        /// <summary>
        /// Returns merchant payment requests.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The collection of merchant payment requests.</returns>
        /// <response code="200">The collection of merchant payment requests.</response>
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests")]
        [SwaggerOperation("PaymentRequestsGetAll")]
        [ProducesResponseType(typeof(List<PaymentRequestModel>), (int) HttpStatusCode.OK)]
        [ValidateModel]
        public async Task<IActionResult> GetAsync([Required, RowKey] string merchantId)
        {
            IReadOnlyList<IPaymentRequest> paymentRequests = await _paymentRequestService.GetAsync(merchantId);

            var model = Mapper.Map<List<PaymentRequestModel>>(paymentRequests);

            return Ok(model.Where(x => !string.IsNullOrEmpty(x.WalletAddress)));
        }

        /// <summary>
        /// Returns merchant payment request.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="404">Payment request not found.</response>
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        [SwaggerOperation("PaymentRequestsGetById")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> GetAsync(
            [Required, RowKey] string merchantId,
            [Required, RowKey] string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                return NotFound(ErrorResponse.Create("Couldn't find payment request"));

            var model = Mapper.Map<PaymentRequestModel>(paymentRequest);

            return Ok(model);
        }

        /// <summary>
        /// Returns merchant payment request details.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request details.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="404">Payment request not found.</response>
        [Obsolete("Need to remove")]
        [HttpGet]
        [Route("merchants/{merchantId}/paymentrequests/details/{paymentRequestId}")]
        [SwaggerOperation("PaymentRequestDetailsGetById")]
        [ProducesResponseType(typeof(PaymentRequestDetailsModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> GetDetailsAsync(
            [Required, RowKey] string merchantId,
            [Required, RowKey] string paymentRequestId)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.GetAsync(merchantId, paymentRequestId);

            if (paymentRequest == null)
                return NotFound(ErrorResponse.Create("Could not find payment request"));

            PaymentRequestRefund refundInfo =
                await _paymentRequestService.GetRefundInfoAsync(paymentRequest.WalletAddress);

            PaymentRequestDetailsModel model = await _paymentRequestDetailsBuilder.Build<
                PaymentRequestDetailsModel,
                PaymentRequestOrderModel,
                PaymentRequestTransactionModel,
                PaymentRequestRefundModel>(paymentRequest, refundInfo);

            return Ok(model);
        }

        /// <summary>
        ///  Returns merchant payment request by wallet address
        /// </summary>
        /// <param name="walletAddress">Wallet address</param>
        /// <returns>The payment request.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="404">Payment request not found.</response>
        [HttpGet]
        [Route("paymentrequests/byAddress/{walletAddress}")]
        [SwaggerOperation("PaymentRequestGetByWalletAddress")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> GetByAddressAsync([Required, RowKey] string walletAddress)
        {
            IPaymentRequest paymentRequest = await _paymentRequestService.FindAsync(walletAddress);

            if (paymentRequest == null)
                return NotFound(ErrorResponse.Create("Couldn't find payment request by wallet address"));

            var model = Mapper.Map<PaymentRequestModel>(paymentRequest);

            return Ok(model);
        }

        /// <summary>
        /// Creates a payment request and wallet.
        /// </summary>
        /// <param name="model">The payment request creation information.</param>
        /// <returns>The payment request.</returns>
        /// <response code="200">The payment request.</response>
        /// <response code="400">Invalid model.</response>
        [HttpPost]
        [Route("merchants/paymentrequests")]
        [SwaggerOperation("PaymentRequestsCreate")]
        [ProducesResponseType(typeof(PaymentRequestModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(PaymentRequestErrorModel), (int) HttpStatusCode.BadRequest)]
        [ProducesResponseType(typeof(PaymentRequestErrorModel), (int)HttpStatusCode.NotFound)]
        [ValidateModel]
        public async Task<IActionResult> CreateAsync([FromBody] CreatePaymentRequestModel model)
        {
            try
            {
                IReadOnlyList<string> settlementAssets =
                    await _assetSettingsService.ResolveSettlementAsync(model.MerchantId);

                if (!settlementAssets.Contains(model.SettlementAssetId))
                    return BadRequest(new PaymentRequestErrorModel
                        {Code = CreatePaymentRequestErrorType.SettlementAssetNotAvailable});

                IReadOnlyList<string> paymentAssets =
                    await _assetSettingsService.ResolvePaymentAsync(model.MerchantId, model.SettlementAssetId);

                if (!paymentAssets.Contains(model.PaymentAssetId))
                    return BadRequest(new PaymentRequestErrorModel
                        {Code = CreatePaymentRequestErrorType.PaymentAssetNotAvailable});

                var paymentRequest = Mapper.Map<PaymentRequest>(model);

                IPaymentRequest createdPaymentRequest = await _paymentRequestService.CreateAsync(paymentRequest);

                return Ok(Mapper.Map<PaymentRequestModel>(createdPaymentRequest));
            }
            catch (AssetUnknownException e)
            {
                _log.Error(e, e.Message, e.Asset);

                return BadRequest(new PaymentRequestErrorModel
                    {Code = CreatePaymentRequestErrorType.RequestValidationCommonError});
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.Error(e, e.Message, e.AssetId);

                return BadRequest(new PaymentRequestErrorModel
                    {Code = CreatePaymentRequestErrorType.RequestValidationCommonError});
            }
        }

        /// <summary>
        /// Starts refund process on payment request associated with source wallet address
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("merchants/paymentrequests/refunds")]
        [SwaggerOperation("Refund")]
        [ProducesResponseType(typeof(RefundResponseModel), (int) HttpStatusCode.OK)]
        [ProducesResponseType(typeof(RefundErrorModel), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> RefundAsync([FromBody] RefundRequestModel request)
        {
            try
            {
                RefundResult refundResult = await _refundService.ExecuteAsync(request.MerchantId,
                    request.PaymentRequestId, request.DestinationAddress);

                return Ok(Mapper.Map<RefundResponseModel>(refundResult));
            }
            catch (RefundOperationFailedException e)
            {
                _log.ErrorWithDetails(e, new {errors = e.TransferErrors});
            }
            catch (AssetUnknownException e)
            {
                _log.ErrorWithDetails(e, new {e.Asset});
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.ErrorWithDetails(e, new {e.AssetId});
            }
            catch (Exception e)
            {
                _log.ErrorWithDetails(e, request);

                if (e is RefundValidationException validationEx)
                {
                    _log.ErrorWithDetails(e, new {validationEx.ErrorType});

                    return BadRequest(new RefundErrorModel {Code = validationEx.ErrorType});
                }
            }

            return BadRequest(new RefundErrorModel {Code = RefundErrorType.Unknown});
        }

        /// <summary>
        /// Cancels the payment request
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="paymentRequestId"></param>
        /// <returns></returns>
        [HttpDelete]
        [Route("merchants/{merchantId}/paymentrequests/{paymentRequestId}")]
        [SwaggerOperation("Cancel")]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int) HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> CancelAsync(
            [Required, RowKey] string merchantId,
            [Required, RowKey] string paymentRequestId)
        {
            try
            {
                await _paymentRequestService.CancelAsync(merchantId, paymentRequestId);

                return NoContent();
            }
            catch (Exception e)
            {
                _log.ErrorWithDetails(e, new
                {
                    merchantId,
                    paymentRequestId
                });

                if (e is PaymentRequestNotFoundException notFoundEx)
                {
                    _log.ErrorWithDetails(notFoundEx, new
                    {
                        notFoundEx.WalletAddress,
                        notFoundEx.MerchantId,
                        notFoundEx.PaymentRequestId
                    });

                    return NotFound(ErrorResponse.Create(notFoundEx.Message));
                }

                if (e is NotAllowedStatusException notAllowedEx)
                {
                    _log.ErrorWithDetails(notAllowedEx,
                        new {status = notAllowedEx.Status.ToString()});

                    return BadRequest(ErrorResponse.Create(notAllowedEx.Message));
                }

                throw;
            }
        }

        /// <summary>
        /// Validates payment using default payer merchant's wallet
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <response code="204">Validated successfully</response>
        /// <response code="400">Insufficient funds</response>
        /// <response code="404">Payment request, merchant, payer merchant, default wallet or payment request wallet not found</response>
        /// <response code="501">Asset network support not implemented</response>
        [HttpPost]
        [Route("paymentrequests/prePayment")]
        [SwaggerOperation(nameof(PrePay))]
        [ProducesResponseType(typeof(void), (int)HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotImplemented)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> PrePay([FromBody] PrePaymentModel request)
        {
            try
            {
                await _paymentRequestService.PrePayAsync(Mapper.Map<PaymentCommand>(request));

                return NoContent();
            }
            catch (InsufficientFundsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.AssetId,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.ErrorWithDetails(e, new { e.AssetId });

                return StatusCode((int)HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new {e.WalletAddress});

                return NotFound(ErrorResponse.Create(e.Message));
            }
        }

        /// <summary>
        /// Executes payment using default payer merchant's wallet
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <response code="204">Payment executed successfully</response>
        /// <response code="400">Payment failed</response>
        /// <response code="404">Payment request, merchant, default wallet, payment request wallet not found or couldn't get payment request lock</response>
        /// <response code="501">Asset network support not implemented</response>
        [HttpPost]
        [Route("paymentrequests/payment")]
        [SwaggerOperation(nameof(Pay))]
        [ProducesResponseType(typeof(void), (int) HttpStatusCode.NoContent)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotFound)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.NotImplemented)]
        [ProducesResponseType(typeof(ErrorResponse), (int)HttpStatusCode.BadRequest)]
        [ValidateModel]
        public async Task<IActionResult> Pay([FromBody] PaymentModel request)
        {
            try
            {
                await _paymentRequestService.PayAsync(Mapper.Map<PaymentCommand>(request));

                return NoContent();
            }
            catch (InsufficientFundsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.AssetId,
                    e.WalletAddress
                });

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (PaymentRequestNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.PaymentRequestId,
                    e.MerchantId,
                    e.WalletAddress
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (AssetNetworkNotDefinedException e)
            {
                _log.ErrorWithDetails(e, new {e.AssetId});

                return StatusCode((int) HttpStatusCode.NotImplemented, ErrorResponse.Create(e.Message));
            }
            catch (MultipleDefaultMerchantWalletsException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (DefaultMerchantWalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new
                {
                    e.MerchantId,
                    e.AssetId,
                    e.PaymentDirection
                });

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (WalletNotFoundException e)
            {
                _log.ErrorWithDetails(e, new {e.WalletAddress});

                return NotFound(ErrorResponse.Create(e.Message));
            }
            catch (PaymentOperationFailedException e)
            {
                _log.ErrorWithDetails(e, new {errors = e.TransferErrors});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
            catch (DistributedLockAcquireException e)
            {
                _log.ErrorWithDetails(e, new {e.Key});

                return BadRequest(ErrorResponse.Create(e.Message));
            }
        }
    }
}
