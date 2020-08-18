using System;
using System.IO;
using System.Text;
using System.Threading;
using NATS.Client;
using NatsTask.Common;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using NatsTask.Common.Helpers;
using Newtonsoft.Json;

namespace NatsPublisher
{
    public class Publisher
    {
        private readonly MessageGenerator _generator;
        private readonly ConnectionFactory _connectionFactory;
        private IEncodedConnection _connection = null!;
        private Timer _timer = null!;

        public Publisher()
        {
            _generator = new MessageGenerator();
            _connectionFactory = new ConnectionFactory();
        }

        public void Run(string[] args)
        {
            CommonDefaults.ConnectionString = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}", "PublisherDatabase");
            Console.WriteLine("The NatsPublisher is running. For turn off, press any key...");
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
            _timer = new Timer(TimerFired, null, 0, 1000);
            Console.ReadKey();
        }

        private void TimerFired(object? state)
        {
            
            if (_connection.State != ConnState.CONNECTED) 
                return;

            using var unitOfWork = new UnitOfWork();
            var message = _generator.Generate();
            unitOfWork.Repository<MessageEntity>().Add(message);

            _connection.Publish(CommonDefaults.Subject, message);
            _connection.Flush();
            Console.WriteLine(message);
        }
    }
}
