using ExpensesPlanner.Client.Services;
using ExpensesPlanner.Client.Models;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class CreateExpense
    {
        List<Expense> expenses = new();
        private ExpenseService ExpenseService { get; set; } = default!;
        protected override async Task OnInitializedAsync()
        {
            expenses = await ExpenseService.GetExpenseByNameAsync("Carro");
        }
    }
}
