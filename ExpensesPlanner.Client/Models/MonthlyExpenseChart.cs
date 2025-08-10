using ExpensesPlanner.Client.DTO;

namespace ExpensesPlanner.Client.Models
{
    public class MonthlyExpenseChart
    {
        public DateDto Date { get; set; } = new DateDto();
        public List<CategoryLimit> CategoryLimits { get; set; } = new List<CategoryLimit>();
        public List<SalaryExpensesChart> SalaryExpensesChart { get; set; } = new List<SalaryExpensesChart>();
    }
}
