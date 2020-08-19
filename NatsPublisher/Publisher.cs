using System;
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
    public class Publisher : NatsClient
    {
        private readonly MessageGenerator _generator;

        public Publisher()
        {
            _generator = new MessageGenerator();
        }

        public override string ClientId => "NatsPublisher";

        protected override void InternalRun()
        {
            while (IsRunning)
                try
                {
                    Console.Clear();
                    Console.WriteLine("Connecting...");
                    CreateConnection();

                    Console.Clear();
                    Console.WriteLine($"Connected to {Options.Url}");

                    RestoreState();

                    CancellationTokenSource = new CancellationTokenSource();

                    PeriodicTask.StartPeriodicTask(SendMessage, 1000, 0, CancellationTokenSource.Token);
                    AutoResetEvent.WaitOne();
                    CancellationTokenSource.Cancel();
                    Connection?.Close();
                }
                catch (NATSException)
                {
                    Reconnect();
                }
                catch (StanException)
                {
                    Reconnect();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UnhandledException: {ex.Message}");
                    Console.WriteLine(ex);
                }
        }

        private void SendMessage()
        {
            if (Connection == null)
                return;

            try
            {
                using var unitOfWork = new UnitOfWork();
                var message = _generator.Generate();

                if (LastHash == null)
                {
                    IncrementalHash.AppendData(Encoding.UTF8.GetBytes(message.Data));
                }
                else
                {
                    IncrementalHash.AppendData(LastHash);
                    IncrementalHash.AppendData(Encoding.UTF8.GetBytes(message.Data));
                }

                message.CheckSum = GetMd5Hash();

                unitOfWork.Collection<MessageEntity>(Options.Subject).Insert(message);

                var bytes = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message));

                Connection?.Publish(Options.Subject, bytes);

                Console.WriteLine($"{Options.Subject} | {message}");
            }
            catch (NATSException)
            {
                AutoResetEvent.Set();
            }
            catch (StanException)
            {
                AutoResetEvent.Set();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UnhandledException: {ex.Message}");
                Console.WriteLine(ex);
            }
        }
    }
}