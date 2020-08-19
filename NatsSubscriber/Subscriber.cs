using System;
using System.Text;
using NATS.Client;
using NatsTask.Common;
using NatsTask.Common.Database;
using NatsTask.Common.Entity;
using Newtonsoft.Json;
using STAN.Client;

namespace NatsSubscriber
{
    public class Subscriber : NatsClient
    {
        public override string ClientId => "NatsSubscriber";

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

                    var lastItemId = RestoreState();

                    var subscriptionOptions = StanSubscriptionOptions.GetDefaultOptions();
                    if (lastItemId.HasValue)
                        subscriptionOptions.StartAt((ulong) lastItemId.Value + 1);
                    else
                        subscriptionOptions.DeliverAllAvailable();

                    var subscription = Connection?.Subscribe(Options.Subject, subscriptionOptions, MessageReceived);
                    AutoResetEvent.WaitOne();
                    Connection?.Close();
                    subscription?.Dispose();
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

        private void MessageReceived(object sender, StanMsgHandlerArgs e)
        {
            try
            {
                HandleMessage(e);
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

        private void HandleMessage(StanMsgHandlerArgs e)
        {
            using var unitOfWork = new UnitOfWork();
            var message = JsonConvert.DeserializeObject<MessageEntity>(Encoding.UTF8.GetString(e.Message.Data));
            message.TimeStamp = DateTime.Now;
            if (unitOfWork.Collection<MessageEntity>(Options.Subject)
                .Exists(entity => entity.Id == message.Id))
                return;

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

            Console.WriteLine($"{Options.Subject} | {message}");
        }
    }
}