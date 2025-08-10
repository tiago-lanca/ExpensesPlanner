using ExpensesPlanner.Client.Enums;

namespace ExpensesPlanner.Client.DTO
{
    public class SalaryExpensesChart
    {
        public string ChartDataItem { get; set; } = string.Empty;
        public int? Salary { get; set; }
        public ExpenseCategory? ExpenseCategory { get; set; }

        public decimal TotalAmount { get; set; }
    }
}
