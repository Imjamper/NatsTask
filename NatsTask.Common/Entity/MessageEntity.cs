using System;

namespace NatsTask.Common.Entity
{
    public class MessageEntity : Entity
    {
        public DateTime TimeStamp { get; set; }
        public string Data { get; set; }

        public string CheckSum { get; set; }

        public override string ToString()
        {
            return $"Message: {Id} | {TimeStamp:G} | {Data} | {CheckSum}";
        }
    }
}