using LiteDB;

namespace NatsTask.Common.Entity
{
    public abstract class Entity : IEntity
    {
        [BsonId]
        public int Id { get; set; }
    }
}
