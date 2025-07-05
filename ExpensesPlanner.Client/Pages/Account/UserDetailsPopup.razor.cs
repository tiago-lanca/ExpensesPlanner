using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class UserDetailsPopup
    {
        [Inject] private UserService _userService { get; set; } = default!;
        [Inject] private ExpenseService _expenseService { get; set; } = default!;
        [Parameter] public string UserId { get; set; } = string.Empty;
        [Parameter] public bool ShowClose { get; set; } = true;

        private List<Expense> expenses = new List<Expense>();
        private string imagePreview { get; set; } = string.Empty;

        private UserDetails user = new();

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            user = await _userService.GetUserByIdAsync(UserId);
            imagePreview = $"data:image/png;base64,{Convert.ToBase64String(user.ProfilePictureUrl)}";

            expenses = await _expenseService.GetAllExpenses();
        }
    }
}
