using Blazored.LocalStorage;
using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Globalization;
using System.Runtime.InteropServices;
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
        private List<Expense> AllExpenses { get; set; } = default!;
        private List<Expense> FilteredExpenses { get; set; } = default!;
        private decimal TotalAmount = 0;
        private IList<Expense> selectedExpense = new List<Expense>();
        private readonly List<string> Categories = new[] { "All" }.Concat(Enum.GetNames(typeof(ExpenseCategory))).ToList();
        private List<MonthlyExpense> MonthlyExpenses { get; set; } = new List<MonthlyExpense>();

        private Dictionary<string, List<MonthlyExpense>> dataByCategory = new();
        private string filteredCategory
        {
            get => _filteredCategory;
            set
            {
                _filteredCategory = value;
                GetExpensesWithFilters();
                CalculateTotalAmount();
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
            if (user.Id is null) { AllExpenses = new List<Expense>(); return; }

            await LoadAllExpenses(user.Id);           

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
            await LoadAllExpenses(userId);
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

            await LoadAllExpenses(userId);           
        }                

        private void GetExpensesWithFilters()
        {
            if (string.IsNullOrEmpty(filteredCategory) || filteredCategory == "All")
            {
                FilteredExpenses = AllExpenses;
                StateHasChanged();
            }
            else
                FilteredExpenses = AllExpenses.Where(e => e.Category == filteredCategory).ToList();
        }

        private async Task LoadAllExpenses(string userId)
        {
            AllExpenses = (await _listExpensesService.GetListByUserIdAsync(userId)).Expenses ?? new List<Expense>();
            GetExpensesWithFilters();
            selectedExpense = new List<Expense> { FilteredExpenses.FirstOrDefault() };
            CalculateTotalAmount();

            GroupByMonthExpenses();
            GroupExpensesByCategory();            
        }

        private void GroupByMonthExpenses()
        {
            MonthlyExpenses = AllExpenses
                // Grouping by year and month of the CreationDate
                .GroupBy(exp => new { exp.CreationDate.Year, exp.CreationDate.Month })
                // Selecting each group to create a MonthlyExpense object with a specific month and total amount of that month
                .Select(g => new MonthlyExpense
                {
                    Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
                    TotalAmount = g.Sum(exp => exp.Amount)
                })
                // Ordering the MonthlyExpense by their date
                .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", CultureInfo.InvariantCulture))
                .ToList();

        }

        private void GroupExpensesByCategory()
        {
            var allMonths = AllExpenses
                .Select(e => new DateTime(e.CreationDate.Year, e.CreationDate.Month, 1))
                .Distinct()
                .OrderBy(day => day)
                .ToList();

            var allCategories = AllExpenses
                .Select(e => e.Category)
                .Distinct()
                .ToList();

            dataByCategory = new Dictionary<string, List<MonthlyExpense>>();

            foreach (var category in allCategories)
            {
                var list = new List<MonthlyExpense>();

                foreach (var month in allMonths)
                {
                    var total = AllExpenses
                        .Where(exp => exp.Category == category && exp.CreationDate.Year == month.Year && exp.CreationDate.Month == month.Month)
                        .Sum(e => e.Amount);

                    list.Add(new MonthlyExpense { Month = month.ToString("MMM yyyy", CultureInfo.InvariantCulture), TotalAmount = total });
                }

                dataByCategory[category] = list;
            }
        }

        private decimal CalculateTotalAmount()
            => TotalAmount = FilteredExpenses?.Sum(expense => expense.Amount) ?? 0;

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

        private string FormatAsEUR(object value)
        {
            return ((double)value).ToString("C", new System.Globalization.CultureInfo("de-DE"));
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
