using System;
using System.Threading;
using NATS.Client;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using NatsTask.Common.Extensions;
using NatsTask.Common.Helpers;

namespace NatsPublisher
{
    public class Publisher
    {
        private readonly MessageGenerator _generator;
        private readonly ConnectionFactory _connectionFactory;
        private IConnection _connection = null!;
        private Timer _timer = null!;

        public Publisher()
        {
            _generator = new MessageGenerator();
            _connectionFactory = new ConnectionFactory();
        }

        public void Run(string[] args)
        {
            Console.WriteLine("The NatsPublisher is running. For turn off, press any key...");
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
            _timer = new Timer(TimerFired, null, 0, 1000);
        }

        private void TimerFired(object? state)
        {
            if (_connection.State != ConnState.CONNECTED) 
                return;

            using var unitOfWork = new UnitOfWork();
            var message = _generator.Generate();
            unitOfWork.Repository<MessageEntity>().Add(message);

            _connection.Publish(message.ToMsg());
            _connection.Flush();
            Console.WriteLine(message);
        }
    }
}
