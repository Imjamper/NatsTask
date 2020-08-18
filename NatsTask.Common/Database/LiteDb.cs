using System.IO;
using LiteDB;
using LiteDB.Engine;

namespace NatsTask.Common.Database
{
    public class LiteDb : LiteDatabase
    {
        private static LiteDb _readWrite;
        private static readonly string ConnectionString = Path.Combine($"{CommonDefaults.ConnectionString}", "LiteDb.db");

        public LiteDb(string connectionString, BsonMapper mapper = null) : base(connectionString, mapper)
        {
        }

        public LiteDb(ConnectionString connectionString, BsonMapper mapper = null) : base(connectionString, mapper)
        {
        }

        public LiteDb(Stream stream, BsonMapper mapper = null, Stream logStream = null) : base(stream, mapper, logStream)
        {
        }

        public LiteDb(ILiteEngine engine, BsonMapper mapper = null, bool disposeOnClose = true) : base(engine, mapper, disposeOnClose)
        {
        }

        public LiteDb() : base(ConnectionString)
        {

        }

        public static void EnsureDbCreate()
        {
            if (!File.Exists(ConnectionString)) using (new LiteEngine(ConnectionString)) { }
        }

        public static void RefreshDb()
        {
            if (CommonDefaults.InMemoryDatabase)
            {
                lock (_readWrite)
                {
                    _readWrite.Dispose();
                    _readWrite = new LiteDb(new MemoryStream());
                }
            }
        }

        public static LiteDb ReadWrite => _readWrite ?? (_readWrite = CommonDefaults.InMemoryDatabase ? new LiteDb(new MemoryStream()) : new LiteDb(ConnectionString));
    }
}
