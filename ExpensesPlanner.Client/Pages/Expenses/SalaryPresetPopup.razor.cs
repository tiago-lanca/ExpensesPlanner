using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Pages.Account;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Globalization;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class SalaryPresetPopup
    {
        [Inject] private IUserService _userService { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Parameter] public string UserId { get; set; } = string.Empty;
        private int Salary { get; set; } = 0;
        private UserDetails user = new();
        private CultureInfo CultureInfo = CultureInfo.GetCultureInfo("de-DE");

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            user = await _userService.GetUserByIdAsync(UserId);
        }

        private void CloseSalarySetDialog()
        {
            DialogService.Close();
        }
    }
}
