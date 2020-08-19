#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using NATS.Client;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using NatsTask.Common.Extensions;
using STAN.Client;

namespace NatsTask.Common
{
    public abstract class NatsClient
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly StanConnectionFactory _stanConnectionFactory;
        protected readonly AutoResetEvent AutoResetEvent = new AutoResetEvent(false);
        protected CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();
        protected IStanConnection? Connection;
        protected IncrementalHash IncrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);
        protected bool IsRunning;
        protected byte[]? LastHash;
        protected NatsClientOptions Options = new NatsClientOptions();

        protected NatsClient()
        {
            _stanConnectionFactory = new StanConnectionFactory();
            _connectionFactory = new ConnectionFactory();
        }

        public abstract string ClientId { get; }

        public void Run(NatsClientOptions options)
        {
            IsRunning = true;
            Options = options;

            NatsClientOptions.DatabasePath = Path.Combine($"{AppDomain.CurrentDomain.BaseDirectory}", ClientId);

            Console.CancelKeyPress += CancelKeyPress;
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
            var messages = unitOfWork.Collection<MessageEntity>(Options.Subject).FindAll().ToList();
            foreach (var message in messages) Console.WriteLine($"{Options.Subject} | {message}");

            var last = messages.LastOrDefault();
            if (last == null)
                return lastItemId;
            lastItemId = last.Id;
            LastHash = last.CheckSum.ToByteArray();

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
            stanOptions.ConnectionLostEventHandler += (sender, args) => { AutoResetEvent.Set(); };

            var connection = _stanConnectionFactory.CreateConnection(Options.ClusterName, ClientId, stanOptions);

            Connection = connection;
        }

        protected string GetMd5Hash()
        {
            var data = IncrementalHash.GetHashAndReset();
            LastHash = data;
            var sBuilder = new StringBuilder();

            foreach (var t in data) sBuilder.Append(t.ToString("x2"));

            return sBuilder.ToString();
        }

        protected void Reconnect()
        {
            Console.Clear();
            Console.WriteLine("Reconnecting after 3 sec");
            Thread.Sleep(3000);
        }

        private Options GetNatsOpts()
        {
            var opts = ConnectionFactory.GetDefaultOptions();
            opts.Url = Options.Url;
            opts.AllowReconnect = true;
            opts.PingInterval = 500;
            opts.MaxPingsOut = 2;
            opts.ReconnectBufferSize = NATS.Client.Options.ReconnectBufferDisabled;
            opts.MaxReconnect = NATS.Client.Options.ReconnectForever;
            opts.ReconnectWait = 1000;
            opts.Timeout = 4000;

            opts.ReconnectedEventHandler += (sender, args) => { AutoResetEvent.Set(); };
            opts.ClosedEventHandler += (sender, args) => { AutoResetEvent.Set(); };
            opts.DisconnectedEventHandler += (sender, args) => { AutoResetEvent.Set(); };

            opts.AsyncErrorEventHandler += (sender, args) => { AutoResetEvent.Set(); };

            return opts;
        }

        private void CancelKeyPress(object sender, ConsoleCancelEventArgs e)
        {
            Stop();
        }
    }
}