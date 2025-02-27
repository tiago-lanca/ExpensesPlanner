using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.ViewModels
{
    public class UserDetailsViewModel
    {
        public string Id { get; set; }

        public string? FirstName { get; set; }
        public string? LastName { get; set; }

        public string? Email { get; set; }
        /*[Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm password is required.")]
        [Display(Name = "Confirm Password")]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Passwords must be the same.")]
        public string ConfirmPassword { get; set; }
        */
        
        [DataType(DataType.PhoneNumber)]
        public string? Phone { get; set; }
        
        public string? Address { get; set; }
        
        public string? City { get; set; }
    }
}
