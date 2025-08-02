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
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class Expenses
    {
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private AuthService AuthService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private DialogService dialogService { get; set; } = default!;
        private List<Expense> AllExpenses { get; set; } = default!;
        private List<Expense> FilteredExpenses { get; set; } = default!;
        private decimal TotalAmount = 0;
        private string chartKey = Guid.NewGuid().ToString();
        private bool showDataLabels = true;
        private bool allYearsExpensesActive = false;
        private bool previousYearEnabled => ExpenseYear <= DateTime.Now.Year - 5;
        private bool nextYearEnabled => ExpenseYear >= DateTime.Now.Year;
        private int ExpenseYear = DateTime.Now.Year;
        private IList<Expense> selectedExpense = new List<Expense>();
        private readonly List<string> Categories = Enum.GetNames(typeof(ExpenseCategory)).ToList();
        private readonly List<string> Months = Enum.GetNames(typeof(Months)).ToList();
        private List<MonthlyExpense> MonthlyExpenses { get; set; } = new List<MonthlyExpense>();

        private Dictionary<string, List<MonthlyExpense>> dataByCategory = new();
        private string? filteredCategory
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
        private string? filteredMonth
        {
            get => _filteredMonth;
            set
            {
                _filteredMonth = value;
                GetExpensesWithFilters();
                CalculateTotalAmount();
            }
        }
        private string? _filteredMonth;
        private string? userId;

        protected override async Task OnInitializedAsync()
        {
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            await Task.Delay(500);

            var user = await AuthService.GetCurrentUserAsync(token);
            userId = user.Id;
            if (user.Id is null) { AllExpenses = new List<Expense>(); return; }

            await LoadExpenses(user.Id);           
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
            await LoadExpenses(userId);
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

            await LoadExpenses(userId);           
        }                

        private void GetExpensesWithFilters()
        {
            if (allYearsExpensesActive)
                FilteredExpenses = AllExpenses;
            else
            {
                FilteredExpenses = AllExpenses
                    .Where(exp =>
                        (filteredCategory == null || exp.Category == filteredCategory) &&
                        (filteredMonth == null || exp.CreationDate.ToString("MMM", CultureInfo.InvariantCulture) == filteredMonth) &&
                        (exp.CreationDate.Year == ExpenseYear)
                    )
                    .ToList();
            }

            SelectFirstExpense();
            CalculateTotalAmount();

            GroupExpensesByCategory();
        }

        private async Task LoadExpenses(string userId)
        {
            AllExpenses = (await _listExpensesService.GetListByUserIdAsync(userId)).Expenses ?? new List<Expense>();
            GetExpensesWithFilters();
        }

        private void GroupByMonthExpenses()
        {
            //MonthlyExpenses = FilteredExpenses
            //    // Filtering expenses by the selected year
            //    .Where(exp => exp.CreationDate.Year == ExpenseYear)
            //    // Grouping by year and month of the CreationDate
            //    .GroupBy(exp => new { exp.CreationDate.Year, exp.CreationDate.Month })
            //    // Selecting each group to create a MonthlyExpense object with a specific month and total amount of that month
            //    .Select(g => new MonthlyExpense
            //    {
            //        Month = new DateTime(g.Key.Year, g.Key.Month, 1).ToString("MMM yyyy", CultureInfo.InvariantCulture),
            //        TotalAmount = g.Sum(exp => exp.Amount)
            //    })
            //    // Ordering the MonthlyExpense by their date
            //    .OrderBy(x => DateTime.ParseExact(x.Month, "MMM yyyy", CultureInfo.InvariantCulture))
            //    .ToList();
        }

        private void GroupExpensesByCategory()
        {
            var allMonths = FilteredExpenses
                .Where(exp => allYearsExpensesActive || exp.CreationDate.Year == ExpenseYear)
                .Select(e => new DateTime(e.CreationDate.Year, e.CreationDate.Month, 1))
                .Distinct()
                .OrderBy(day => day)
                .ToList();

            var allCategories = FilteredExpenses
                .Where(exp => allYearsExpensesActive || exp.CreationDate.Year == ExpenseYear)
                .Select(e => e.Category)
                .Distinct()
                .ToList();

            dataByCategory = new Dictionary<string, List<MonthlyExpense>>();

            foreach (var category in allCategories)
            {
                var list = new List<MonthlyExpense>();

                foreach (var month in allMonths)
                {
                    var total = FilteredExpenses
                        .Where(exp => exp.Category == category && exp.CreationDate.Year == month.Year && exp.CreationDate.Month == month.Month)
                        .Sum(e => e.Amount);


                    list.Add(new MonthlyExpense { Month = month.ToString("MMM yyyy", CultureInfo.InvariantCulture), TotalAmount = total });

                }
                
                dataByCategory[category] = list;
            }

            // Force recreation of StackedColumns
            chartKey = Guid.NewGuid().ToString();
            StateHasChanged();
        }

        private void SelectFirstExpense() => selectedExpense = new List<Expense> { FilteredExpenses.FirstOrDefault() };

        private void ToggleAllYearsButton() 
        { 
            allYearsExpensesActive = !allYearsExpensesActive; 
            GetExpensesWithFilters(); 
        }
        private async Task PreviousYear()
        {
            ExpenseYear -= 1;
            GetExpensesWithFilters();
        }

        private async Task NextYear()
        {
            ExpenseYear += 1;
            GetExpensesWithFilters();
        }

        private decimal CalculateTotalAmount()
            => TotalAmount = FilteredExpenses?.Sum(expense => expense.Amount) ?? 0;

        private void ClearCategoryFilter() { filteredCategory = null; LoadExpenses(userId); }


        private void ClearMonthFilter() { filteredMonth = null; LoadExpenses(userId); }

        private string FormatAsEUR(object value)
        {
            return ((double)value).ToString("C", new System.Globalization.CultureInfo("de-DE"));
        }


        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
               var width = await JSRuntime.InvokeAsync<int>("blazorGetWindowWidth");
               showDataLabels = width >= 650; // Show data labels only on larger screens
               StateHasChanged();
            }
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
