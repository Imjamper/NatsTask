using System;
using System.IO;
using System.Text;
using System.Threading;
using NatsTask.Common;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using Newtonsoft.Json;
using STAN.Client;

namespace NatsSubscriber
{
    public class Subscriber
    {
        private readonly StanConnectionFactory _connectionFactory;
        private IStanConnection _connection = null!;

        public Subscriber()
        {
            _connectionFactory = new StanConnectionFactory();
        }

        public void Run(string[] args)
        {
            CommonDefaults.ConnectionString = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}", "SubscriberDatabase");

            Console.WriteLine("The NatsSubscriber is running. For turn off, press any key...");
            var lastItemId = RestoreState();

            InternalRun(lastItemId);
        }

        private void CreateConnection()
        {
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = "nats://192.168.99.100:4222";

            _connection = _connectionFactory.CreateConnection(CommonDefaults.ClusterName, "NatsSubscriber", options);
        }

        private long? RestoreState()
        {
            long? lastItemId = null;
            using var unitOfWork = new UnitOfWork();
            var messages = unitOfWork.Repository<MessageEntity>().FindAll();
            foreach (var message in messages)
            {
                lastItemId = message.Id;
                Console.WriteLine(message);
            }

            return lastItemId;
        }

        private void InternalRun(long? lastItemId)
        {
            CreateConnection();

            AutoResetEvent ev = new AutoResetEvent(false);
            StanSubscriptionOptions subscriptionOptions = StanSubscriptionOptions.GetDefaultOptions();
            if (lastItemId.HasValue)
            {
                subscriptionOptions.StartAt((ulong) lastItemId.Value + 1);
            }
            else
            {
                subscriptionOptions.DeliverAllAvailable();
            }

            using var subscription =
                _connection.Subscribe(CommonDefaults.Subject, subscriptionOptions, MessageEventHandler);
            {
                ev.WaitOne();
            }
        }

        private void MessageEventHandler(object sender, StanMsgHandlerArgs e)
        {
            using var unitOfWork = new UnitOfWork();

            var message = JsonConvert.DeserializeObject<MessageEntity>(Encoding.UTF8.GetString(e.Message.Data));
            message.TimeStamp = DateTime.Now;
            unitOfWork.Repository<MessageEntity>().Add(message);

            Console.WriteLine(message);
        }
    }
}
