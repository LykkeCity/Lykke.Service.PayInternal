﻿using System;
using System.ComponentModel.DataAnnotations;
using Lykke.Service.PayInternal.Validation;
using LykkePay.Common.Validation;

namespace Lykke.Service.PayInternal.Models.Exchange
{
    /// <summary>
    /// Exchange operation details
    /// </summary>
    public class PreExchangeModel
    {
        /// <summary>
        /// Gets ot sets merchant id
        /// </summary>
        [Required]
        [RowKey]
        public string MerchantId { get; set; }

        /// <summary>
        /// Gets or sets source asset id
        /// </summary>
        [Required]
        [AssetExists]
        public string SourceAssetId { get; set; }

        /// <summary>
        /// Gets or sets source amount
        /// </summary>
        [Required]
        [Range(Double.Epsilon, Double.MaxValue, ErrorMessage = "Source amount must be greater than 0")]
        public decimal SourceAmount { get; set; }

        /// <summary>
        /// Gets or sets destination asset id
        /// </summary>
        [Required]
        [AssetExists]
        public string DestAssetId { get; set; }
    }
}
