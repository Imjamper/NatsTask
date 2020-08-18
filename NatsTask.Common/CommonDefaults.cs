using System.IO;
using NatsTask.Common.Database;

namespace NatsTask.Common
{
    public static class CommonDefaults
    {
        private static string _connectionString;
        private static string _loggerFolder;

        public const string Subject = "NATS_TASK";
        public const string ClusterName = "test-cluster";

        public static bool InMemoryDatabase { get; set; }

        public static string ConnectionString
        {
            get
            {
                return _connectionString;
            }
            set
            {
                if (!Directory.Exists(value))
                {
                    Directory.CreateDirectory(value);

                }
                _connectionString = value;
                LiteDb.EnsureDbCreate();
            }
        }

        public static string LoggerFolder
        {
            get
            {
                return _loggerFolder;
            }
            set
            {
                if (!Directory.Exists(value))
                    Directory.CreateDirectory(value);
                _loggerFolder = value;
            }
        }
    }
}
