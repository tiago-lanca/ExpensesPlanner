

using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ExpensesPlanner.Client.DTO
{
    public class UserDetails
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required.")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last name is required.")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address format.")]
        public string? Email { get; set; }

        [RegularExpression(@"^(\+?\d{1,4})?[\s\-]?(\d{3})[\s\-]?(\d{3})[\s\-]?(\d{3})$", ErrorMessage = "Número inválido. Ex: +351 912345678")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        public DateTime? DateOfBirth { get; set; }

        public byte[]? ProfilePictureUrl { get; set; }
        public RoleType Role { get; set; }

        public List<Expense> ListExpenses { get; set; } = new List<Expense>();
    }
}
