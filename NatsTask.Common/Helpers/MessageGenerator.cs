using System;
using NatsTask.Common.Entity;

namespace NatsTask.Common.Helpers
{
    public class MessageGenerator
    {
        public MessageEntity Generate()
        {
            var result = new MessageEntity();
            result.Data = Guid.NewGuid().ToString("D");
            result.TimeStamp = DateTime.Now;

            return result;
        }
    }
}
