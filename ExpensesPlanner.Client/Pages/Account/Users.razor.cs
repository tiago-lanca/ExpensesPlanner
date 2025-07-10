using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Text.Json;

namespace ExpensesPlanner.Client.Pages.Account
{
    public partial class Users
    {
        private List<UserDetails> users;
        IList<UserDetails> selectedUsers;
        [Inject] private DialogService dialogService { get; set; } = default!;
        [Inject] private IUserService userService { get; set; } = default!;
        protected override async Task OnInitializedAsync()
        {
            await Task.Delay(500);
            await LoadAllUsersAsync();
            selectedUsers = new List<UserDetails>() { users.FirstOrDefault() };
        }

        private async Task LoadAllUsersAsync()
        {
            users = await userService.GetAllUsers();
        }

        private async Task OpenUserDetails(string userID)
        {
            await LoadStateAsync();

            await dialogService.OpenAsync<UserDetailsPopup>("Details",
                   new Dictionary<string, object>() { { "UserId", userID } },
                   new DialogOptions()
                   {
                       CssClass = "details-dialog",
                       CloseDialogOnOverlayClick = true,
                       Width = "800px",
                       //Height = Settings != null ? Settings.Height : "712px",
                       //Left = Settings != null ? Settings.Left : null,
                       //Top = Settings != null ? Settings.Top : null
                   });

            await SaveStateAsync();
        }

        private async Task OpenDeleteUserDialog(string userID)
        {
            await LoadStateAsync();

            await dialogService.OpenAsync<DeleteUserPopup>("Delete User",
                   new Dictionary<string, object>() { { "UserId", userID } },
                   new DialogOptions()
                   {
                       CssClass = "userdetails-dialog",
                       CloseDialogOnOverlayClick = false,
                       //Width = Settings != null ? Settings.Width : "712px",
                       //Height = Settings != null ? Settings.Height : "712px",
                       //Left = Settings != null ? Settings.Left : null,
                       //Top = Settings != null ? Settings.Top : null
                   });

            await SaveStateAsync();
            await LoadAllUsersAsync();
        }

        

        void OnDrag(System.Drawing.Point point)
        {
            JSRuntime.InvokeVoidAsync("eval", $"console.log('Dialog drag. Left:{point.X}, Top:{point.Y}')");

            if (Settings == null)
            {
                Settings = new DialogSettings();
            }

            Settings.Left = $"{point.X}px";
            Settings.Top = $"{point.Y}px";

            InvokeAsync(SaveStateAsync);
        }

        void OnResize(System.Drawing.Size size)
        {
            JSRuntime.InvokeVoidAsync("eval", $"console.log('Dialog resize. Width:{size.Width}, Height:{size.Height}')");

            if (Settings == null)
            {
                Settings = new DialogSettings();
            }

            Settings.Width = $"{size.Width}px";
            Settings.Height = $"{size.Height}px";

            InvokeAsync(SaveStateAsync);
        }

        DialogSettings _settings;
        public DialogSettings Settings
        {
            get
            {
                return _settings;
            }
            set
            {
                if (_settings != value)
                {
                    _settings = value;
                    InvokeAsync(SaveStateAsync);
                }
            }
        }

        private async Task LoadStateAsync()
        {
            await Task.CompletedTask;

            var result = await JSRuntime.InvokeAsync<string>("window.localStorage.getItem", "DialogSettings");
            if (!string.IsNullOrEmpty(result))
            {
                _settings = JsonSerializer.Deserialize<DialogSettings>(result);
            }
        }

        private async Task SaveStateAsync()
        {
            await Task.CompletedTask;

            await JSRuntime.InvokeVoidAsync("window.localStorage.setItem", "DialogSettings", JsonSerializer.Serialize<DialogSettings>(Settings));
        }

        public class DialogSettings
        {
            public string Left { get; set; }
            public string Top { get; set; }
            public string Width { get; set; }
            public string Height { get; set; }
        }
    }
}
