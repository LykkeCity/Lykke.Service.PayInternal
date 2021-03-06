﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Service.PayInternal.Client.Models.Asset;
using Lykke.Service.PayInternal.Client.Models.AssetRates;
using Lykke.Service.PayInternal.Client.Models.Cashout;
using Lykke.Service.PayInternal.Client.Models.Exchange;
using Lykke.Service.PayInternal.Client.Models.Markup;
using Lykke.Service.PayInternal.Client.Models.Order;
using Lykke.Service.PayInternal.Client.Models.PaymentRequest;
using Lykke.Service.PayInternal.Client.Models.SupervisorMembership;
using Lykke.Service.PayInternal.Client.Models.Transactions;
using Lykke.Service.PayInternal.Client.Models.Wallets;
using Lykke.Service.PayInternal.Client.Models.File;
using Lykke.Service.PayInternal.Client.Models.MerchantWallets;
using Lykke.Service.PayInternal.Client.Models.Transactions.Ethereum;

namespace Lykke.Service.PayInternal.Client
{
    /// <summary>
    /// Pay Internal client interface
    /// </summary>
    public interface IPayInternalClient
    {
        /// <summary>
        /// Returns wallet addresses that are not considered as expired
        /// </summary>
        /// <returns></returns>
        Task<IEnumerable<WalletStateResponse>> GetNotExpiredWalletsAsync();

        /// <summary>
        /// Creates transaction of payment type
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task CreatePaymentTransactionAsync(CreateTransactionRequest request);

        /// <summary>
        /// Updates transaction
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task UpdateTransactionAsync(UpdateTransactionRequest request);

        /// <summary>
        /// Returns an order by id.
        /// </summary>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <param name="orderId">The order id.</param>
        /// <returns>The payment request order.</returns>
        Task<OrderModel> GetOrderAsync(string paymentRequestId, string orderId);

        /// <summary>
        /// Creates an order if it does not exist or expired.
        /// </summary>
        /// <param name="model">The order creation information.</param>
        /// <returns>An active order related with payment request.</returns>
        Task<OrderModel> ChechoutOrderAsync(ChechoutRequestModel model);

        /// <summary>
        /// Get calculated amount to show amount to pay in paymentAsset
        /// </summary>
        /// <param name="model">The request in order to get amount</param>
        Task<CalculatedAmountResponse> GetCalculatedAmountInfoAsync(GetCalculatedAmountInfoRequest model);

        /// <summary>
        /// Gets whether merchant has any payment request
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        Task<bool> HasAnyPaymentRequestAsync(string merchantId);

        /// <summary>
        /// Gets payment requests by filter
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        /// <param name="statuses">The statuses (e.g. ?statuses=one&amp;statuses=two)</param>
        /// <param name="processingErrors">The processing errors (e.g. ?processingErrors=one&amp;processingErrors=two)</param>
        /// <param name="dateFrom">The date from which to take</param>
        /// <param name="dateTo">The date until which to take</param>
        /// <param name="take">The number of records to take</param>
        Task<GetByPaymentsFilterResponse> GetByPaymentsFilterAsync(
            string merchantId,
            IEnumerable<string> statuses,
            IEnumerable<string> processingErrors,
            DateTime? dateFrom,
            DateTime? dateTo,
            int? take
        );

        /// <summary>
        /// Returns merchant payment requests.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The collection of merchant payment requests.</returns>
        Task<IReadOnlyList<PaymentRequestModel>> GetPaymentRequestsAsync(string merchantId);

        /// <summary>
        /// Returns merchant payment request.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request.</returns>
        Task<PaymentRequestModel> GetPaymentRequestAsync(string merchantId, string paymentRequestId);

        /// <summary>
        /// Returns merchant payment request details.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="paymentRequestId">The payment request id.</param>
        /// <returns>The payment request details.</returns>
        Task<PaymentRequestDetailsModel> GetPaymentRequestDetailsAsync(string merchantId, string paymentRequestId);

