using System;

namespace NatsTask.Common.Entity
{
    public class MessageEntity: Entity
    {
        public DateTime TimeStamp { get; set; }
        public string Data { get; set; }
    }
}
