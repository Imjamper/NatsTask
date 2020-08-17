using System;
using System.Text;
using NATS.Client;
using NatsTask.Common;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using NatsTask.Common.Extensions;
using NatsTask.Common.Helpers;

namespace NatsPublisher
{
    public class Publisher
    {
        private MessageGenerator _generator;

        public Publisher()
        {
            _generator = new MessageGenerator();
        }

        public void Run(string[] args)
        {
            RestoreState();
            InternalRun();
        }

        private IConnection CreateConnection()
        {
            Options options = ConnectionFactory.GetDefaultOptions();
            options.AllowReconnect = true;
            options.Url = "nats://demo.nats.io";

            return new ConnectionFactory().CreateConnection(options);
        }

        private void RestoreState()
        {
            using var unitOfWork = new UnitOfWork();
            var messages = unitOfWork.Repository<MessageEntity>().FindAll();
            foreach (var message in messages)
            {
                Console.WriteLine($"Message: {message.Id} | {message.TimeStamp.ToLongDateString()} | {message.Data}");
            }
        }

        private void InternalRun()
        {
            using var connection = CreateConnection();
            using var unitOfWork = new UnitOfWork();

            var message = _generator.Generate();
            unitOfWork.Repository<MessageEntity>().Add(message);

            connection.Publish(message.ToMsg());
            connection.Flush();
        }
    }
}
