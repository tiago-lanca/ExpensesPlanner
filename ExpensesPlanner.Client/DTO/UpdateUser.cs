using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.Client.DTO
{
    public class UpdateUser
    {
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
    }
}
