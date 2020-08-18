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
    public class Subscriber: NatsClient
    {
        public override string ClientId => "NatsSubscriber";

        protected override void InternalRun()
        {
            while (IsRunning)
            {
                try
                {
                    Console.Clear();
                    Console.WriteLine("Connecting...");
                    CreateConnection();

                    Console.Clear();
                    Console.WriteLine("Connected");

                    var lastItemId = RestoreState();

                    StanSubscriptionOptions subscriptionOptions = StanSubscriptionOptions.GetDefaultOptions();
                    if (lastItemId.HasValue)
                    {
                        subscriptionOptions.StartAt((ulong) lastItemId.Value + 1);
                    }
                    else
                    {
                        subscriptionOptions.DeliverAllAvailable();
                    }

                    var subscription = Connection?.Subscribe(CommonDefaults.Subject, subscriptionOptions, MessageReceived);
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
        }

        private void MessageReceived(object sender, StanMsgHandlerArgs e)
        {
            try
            {
                using var unitOfWork = new UnitOfWork();

                var message = JsonConvert.DeserializeObject<MessageEntity>(Encoding.UTF8.GetString(e.Message.Data));
                message.TimeStamp = DateTime.Now;
                unitOfWork.Repository<MessageEntity>().Add(message);

                Console.WriteLine(message);
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
    }
}
