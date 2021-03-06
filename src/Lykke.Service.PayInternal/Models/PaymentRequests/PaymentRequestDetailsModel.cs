﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Core.Domain.PaymentRequests;

namespace Lykke.Service.PayInternal.Models.PaymentRequests
{
    [Obsolete]
    public class PaymentRequestDetailsModel
    {
        public PaymentRequestDetailsModel()
        {
            Transactions= new List<PaymentRequestTransactionModel>();
        }
        
        public string Id { get; set; }

        public string OrderId { get; set; }
        
        public string MerchantId { get; set; }
        
        public decimal Amount { get; set; }
        
        public string SettlementAssetId { get; set; }
        
        public string PaymentAssetId { get; set; }
        
        public DateTime DueDate { get; set; }
        
        public decimal MarkupPercent { get; set; }
        
        public int MarkupPips { get; set; }

        public decimal MarkupFixedFee { get; set; }
        
        public string WalletAddress { get; set; }
        
        [EnumDataType(typeof(PaymentRequestStatus), ErrorMessage = "Invalid value, possible values are: None, New, Cancelled, InProcess, Confirmed, RefundInProgress, Refunded, Error, SettlementInProgress, SettlementError, Settled")]
        public PaymentRequestStatus Status { get; set; }
        
        public decimal PaidAmount { get; set; }
        
        public DateTime? PaidDate { get; set; }

        [EnumDataType(typeof(PaymentRequestProcessingError), ErrorMessage = "Invalid value, possible values are: None, UnknownRefund, UnknownPayment, PaymentAmountAbove, PaymentAmountBelow, PaymentAmountBelow, RefundNotConfirmed, LatePaid")]
        public PaymentRequestProcessingError ProcessingError { get; set; }

        public DateTime Timestamp { get; set; }

        public PaymentRequestOrderModel Order { get; set; }
        
        public List<PaymentRequestTransactionModel> Transactions { get; set; }

        public PaymentRequestRefundModel Refund { get; set; }

        public string Initiator { get; set; }
    }
}
