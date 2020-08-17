using System.Text;
using NATS.Client;
using NatsTask.Common.Entity;
using Newtonsoft.Json;

namespace NatsTask.Common.Extensions
{
    public static class MessageEntityExtensions
    {
        public static Msg ToMsg(this MessageEntity entity)
        {
            var result = new Msg(CommonDefaults.Subject);
            var json = JsonConvert.SerializeObject(entity);
            var bytes = Encoding.UTF8.GetBytes(json);
            result.Data = bytes;

            return result;
        }
    }
}
