using System;
using System.IO;
using System.Text;
using NATS.Client;
using NatsTask.Common;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using Newtonsoft.Json;

namespace NatsSubscriber
{
    public class Subscriber
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly Object _lock = new Object();
        private IEncodedConnection _connection = null!;

        public Subscriber()
        {
            _connectionFactory = new ConnectionFactory();
        }

        public void Run(string[] args)
        {
            CommonDefaults.ConnectionString = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}", "SubscriberDatabase");

            Console.WriteLine("The NatsSubscriber is running. For turn off, press any key...");
            RestoreState();
            InternalRun();
        }

        private void CreateConnection()
        {
            Options options = ConnectionFactory.GetDefaultOptions();
            options.ReconnectedEventHandler = ReconnectedEventHandler;
            options.ReconnectWait = 1000;
            options.MaxReconnect = Options.ReconnectForever;
            options.AllowReconnect = true;
            options.Url = "nats://demo.nats.io";

            _connection = _connectionFactory.CreateEncodedConnection(options);

            _connection.OnSerialize += o => Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(o));
            _connection.OnDeserialize +=
                bytes => JsonConvert.DeserializeObject<MessageEntity>(Encoding.UTF8.GetString(bytes));
        }

        private void ReconnectedEventHandler(object? sender, ConnEventArgs e)
        {

        }

        private void RestoreState()
        {
            using var unitOfWork = new UnitOfWork();
            var messages = unitOfWork.Repository<MessageEntity>().FindAll();
            foreach (var message in messages)
            {
                Console.WriteLine(message);
            }
        }

        private void InternalRun()
        {
            CreateConnection();
            using var subscription = _connection.SubscribeAsync(CommonDefaults.Subject, MessageEventHandler);
            subscription.Start();
            Console.ReadKey();
        }

        private void MessageEventHandler(object? sender, EncodedMessageEventArgs e)
        {
            using var unitOfWork = new UnitOfWork();

            if (e.ReceivedObject is MessageEntity message)
            {
                message.TimeStamp = DateTime.Now;
                unitOfWork.Repository<MessageEntity>().Add(message);

                Console.WriteLine(message);
            }
        }
    }
}
