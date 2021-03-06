﻿using System;
using System.Threading.Tasks;
using Autofac;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Log;
using Lykke.RabbitMqBroker.Publisher;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.PayInternal.Contract;
using Lykke.Service.PayInternal.Core.Domain.Transaction;
using Lykke.Service.PayInternal.Core.Services;
using Lykke.Service.PayInternal.Core.Settings.ServiceSettings;

namespace Lykke.Service.PayInternal.Rabbit.Publishers
{
    /// <summary>
    /// Publishes messages about new transactions which have been created by service
    /// </summary>
    [UsedImplicitly]
    public class TransactionPublisher : ITransactionPublisher, IStartable, IStopable
    {
        private readonly ILog _log;
        private readonly ILogFactory _logFactory;
        private readonly RabbitMqSettings _settings;
        private RabbitMqPublisher<NewTransactionMessage> _publisher;

        public TransactionPublisher(
            [NotNull] ILogFactory logFactory, 
            [NotNull] RabbitMqSettings settings)
        {
            _logFactory = logFactory;
            _log = logFactory.CreateLog(this);
            _settings = settings;
        }

        public void Dispose()
        {
            _publisher?.Dispose();
        }

        public async Task PublishAsync(IPaymentRequestTransaction transaction)
        {
            var message = new NewTransactionMessage
            {
                Id = transaction.TransactionId,
                AssetId = transaction.AssetId,
                Amount = transaction.Amount,
                Confirmations = transaction.Confirmations,
                BlockId = transaction.BlockId,
                Blockchain = Enum.Parse<BlockchainType>(transaction.Blockchain.ToString()),
                //todo
                DueDate = transaction.DueDate ?? DateTime.UtcNow.AddDays(1)
            };

            _log.Info("Publishing new transaction message", message);

            await _publisher.ProduceAsync(message);
        }

        public void Start()
        {
            var settings = RabbitMqSubscriptionSettings
                .CreateForPublisher(_settings.ConnectionString, _settings.TransactionUpdatesExchangeName);

            settings.MakeDurable();

            _publisher = new RabbitMqPublisher<NewTransactionMessage>(_logFactory, settings)
                .SetSerializer(new JsonMessageSerializer<NewTransactionMessage>())
                .SetPublishStrategy(new DefaultFanoutPublishStrategy(settings))
                .PublishSynchronously()
                .Start();
        }

        public void Stop()
        {
            _publisher?.Stop();
        }
    }
}
