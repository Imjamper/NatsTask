using System.IO;
using CommandLine;
using NatsTask.Common.Database;

namespace NatsTask.Common
{
    public class NatsClientOptions
    {
        private static string _databasePath;

        public NatsClientOptions()
        {
            Subject = "NATS_TASK";
            ClusterName = "test-cluster";
            Url = "nats://192.168.99.100:4222";
        }

        [Option('s', "subject", Required = false, HelpText = "NATS streaming server subject. Default - 'NATS_TASK'")]
        public string Subject { get; set; }

        [Option('c', "clusterId", Required = false,
            HelpText = "NATS streaming server cluster name. Default = 'test-cluster'")]
        public string ClusterName { get; set; }

        [Option('u', "url", Required = false,
            HelpText = "NATS streaming server url. Default = 'nats://localhost:4222'")]
        public string Url { get; set; }

        public static string DatabasePath
        {
            get => _databasePath;
            set
            {
                if (!Directory.Exists(value)) Directory.CreateDirectory(value);

                _databasePath = value;
                LiteDb.EnsureDbCreate();
            }
        }
    }
}