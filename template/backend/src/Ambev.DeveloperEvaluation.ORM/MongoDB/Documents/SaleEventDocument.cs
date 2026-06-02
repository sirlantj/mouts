using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Ambev.DeveloperEvaluation.ORM.MongoDB.Documents;

public class SaleEventDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    public Guid SaleId { get; set; }
    public string SaleNumber { get; set; } = string.Empty;
    public string EventType { get; set; } = string.Empty;
    public string EventData { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; }
}
