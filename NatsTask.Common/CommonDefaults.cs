using System.IO;
using NatsTask.Common.Database;

namespace NatsTask.Common
{
    public static class CommonDefaults
    {
        private static string _connectionString;

        public const string Subject = "NATS_TASK";
        public const string ClusterName = "test-cluster";

        public static string ConnectionString
        {
            get => _connectionString;
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
    }
}
