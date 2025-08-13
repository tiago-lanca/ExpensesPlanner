using ExpensesPlanner.Client.Enums;

namespace ExpensesPlanner.Client.DTO
{
    public class MonthlyAmount
    {
        public string? Month { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
