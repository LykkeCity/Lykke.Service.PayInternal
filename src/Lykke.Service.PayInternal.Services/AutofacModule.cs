﻿using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Core;
using JetBrains.Annotations;
using Lykke.Service.PayInternal.Core;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;
using StackExchange.Redis;

namespace Lykke.Service.PayInternal.Services
{
    public class AutofacModule: Module
    {
        private readonly ExpirationPeriodsSettings _expirationPeriods;
        private readonly int _transactionConfirmationCount;
        private readonly IList<BlockchainWalletAllocationPolicy> _walletAllocationSettings;
        private readonly IReadOnlyList<AssetPairSetting> _assetPairLocalStorageSettings;
        private readonly CacheSettings _cacheSettings;
        private readonly RetryPolicySettings _retryPolicySettings;

        public AutofacModule(
            [NotNull] ExpirationPeriodsSettings expirationPeriods,
            int transactionConfirmationCount,
            [NotNull] IList<BlockchainWalletAllocationPolicy> walletAllocationSettings, 
            [NotNull] IReadOnlyList<AssetPairSetting> assetPairLocalStorageSettings, 
            [NotNull] CacheSettings cacheSettings, 
            [NotNull] RetryPolicySettings retryPolicySettings)
        {
            _expirationPeriods = expirationPeriods ?? throw new ArgumentNullException(nameof(expirationPeriods));
            _transactionConfirmationCount = transactionConfirmationCount;
            _walletAllocationSettings = walletAllocationSettings ?? throw new ArgumentNullException(nameof(walletAllocationSettings));
            _assetPairLocalStorageSettings = assetPairLocalStorageSettings ?? throw new ArgumentNullException(nameof(assetPairLocalStorageSettings));
            _cacheSettings = cacheSettings ?? throw new ArgumentNullException(nameof(cacheSettings));
            _retryPolicySettings = retryPolicySettings ?? throw new ArgumentNullException(nameof(retryPolicySettings));
        }
        
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<RefundService>()
                .As<IRefundService>()
                .WithParameter(TypedParameter.From(_expirationPeriods.Refund));

            builder.RegisterType<OrderService>()
                .WithParameter(TypedParameter.From(_expirationPeriods.Order))
                .As<IOrderService>();

            builder.RegisterType<PaymentRequestStatusResolver>()
                .WithParameter(TypedParameter.From(_transactionConfirmationCount))
                .As<IPaymentRequestStatusResolver>();

            builder.RegisterType<BcnWalletUsageService>()
                .As<IBcnWalletUsageService>();

            builder.RegisterType<VirtualWalletService>()
                .As<IVirtualWalletService>();

            builder.RegisterType<WalletManager>()
                .WithParameter(TypedParameter.From(_walletAllocationSettings))
                .As<IWalletManager>();

            builder.RegisterType<TransactionsManager>()
                .WithParameter(TypedParameter.From(_transactionConfirmationCount))
                .As<ITransactionsManager, IEthereumTransactionsManager>();

            builder.RegisterType<BlockchainClientProvider>()
                .As<IBlockchainClientProvider>();

            builder.RegisterType<PaymentRequestDetailsBuilder>()
                .As<IPaymentRequestDetailsBuilder>();

            builder.RegisterType<FileService>()
                .As<IFileService>();

            builder.RegisterType<MerchantWalletService>()
                .As<IMerchantWalletService>();

            builder.RegisterType<AssetRatesService>()
                .As<IAssetRatesService>();

            builder.RegisterType<AssetPairSettingsService>()
                .WithParameter(TypedParameter.From(_assetPairLocalStorageSettings))
                .As<IAssetPairSettingsService>();

            builder.Register(c => ConnectionMultiplexer.Connect(_cacheSettings.RedisConfiguration))
                .As<IConnectionMultiplexer>()
                .SingleInstance();

            builder.RegisterType<RedisLocksService>()
                .WithParameter(TypedParameter.From(_cacheSettings.PaymentLocksCacheKeyPattern))
                .Keyed<IDistributedLocksService>(DistributedLockPurpose.InternalPayment)
                .SingleInstance();

            builder.RegisterType<RedisLocksService>()
                .WithParameter(TypedParameter.From(_cacheSettings.CheckoutLocksCacheKeyPattern))
                .Keyed<IDistributedLocksService>(DistributedLockPurpose.OrderCheckout)
                .SingleInstance();

            builder.RegisterType<PaymentRequestService>()
                .As<IPaymentRequestService>()
                .WithParameter(TypedParameter.From(_expirationPeriods))
                .WithParameter(TypedParameter.From(_retryPolicySettings))
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IDistributedLocksService) &&
                                 pi.Name == "paymentLocksService",
                    (pi, ctx) => ctx.ResolveKeyed<IDistributedLocksService>(DistributedLockPurpose.InternalPayment)))
                .WithParameter(new ResolvedParameter(
                    (pi, ctx) => pi.ParameterType == typeof(IDistributedLocksService) &&
                                 pi.Name == "checkoutLocksService",
                    (pi, ctx) => ctx.ResolveKeyed<IDistributedLocksService>(DistributedLockPurpose.OrderCheckout)));

            builder.RegisterType<ExchangeService>()
                .As<IExchangeService>()
                .WithParameter(TypedParameter.From(_retryPolicySettings));

            builder.RegisterType<CashoutService>()
                .As<ICashoutService>()
                .WithParameter(TypedParameter.From(_retryPolicySettings));
        }
    }
}
