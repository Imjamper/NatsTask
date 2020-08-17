using System;

namespace NatsTask.Common.DTO
{
    public class MessageDto
    {
        public MessageDto(string name, DateTime timeStamp, string data)
        {
            Name = name;
            TimeStamp = timeStamp;
            Data = data;
        }

        public string Name { get; set; }

        public DateTime TimeStamp { get; set; }
        public string Data { get; set; }
    }
}