        /// <summary>
        ///  Returns merchant payment request by wallet address.
        /// </summary>
        /// <param name="walletAddress">Wallet address</param>
        /// <returns>The payment request.</returns>
        Task<PaymentRequestModel> GetPaymentRequestByAddressAsync(string walletAddress);

        /// <summary>
        /// Creates a payment request and wallet.
        /// </summary>
        /// <param name="model">The payment request creation information.</param>
        /// <returns>The payment request.</returns>
        Task<PaymentRequestModel> CreatePaymentRequestAsync(CreatePaymentRequestModel model);

        /// <summary>
        /// Transfers BTC from source addresses with amount provided to destination address without LykkePay fees
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<BtcTransferResponse> BtcFreeTransferAsync(BtcFreeTransferRequest request);

        /// <summary>
        /// Finds and returns all monitored (i.e., not expired and not fully confirmed yet) transactions.
        /// </summary>
        /// <returns>The list of monitored transactions.</returns>
        Task<IEnumerable<TransactionStateResponse>> GetAllMonitoredTransactionsAsync();

        /// <summary>
        /// Initiates a refund for the specified payment request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task<RefundResponse> RefundAsync(RefundRequestModel request);

        /// <summary>
        /// Marks transaction as expired
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetTransactionExpiredAsync(TransactionExpiredRequest request);

        /// <summary>
        /// Returns list of assets available for merchant according to availability type and general asset settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        [Obsolete("Use ResolveSettlementAssetsAsync and ResolvePaymentAssetsAsync instead")]
        Task<AvailableAssetsResponse> ResolveAvailableAssetsAsync(string merchantId, AssetAvailabilityType type);

        /// <summary>
        /// Returns available settlement assets for merchant
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        Task<AvailableAssetsResponse> GetAvailableSettlementAssetsAsync(string merchantId);

        /// <summary>
        /// Returns available payment assets for merchant and settlement asset id
        /// </summary>
        /// <param name="merchantId"></param>
        /// <param name="settlementAssetId"></param>
        /// <returns></returns>
        Task<AvailableAssetsResponse> GetAvailablePaymentAssetsAsync(string merchantId, string settlementAssetId);

        /// <summary>
        /// Returns asset general settings
        /// </summary>
        Task<IEnumerable<AssetGeneralSettingsResponse>> GetAssetGeneralSettingsAsync();

        /// <summary>
        /// Returns merchant asset settings
        /// </summary>
        /// <param name="merchantId"></param>
        /// <returns></returns>
        Task<AssetMerchantSettingsResponse> GetAssetMerchantSettingsAsync(string merchantId);

        /// <summary>
        ///  Updates asset general settings
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetAssetGeneralSettingsAsync(UpdateAssetGeneralSettingsRequest request);

        /// <summary>
        /// Updates merchant asset settings
        /// </summary>
        /// <param name="settingsRequest"></param>
        /// <returns></returns>
        Task SetAssetMerchantSettingsAsync(UpdateAssetMerchantSettingsRequest settingsRequest);

        /// <summary>
        /// Cancels payment request
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="paymentRequestId">Payment request id</param>
        /// <returns></returns>
        Task CancelAsync(string merchantId, string paymentRequestId);

        /// <summary>
        /// Marks wallet as expired
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        Task SetWalletExpiredAsync(BlockchainWalletExpiredRequest request);

        /// <summary>
        /// Returns markup values for merchant and asset pair
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        Task<MarkupResponse> ResolveMarkupByMerchantAsync(string merchantId, string assetPairId);

        /// <summary>
        /// Returns the default markup values for all asset pairs
        /// </summary>
        /// <returns></returns>
        Task<IReadOnlyList<MarkupResponse>> GetDefaultMarkupsAsync();

        /// <summary>
        /// Returns the default markup values for asset pair id
        /// </summary>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        Task<MarkupResponse> GetDefaultMarkupAsync(string assetPairId);

        /// <summary>
        /// Updates markup values for asset pair id
        /// </summary>
        /// <param name="assetPairId">Asset pair id</param>
        /// <param name="request">Markup values</param>
        /// <returns></returns>
        Task SetDefaultMarkupAsync(string assetPairId, UpdateMarkupRequest request);

