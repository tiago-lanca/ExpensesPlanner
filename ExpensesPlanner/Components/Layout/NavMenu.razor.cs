using Blazored.LocalStorage;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;
using Radzen;
using System.Text.Json;

namespace ExpensesPlanner.Components.Layout
{
    public partial class NavMenu
    {
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private DialogService DialogService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;

        private bool IsInteractive = false;

        public async Task OpenLoginPopup()
        {
            await LoadStateAsync();

            await DialogService.OpenAsync<LoginPopup>("",
                    new Dictionary<string, object>(),
                    new DialogOptions()
                    {
                        //CssClass = "login-dialog",
                        CloseDialogOnOverlayClick = false,
                        //Width = "100%",
                        //Height = Settings != null ? Settings.Height : "712px",
                        //Left = Settings != null ? Settings.Left : null,
                        //Top = Settings != null ? Settings.Top : null
                    });

            await SaveStateAsync();
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

            await JSRuntime.InvokeVoidAsync("window.localStorage.setItem", "DialogSettings", JsonSerializer.Serialize(Settings));
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
