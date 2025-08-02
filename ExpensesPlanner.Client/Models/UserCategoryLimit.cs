using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace ExpensesPlanner.Client.Models
{
    public class UserCategoryLimit
    {
        public List<CategoryLimit> CategoryLimits { get; set; } = new List<CategoryLimit>();

        public string UserId { get; set; } = string.Empty;
    }
}
