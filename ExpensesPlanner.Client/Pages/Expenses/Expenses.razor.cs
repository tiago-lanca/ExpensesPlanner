using Blazored.LocalStorage;
using ExpensesPlanner.Client.Enums;
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
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private AuthService authService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private DialogService dialogService { get; set; } = default!;
        private List<Expense> expenses { get; set; } = default!;
        private IList<Expense> selectedExpenses = new List<Expense>();
        private readonly List<string> Categories = new[] { "All" }.Concat(Enum.GetNames(typeof(ExpenseCategory))).ToList();
        private string filteredCategory
        {
            get => _filteredCategory;
            set
            {
                _filteredCategory = value;
                GetExpensesWithFilters();
            }
        }
        private string? _filteredCategory;
        private string? userId;

        protected override async Task OnInitializedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            await Task.Delay(500);

            var user = await authService.GetCurrentUserAsync(token);
            userId = user.Id;

            if (user.Id is null) { expenses = new List<Expense>(); return; }

            await LoadUserExpensesAsync(user.Id);

            selectedExpenses = expenses.ToList();

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
            await LoadUserExpensesAsync(userId);
        }

        private async Task OpenUpdateExpenseDialog(string expenseId)
        {
            await LoadStateAsync();
            await dialogService.OpenAsync<EditExpensePopup>("Update Expense",
                   new Dictionary<string, object>() { 
                       { "ExpenseId", expenseId }, // Pass expenseId to the EditExpensePopup
                       { "UserId", userId }  // Pass userId to the EditExpensePopup
                   },
                   new DialogOptions()
                   {
                       CssClass = "details-dialog",
                       CloseDialogOnOverlayClick = false
                   });

            await SaveStateAsync();

            await LoadUserExpensesAsync(userId);
            
        }

        //private void OnFilterChanged(string filter)
        //{
        //    filteredCategory = filter;
        //    GetExpensesWithFilters();
        //}

        private void GetExpensesWithFilters()
        {
            if (string.IsNullOrEmpty(filteredCategory) || filteredCategory == "All")
            {
                selectedExpenses = expenses;
            }
            else
            {
                selectedExpenses = expenses.Where(e => e.Category == filteredCategory).ToList();
            }
        }

        private string GetIcon(string category)
        {

            return category switch
            {
                "Food" => "flatware",
                "Transport" => "directions_car",
                "Entertainment" => "movie",
                "Health" => "health_and_safety",
                "Gym" => "exercise",
                "Shopping" => "shopping_bag",
                "Bills" => "receipt",
                "Travel" => "travel",
                "Education" => "school",
                "Investment" => "finance_mode",
                "Savings" => "savings",
                "Other" => "search",
                _ => throw new NotImplementedException(),
            };
        }

        private async Task LoadUserExpensesAsync(string userId)
        {
            expenses = (await _listExpensesService.GetListByUserIdAsync(userId)).Expenses ?? new List<Expense>();
            GetExpensesWithFilters();
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
