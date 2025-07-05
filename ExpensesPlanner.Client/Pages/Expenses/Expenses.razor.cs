using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class Expenses
    {
        [Inject] private ExpenseService expenseService { get; set; } = default!;
        private List<Expense> expenses;

        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);
            expenses = await expenseService.GetAllExpenses();
            //expenses = await HttpClient.GetFromJsonAsync<List<Expense>>("https://localhost:8081/api/expenses") ?? new List<Expense>();
        }
    }
}
