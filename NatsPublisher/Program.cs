using CommandLine;
using NatsTask.Common;

namespace NatsPublisher
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Parser.Default.ParseArguments<NatsClientOptions>(args)
                .WithParsed(o => { new Publisher().Run(o); });
        }
    }
}