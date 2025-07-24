namespace ExpensesPlanner.Client.Models
{
    public class MonthlyExpense
    {
        public string Month { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } = 0;
    }
}
