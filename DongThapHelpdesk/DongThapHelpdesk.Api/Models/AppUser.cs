using DongThapHelpdesk.Api.Enums;
using DongThapHelpdesk.Api.Attributes;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace DongThapHelpdesk.Api.Models
{
    [BsonCollection("users")]
    public class AppUser
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string PhoneNumber { get; set; }
        public UserRole Role { get; set; }

        [BsonRepresentation(BsonType.ObjectId)]
        public string? DepartmentId { get; set; } // null for Citizen/Manager

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
