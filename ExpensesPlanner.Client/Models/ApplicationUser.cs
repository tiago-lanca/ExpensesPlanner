using AspNetCore.Identity.MongoDbCore.Models;
using ExpensesPlanner.Client.Enums;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDbGenericRepository.Attributes;
using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.Client.Models
{
    [CollectionName("Users")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {
        [BsonId]
        [BsonElement("_id"), BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        [Required]
        public string FirstName { get; set; }

        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        [Required]
        public string LastName { get; set; }

        [BsonElement("email"), BsonRepresentation(BsonType.String)]
        [Required]
        public string? Email { get; set; }


        [BsonElement("phoneNumber"), BsonRepresentation(BsonType.String)]
        public string? PhoneNumber { get; set; }

        [BsonElement("address"), BsonRepresentation(BsonType.String)]
        public string? Address { get; set; }

        [BsonElement("dateBirth"), BsonRepresentation(BsonType.DateTime)]
        public DateTime? DateOfBirth { get; set; }
        
        [BsonElement("profilePictureUrl")]
        public byte[]? ProfilePictureUrl { get; set; }

        [BsonElement("role"), BsonRepresentation(BsonType.String)]
        public RoleType Role { get; set; } = RoleType.User;

        [BsonElement("listExpensesId"), BsonRepresentation(BsonType.String)]
        public string ListExpensesId { get; set; } = string.Empty;
        public int? Salary { get; set; }
        public bool Salary_Preset { get; set; } = false;
    }
}
