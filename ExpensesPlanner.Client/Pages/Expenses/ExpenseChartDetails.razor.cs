using ExpensesPlanner.Client.Models;
using Microsoft.AspNetCore.Components;
using System.Runtime.CompilerServices;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class ExpenseChartDetails
    {
        [Parameter] public string UserId { get; set; } = string.Empty;
        [Parameter] public List<Expense> expensesList { get; set; } = default!;

        private decimal TotalAmount;
        private string Category => expensesList?.FirstOrDefault()?.Category!;

        protected override void OnInitialized()
        {
            if (expensesList != null && expensesList.Any())
            {
                TotalAmount = expensesList.Sum(e => e.Amount);
            }
            else
            {
                TotalAmount = 0;
            }
        }
    }
}
