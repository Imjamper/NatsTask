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
using STAN.Client;

namespace NatsPublisher
{
    public class Publisher
    {
        private readonly MessageGenerator _generator;
        private readonly StanConnectionFactory _connectionFactory;
        private IStanConnection _connection = null!;
        private Timer _timer = null!;

        public Publisher()
        {
            _generator = new MessageGenerator();
            _connectionFactory = new StanConnectionFactory();
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
            var options = StanOptions.GetDefaultOptions();
            options.NatsURL = "nats://192.168.99.100:4222";

            _connection = _connectionFactory.CreateConnection(CommonDefaults.ClusterName, "NatsPublisher", options);
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
            if (_connection.NATSConnection.State != ConnState.CONNECTED) 
                return;

            using var unitOfWork = new UnitOfWork();
            var message = _generator.Generate();
            unitOfWork.Repository<MessageEntity>().Add(message);

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            _connection.Publish(CommonDefaults.Subject, bytes);

            Console.WriteLine(message);
        }
    }
}
