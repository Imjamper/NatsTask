using System;
using System.Text;
using System.Threading;
using NATS.Client;
using NatsTask.Common;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using NatsTask.Common.Helpers;
using Newtonsoft.Json;

namespace NatsSubscriber
{
    public class Subscriber
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly Object _lock = new Object();
        private IConnection _connection = null!;

        public Subscriber()
        {
            _connectionFactory = new ConnectionFactory();
        }

        public void Run(string[] args)
        {
            Console.WriteLine("The NatsSubscriber is running. For turn off, press any key...");
            RestoreState();
            InternalRun();
        }

        private IConnection CreateConnection()
        {
            Options options = ConnectionFactory.GetDefaultOptions();
            options.ReconnectedEventHandler = ReconnectedEventHandler;
            options.ReconnectWait = 1000;
            options.MaxReconnect = Options.ReconnectForever;
            options.AllowReconnect = true;
            options.Url = "nats://demo.nats.io";

            return _connectionFactory.CreateConnection(options);
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
            _connection = CreateConnection();
            using (IAsyncSubscription subscription = _connection.SubscribeAsync(CommonDefaults.Subject, MessageEventHandler))
            {
                lock (_lock)
                {
                    Monitor.Wait(_lock);
                }
            }
        }

        private void MessageEventHandler(object? sender, MsgHandlerEventArgs args)
        {
            using var unitOfWork = new UnitOfWork();

            var jsonString = Encoding.UTF8.GetString(args.Message.Data);
            var message = JsonConvert.DeserializeObject<MessageEntity>(jsonString);
            message.TimeStamp = DateTime.Now;

            unitOfWork.Repository<MessageEntity>().Add(message);
            
            Console.WriteLine(message);
        }
    }
}
