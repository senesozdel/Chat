using MongoDB.Bson.Serialization.Attributes;

namespace Chat.Entities
{
    public class BaseEntity
    {
        [BsonId]
        [BsonElement("_id")]    
        public int Id { get; set; } 
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  
        public DateTime? LastUpdatedAt { get; set; }                  
        public bool IsDeleted { get; set; } = false;
    }
}
