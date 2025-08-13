using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Enums;

namespace ExpensesPlanner.Client.Models
{
    public class CategoryLimit
    {
        public string Category { get; set; } = string.Empty;
        public int Limit { get; set; }
        public DateDto? Date { get; set; }
    }
}
