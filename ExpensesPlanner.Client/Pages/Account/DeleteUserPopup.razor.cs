using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using Microsoft.AspNetCore.Components;
using Radzen;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class DeleteUserPopup
    {
        [Inject] private IUserService _userService { get; set; } = default!;
        [Inject] private DialogService dialogService { get; set; } = default!;
        [Inject] NotificationService NotificationService { get; set; } = default!;
        [Parameter] public string UserId { get; set; } = string.Empty;

        private ApplicationUser? user = new();

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            user = await _userService.GetUserByIdAsync(UserId);

        }

        private async Task DeleteUserAsync()
        {
            var response = await _userService.DeleteUserAsync(UserId);

            if (response.IsSuccessStatusCode)
            {
                dialogService.Close();
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Success,
                    Summary = "Success",
                    Detail = "User deleted successfully.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });

            }
            else
            {
                dialogService.Close();
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = "Error deleting user.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
            }
        }

        private void CancelDelete()
        {
            dialogService.Close("Delete cancelled.");            
        }
    }
}
