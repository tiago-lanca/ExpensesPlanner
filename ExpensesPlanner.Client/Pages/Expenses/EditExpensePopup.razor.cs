using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Runtime.CompilerServices;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class EditExpensePopup
    {
        [Inject] private IExpenseService _expenseService { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;        
        [Parameter] public string UserId { get; set; } = string.Empty;
        [Parameter] public string ExpenseId { get; set; } = string.Empty;
        private Expense editExpense = new Expense();
        private List<string> Categories = Enum.GetNames(typeof(ExpenseCategory)).ToList();
        private bool busy;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            editExpense = await _expenseService.GetExpenseByIdAsync(ExpenseId);
        }

        private async Task UpdateExpense()
        {
            busy = true;
            try 
            { 
                var result = await _expenseService.UpdateExpenseAsync(editExpense);

                if (result.IsSuccessStatusCode)
                {
                    await Task.Delay(2000); // Simulate some delay for the update operation
                    busy = false;
                    DialogService.Close();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Expense updated successfully.",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });
                    
                }
                else
                {
                    busy = false;
                    DialogService.Close();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = "Error updating expense.",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });
                }
                
            }

            catch(Exception ex)
            {
                DialogService.Close();
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"An error occurred while updating the expense: {ex.Message}",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
            }

        }
        private void Cancel()
        {
            DialogService.Close();
        }
    }
}
