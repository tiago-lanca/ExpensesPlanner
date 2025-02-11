using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.Models
{
    public class Expense
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }

        [Required]
        public string? Description { get; set; }
        
    }
}
