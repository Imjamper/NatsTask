#nullable enable
using System;
using System.IO;
using System.Threading;
using NATS.Client;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using STAN.Client;

namespace NatsTask.Common
{
    public abstract class NatsClient
    {
        protected IStanConnection? Connection;
        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected bool IsRunning;
        protected readonly AutoResetEvent AutoResetEvent = new AutoResetEvent(false);

        private readonly ConnectionFactory _connectionFactory;
        private readonly StanConnectionFactory _stanConnectionFactory;

        public abstract string ClientId { get; }

        protected NatsClient()
        {
            _stanConnectionFactory = new StanConnectionFactory();
            _connectionFactory = new ConnectionFactory();
        }

        public void Run()
        {
            IsRunning = true;
            Console.CancelKeyPress += CancelKeyPress;
            if (Connection != null && Connection.NATSConnection.State == ConnState.CONNECTED)
                return;

            CommonDefaults.ConnectionString = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}", ClientId);
            Console.WriteLine($"The {ClientId} is running.");

            InternalRun();
        }


        protected abstract void InternalRun();

        protected void Stop()
        {
            AutoResetEvent.Set();
            IsRunning = false;
            Environment.Exit(-1);
        }

        protected long? RestoreState()
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

        protected void CreateConnection()
        {
            var natsOptions = GetNatsOpts();
            var natsConnection = _connectionFactory.CreateConnection(natsOptions);

            var stanOptions = StanOptions.GetDefaultOptions();
            stanOptions.ConnectTimeout = 4000;
            stanOptions.NatsConn = natsConnection;
            stanOptions.PubAckWait = 40000;
            stanOptions.ConnectionLostEventHandler += (sender, args) =>
            {
                AutoResetEvent.Set();
            };

            var connection = _stanConnectionFactory.CreateConnection(CommonDefaults.ClusterName, ClientId, stanOptions);

            Connection = connection;
        }

        protected void Reconnect()
        {
            Console.Clear();
            Console.WriteLine("Reconnecting after 1 sec");
            Thread.Sleep(1000);
        }

        private Options GetNatsOpts()
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = "nats://192.168.99.100:4222";
            opts.AllowReconnect = true;
            opts.PingInterval = 5000;
            opts.MaxPingsOut = 2;
            //opts.ReconnectBufferSize = Options.ReconnectBufferDisabled;
            opts.MaxReconnect = Options.ReconnectForever;
            opts.ReconnectWait = 1000;
            opts.Timeout = 4000;

            opts.ReconnectedEventHandler += (sender, args) =>
            {
                AutoResetEvent.Set();
            };
            opts.ClosedEventHandler += (sender, args) =>
            {
                AutoResetEvent.Set();
            };
            opts.DisconnectedEventHandler += (sender, args) =>
            {
                AutoResetEvent.Set();
            };
                
            opts.AsyncErrorEventHandler += (sender, args) =>
            {
                AutoResetEvent.Set();
            };

            return opts;
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Stop();
        }
    }
}
