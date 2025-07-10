using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class DeleteExpensePopup
    {
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private IExpenseService _expenseService { get; set; } = default!;
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Parameter] public string ExpenseId { get; set; } = string.Empty;
        private Expense expense = default!;
        private ListExpenses listExpenses = default!;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();
        }

        private async Task DeleteExpenseAsync()
        {
            //expense = await _expenseService.GetExpenseByIdAsync(ExpenseId);

            var response = await _expenseService.DeleteExpenseAsync(ExpenseId);
            if (response.IsSuccessStatusCode)
            {
                DialogService.Close();
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "Expense deleted successfully.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
            }
            else
            {
                DialogService.Close();
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = "Error deleting expense.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
            }
        }

        private void CancelDelete()
        {
            DialogService.Close("Delete cancelled.");
        }
    }
}
