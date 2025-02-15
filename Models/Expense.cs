using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.Models
{
    public class Expense
    {
        public int Id { get; set; }
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string? Description { get; set; }
        public string? UserId { get; set; }
        
    }
}
