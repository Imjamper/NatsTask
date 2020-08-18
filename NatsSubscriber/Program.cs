using CommandLine;
using NatsTask.Common;

namespace NatsSubscriber
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<NatsClientOptions>(args)
                .WithParsed(o => { new Subscriber().Run(o); });
        }
    }
}