        /// <summary>
        /// Returns all markup values for merchant
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <returns></returns>
        Task<IReadOnlyList<MarkupResponse>> GetMarkupsForMerchantAsync(string merchantId);

        /// <summary>
        /// Returns markup value for merchant and asset pair id
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="assetPairId">Asset pair id</param>
        /// <returns></returns>
        Task<MarkupResponse> GetMarkupForMerchantAsync(string merchantId, string assetPairId);

        /// <summary>
        /// Updates markup values for merchant and asset pair
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <param name="assetPairId">Asset pair id</param>
        /// <param name="request">Markup values</param>
        /// <returns></returns>
        Task SetMarkupForMerchantAsync(string merchantId, string assetPairId, UpdateMarkupRequest request);

        /// <summary>
        /// Creates supervisor membership
        /// </summary>
        /// <param name="request">Supervisor membership creation details</param>
        /// <returns>Supervisor membership details</returns>
        Task<SupervisorMembershipResponse> AddSupervisorMembershipAsync(AddSupervisorMembershipRequest request);

        /// <summary>
        /// Returns supervisor membership details for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <returns>Supervisor membership details</returns>
        Task<SupervisorMembershipResponse> GetSupervisorMembershipAsync(string employeeId);

        /// <summary>
        /// Updates supervisor membership
        /// </summary>
        /// <param name="request">Supervisor membership update details</param>
        /// <returns></returns>
        Task UpdateSupervisorMembershipAsync(UpdateSupervisorMembershipRequest request);

        /// <summary>
        /// Removes supervisor membership for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <returns></returns>
        Task RemoveSupervisorMembershipAsync(string employeeId);

        /// <summary>
        /// Creates supervisor membership
        /// </summary>
        /// <param name="request">Supervisor membership creation details</param>
        /// <returns>Supervisor membership details</returns>
        Task<MerchantsSupervisorMembershipResponse> AddSupervisorMembershipForMerchantsAsync(AddSupervisorMembershipMerchantsRequest request);

        /// <summary>
        /// Returns supervisor membership details for employee
        /// </summary>
        /// <param name="employeeId">Employee id</param>
        /// <returns>Supervisor membership details</returns>
        Task<MerchantsSupervisorMembershipResponse> GetSupervisorMembershipWithMerchantsAsync(string employeeId);

        /// <summary>
        /// Returns a collection of merchant files.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <returns>The collection of file info.</returns>
        Task<IEnumerable<FileInfoModel>> GetFilesAsync(string merchantId);

        /// <summary>
        /// Returns file content.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="fileId">The file id.</param>
        Task<byte[]> GetFileAsync(string merchantId, string fileId);

        /// <summary>
        /// Get merchant logo url
        /// </summary>
        /// <param name="merchantId">The merchant id</param>
        Task<string> GetMerchantLogoUrl(string merchantId);

        /// <summary>
        /// Saves file.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="content">The file content.</param>
        /// <param name="fileName">The file name with extension.</param>
        /// <param name="contentType">The file mime type.</param>
        Task UploadFileAsync(string merchantId, byte[] content, string fileName, string contentType);

        /// <summary>
        /// Deletes file.
        /// </summary>
        /// <param name="merchantId">The merchant id.</param>
        /// <param name="fileId">The file id.</param>
        Task DeleteFileAsync(string merchantId, string fileId);

        /// <summary>
        /// Creates new merchant wallet
        /// </summary>
        /// <param name="request">>Merchant wallet creation details</param>
        /// <returns></returns>
        Task<MerchantWalletResponse> CreateMerchantWalletAsync(CreateMerchantWalletRequest request);

        /// <summary>
        /// Deletes merchant wallet
        /// </summary>
        /// <param name="merchantWalletId">Merchant wallet id</param>
        /// <returns></returns>
        Task DeleteMerchantWalletAsync(string merchantWalletId);

        /// <summary>
        /// Updates default assets for merchant wallet
        /// </summary>
        /// <param name="request">Merchant wallet default assets update details</param>
        /// <returns></returns>
        Task SetMerchantWalletDefaultAssetsAsync(UpdateMerchantWalletDefaultAssetsRequest request);

