using System;
using System.Security.Cryptography;
using NatsTask.Common.Entity;

namespace NatsTask.Common.Helpers
{
    public class MessageGenerator
    {
        private readonly IncrementalHash _incrementalHash = IncrementalHash.CreateHash(HashAlgorithmName.MD5);

        public MessageEntity Generate()
        {
            var result = new MessageEntity();
            result.Data = Guid.NewGuid().ToString("D");
            result.TimeStamp = DateTime.Now;


            return result;
        }
    }
}