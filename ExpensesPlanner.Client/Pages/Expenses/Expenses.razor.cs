using Blazored.LocalStorage;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Text.Json;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class Expenses
    {
        [Inject] private IExpenseService expenseService { get; set; } = default!;
        [Inject] private HttpClient HttpClient { get; set; } = default!;
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private AuthService authService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private DialogService dialogService { get; set; } = default!;
        private List<Expense> expenses = new List<Expense>();
        private IList<Expense> selectedExpenses = new List<Expense>();
        private string? userId;

        protected override async Task OnInitializedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            await Task.Delay(500);

            var user = await authService.GetCurrentUserAsync(token);
            userId = user.Id;

            if (user.Id is null) { expenses = new List<Expense>(); return; }

            await LoadAllUserExpensesAsync(user.Id);

            selectedExpenses = new List<Expense>() { expenses.FirstOrDefault() };
        }

        private async Task OpenDeleteExpenseDialog(string expenseId)
        {
            await LoadStateAsync();

            await dialogService.OpenAsync<DeleteExpensePopup>("Delete Expense",
                   new Dictionary<string, object>() { { "ExpenseId", expenseId } },
                   new DialogOptions()
                   {
                       CssClass = "details-dialog",
                       CloseDialogOnOverlayClick = false,
                       //Width = Settings != null ? Settings.Width : "712px",
                       //Height = Settings != null ? Settings.Height : "712px",
                       //Left = Settings != null ? Settings.Left : null,
                       //Top = Settings != null ? Settings.Top : null
                   });

            await SaveStateAsync();
            await LoadAllUserExpensesAsync(userId);
        }

        private async Task LoadAllUserExpensesAsync(string userId)
            => expenses = (await _listExpensesService.GetListByUserIdAsync(userId)).Expenses ?? new List<Expense>();
        

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