        /// <summary>
        /// Returns list of merchant wallets
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <returns></returns>
        Task<IEnumerable<MerchantWalletResponse>> GetMerchantWalletsAsync(string merchantId);

        /// <summary>
        /// Returns default merchant wallet for given asset and payment direction
        /// </summary>
        /// <param name="merchantId">Merchant Id</param>
        /// <param name="assetId">Asset id</param>
        /// <param name="paymentDirection">Payment direction</param>
        /// <returns></returns>
        Task<MerchantWalletResponse> GetDefaultMerchantWalletAsync(string merchantId, string assetId, PaymentDirection paymentDirection);

        /// <summary>
        /// Returns balances for all merchant's wallets
        /// </summary>
        /// <param name="merchantId">Merchant id</param>
        /// <returns></returns>
        Task<IEnumerable<MerchantWalletBalanceResponse>> GetMerchantWalletBalancesAsync(string merchantId);

        /// <summary>
        /// Adds new rate for given asset pair
        /// </summary>
        /// <param name="request">New asset pair request details</param>
        /// <returns></returns>
        Task<AssetRateResponse> AddAssetPairRateAsync(AddAssetRateRequest request);

        /// <summary>
        /// Returns current rate for given asset pair
        /// </summary>
        /// <param name="baseAssetId">Base asset id</param>
        /// <param name="quotingAssetId">Quoting asset id</param>
        /// <returns></returns>
        Task<AssetRateResponse> GetCurrentAssetPairRateAsync(string baseAssetId, string quotingAssetId);

        /// <summary>
        /// Executes payment using default payer merchant's wallet
        /// </summary>
        /// <param name="request">Payment details</param>
        /// <returns></returns>
        Task PayAsync(PaymentRequest request);

        /// <summary>
        /// Validates payment using default payer merchant's wallet
        /// </summary>
        /// <param name="request">Prepayment request details</param>
        /// <returns></returns>
        Task PrePayAsync(PrePaymentRequest request);

        /// <summary>
        /// Executes exchange
        /// </summary>
        /// <param name="request">Exchange operation details</param>
        /// <returns>Exchange execution result</returns>
        Task<ExchangeResponse> ExchangeAsync(ExchangeRequest request);

        /// <summary>
        /// Returns current exchange rate
        /// </summary>
        /// <param name="request">PreExchange operation details</param>
        /// /// <returns>Result of possible exchange execution </returns>
        Task<ExchangeResponse> PreExchangeAsync(PreExchangeRequest request);

        /// <summary>
        /// Registers new ethereum inbound transaction
        /// </summary>
        /// <param name="request">Transaction registration details</param>
        /// <returns></returns>
        Task RegisterEthereumInboundTransactionAsync(RegisterInboundTxModel request);

        /// <summary>
        /// Registers new ethereum outbound transaction
        /// </summary>
        /// <param name="request">Transaction registration details</param>
        /// <returns></returns>
        Task RegisterEthereumOutboundTransactionAsync(RegisterOutboundTxModel request);

        /// <summary>
        /// Marks ethereum outbound transaction as completed
        /// </summary>
        /// <param name="request">Transaction identification details</param>
        /// <returns></returns>
        Task CompleteEthereumOutboundTransactionAsync(CompleteOutboundTxModel request);

        /// <summary>
        ///  Marks ethereum outbound transaction as failed
        /// </summary>
        /// <param name="request">Transaction identification details</param>
        /// <returns></returns>
        Task FailEthereumOutboundTransactionAsync(FailOutboundTxModel request);

        /// <summary>
        ///  Marks ethereum outbound transaction as complemeted with zero amount
        /// </summary>
        /// <param name="request">Transaction identification details</param>
        /// <returns></returns>
        Task FailEthereumOutboundTransactionAsync(NotEnoughFundsOutboundTxModel request);

        /// <summary>
        /// Executes cashout
        /// </summary>
        /// <param name="request">Cashout request details</param>
        /// <returns>Cashout execution result</returns>
        Task<CashoutResponse> CashoutAsync(CashoutRequest request);
    }
}
