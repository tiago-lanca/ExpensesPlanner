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
        public new string Id { get; set; } = Guid.NewGuid().ToString();

        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        [Required]
        public string FirstName { get; set; } = string.Empty;

        [BsonElement("name"), BsonRepresentation(BsonType.String)]
        [Required]
        public string LastName { get; set; } = string.Empty;

        [BsonElement("email"), BsonRepresentation(BsonType.String)]
        [Required]
        public override string? Email { get; set; }


        [BsonElement("phoneNumber"), BsonRepresentation(BsonType.String)]
        public override string? PhoneNumber { get; set; }

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
        public int? Salary_Preset { get; set; }
        public bool Salary_Preset_Enabled { get; set; } = false;
        public List<MonthlyExpenseChart> MonthlyExpenseChart { get; set; } = new List<MonthlyExpenseChart>();
        public string ApiKeyHash { get; set; } = string.Empty;




        public bool IsListExpensesEmpty()
        {
            return string.IsNullOrEmpty(ListExpensesId) || ListExpensesId == Guid.Empty.ToString();
        }
    }
}
