using System.ComponentModel.DataAnnotations;

namespace ExpensesPlanner.ViewModels
{
    public class EditExpenseViewModel
    {
        [Required]
        public decimal Amount { get; set; }
        [Required]
        public string? Description { get; set; }
    }
}
