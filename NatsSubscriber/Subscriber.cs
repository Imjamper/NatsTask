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
        private IStanSubscription _subscription;

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
                }
                catch (NATSException)
                {
                    Reconnect();
                    continue;
                }

                Console.Clear();
                Console.WriteLine("Connected");

                var lastItemId = RestoreState();

                StanSubscriptionOptions subscriptionOptions = StanSubscriptionOptions.GetDefaultOptions();
                if (lastItemId.HasValue)
                {
                    subscriptionOptions.StartAt((ulong)lastItemId.Value + 1);
                }
                else
                {
                    subscriptionOptions.DeliverAllAvailable();
                }

                _subscription = Connection?.Subscribe(CommonDefaults.Subject, subscriptionOptions, MessageEventHandler);
                AutoResetEvent.WaitOne();
                Connection?.Close();
                _subscription?.Unsubscribe();
            }
        }

        private void MessageEventHandler(object sender, StanMsgHandlerArgs e)
        {
            using var unitOfWork = new UnitOfWork();

            var message = JsonConvert.DeserializeObject<MessageEntity>(Encoding.UTF8.GetString(e.Message.Data));
            message.TimeStamp = DateTime.Now;
            unitOfWork.Repository<MessageEntity>().Add(message);

            Console.WriteLine(message);
        }
    }
}
