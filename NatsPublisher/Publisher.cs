﻿using System;
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
    public class Publisher: NatsClient
    {
        private readonly MessageGenerator _generator;

        public override string ClientId => "NatsPublisher";

        public Publisher()
        {
            _generator = new MessageGenerator();
        }

        protected override void InternalRun()
        {
            while (IsRunning)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("Connecting...");
                    CreateConnection();
                }
                catch (NATSException)
                {
                    Reconnect();
                    continue;
                }
                
                Console.Clear();
                Console.WriteLine("Connected");

                RestoreState();

                CancellationTokenSource = new CancellationTokenSource();

                PeriodicTask.StartPeriodicTask(SendMessage, 1000, 0, CancellationTokenSource.Token);
                AutoResetEvent.WaitOne();
                CancellationTokenSource.Cancel();
                Connection?.Close();
            }
        }

        private void SendMessage()
        {
            if (Connection?.NATSConnection?.State != ConnState.CONNECTED) 
                return;

            using var unitOfWork = new UnitOfWork();
            var message = _generator.Generate();
            unitOfWork.Repository<MessageEntity>().Add(message);

            var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

            Connection.Publish(CommonDefaults.Subject, bytes);

            Console.WriteLine(message);
        }
    }
}
