using Blazored.LocalStorage;
using DevExpress.Blazor;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Services;
using ExpensesPlanner.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Radzen;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace ExpensesPlanner.Client.Pages.Expenses
{
    public partial class Finances
    {
        [Inject] private CategoryLimitService _categoryLimitService { get; set; } = default!;
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Inject] private ILocalStorageService _localStorage { get; set; } = default!;
        [Inject] private AuthService authService { get; set; } = default!;
        [Inject] private ListExpensesService _listExpensesService { get; set; } = default!;
        [Inject] private NavigationManager Navigation { get; set; } = default!;
        [Inject] private Radzen.DialogService dialogService { get; set; } = default!;
        [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
        [Inject] private IUserService _userService { get; set; } = default!;
        [Inject] private DialogSettingsService DialogSettings { get; set; } = default!;

        private string todayMonth => DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture);
        private readonly string dropdownCategoryDefaultMessage = "Select Category";
        private string userId = string.Empty;
        private bool hasChanges = false;
        private bool PopupVisible = false;
        private string selectedAddCategory = string.Empty;
        private string progressBarClass = "progressbar-blue";
        private string filteredDate => $"{filteredMonth} {filteredYear}";
        private string filteredMonth = DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture);
        private int filteredYear = DateTime.Now.Year;
        private bool previousDateEnabled => filteredYear <= (ExpenseYear - 3) && filteredMonth == todayMonth; // 3 years behind
        private bool nextDateEnabled => filteredYear == ExpenseYear && filteredMonth == todayMonth; // Actual date

        private int ExpenseYear => DateTime.Now.Year;
        private List<string> availableCategories = new();
        private ListExpenses listExpenses = new();
        private List<Expense> monthExpenses = new();
        private List<CategoryLimit> initialCategoryLimitList = new();
        private Dictionary<string, decimal> expensesByCategory = new();
        private List<CategoryLimit> allCategoryLimitsList = new();
        private List<CategoryLimit> filteredCategoryLimits = new();
        private readonly List<string> Months = Enum.GetNames(typeof(Months)).ToList();
        private List<SalaryExpensesChart> chartData = new();
        private List<ExpenseCategory> monthCategories => GetExpensesCategoriesMonthFilter();
        private ApplicationUser? User { get; set; } = default!;

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            selectedAddCategory = dropdownCategoryDefaultMessage;
                                   
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            User = await authService.GetCurrentUserAsync(token);          

            listExpenses = await _listExpensesService.GetListByUserIdAsync(User);

            await InitializeCategoryLimits();

            GetFilteredCategoryLimitByMonth();

            GetAvailableCategoriesToAdd();

            await RefreshChartData();
        }

        private void AddCategoryLimit(string category)
        {
            if (category.Equals(dropdownCategoryDefaultMessage))
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = "The selected category is invalid.",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });
                return;
            }            

            allCategoryLimitsList.Add(new CategoryLimit
            {
                Category = category,
                Limit = 100, // Default limit
                Date = new DateDto {  Month = Enum.Parse<Months>(filteredMonth), Year = filteredYear }
            });

            GetFilteredCategoryLimitByMonth();

            selectedAddCategory = dropdownCategoryDefaultMessage;
            GetAvailableCategoriesToAdd();

            CheckHasChanges();
        }

        private void RemoveCategoryLimitItem(CategoryLimit categoryLimit)
        {
            allCategoryLimitsList.Remove(categoryLimit);
            filteredCategoryLimits.Remove(categoryLimit);
            GetAvailableCategoriesToAdd();
            CheckHasChanges();
        }

        private void OnLimitValueChanged(CategoryLimit changedCategory, int value)
        {
            changedCategory.Limit = value;
            
            var index = allCategoryLimitsList.FindIndex(cat => cat.Category == changedCategory.Category);
            if (index >= 0)
            {
                allCategoryLimitsList[index] = changedCategory;
            }

            CheckHasChanges();
        }

        private async Task UpdateCategoryLimits()
        {
            try
            {
                UserCategoryLimit userCategoryLimit = new UserCategoryLimit
                {
                    UserId = userId,
                    CategoryLimits = allCategoryLimitsList
                };

                var response = await _categoryLimitService.UpdateUserCategoryLimitsAsync(userCategoryLimit);

                if (response.IsSuccessStatusCode)
                {
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Category limits updated successfully.",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });

                    // Refresh data to a new initialCategoryLimitList
                    initialCategoryLimitList = allCategoryLimitsList
                        .Select(catlimit => new CategoryLimit { Category = catlimit.Category, Limit = catlimit.Limit })
                        .ToList();
                    hasChanges = false;
                }

                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    NotificationService.Notify(new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = $"Failed response to update category limits: {errorMessage}",
                        Duration = 4000,
                        ShowProgress = true,
                        CloseOnClick = true,
                        Payload = DateTime.Now
                    });
                }
            }

            catch (Exception ex)
            {
                NotificationService.Notify(new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Failed to update category limits: {ex.Message}",
                    Duration = 4000,
                    ShowProgress = true,
                    CloseOnClick = true,
                    Payload = DateTime.Now
                });

                Console.WriteLine($"Error updating category limits: {ex.Message}");
                return;
            }
        }

        private async Task InitializeCategoryLimits()
        {
            var userCategoryLimit = await _categoryLimitService.GetUserCategoryLimitsAsync(userId);
            allCategoryLimitsList = userCategoryLimit.CategoryLimits ?? new List<CategoryLimit>();

            initialCategoryLimitList = allCategoryLimitsList
                .Select(catlimit => new CategoryLimit { Category = catlimit.Category, Limit = catlimit.Limit })
                .ToList();

            GetExpensesAmountByCategory();
        }

        private void GetExpensesAmountByCategory()
        {    
            foreach (var category in filteredCategoryLimits)
            {
                expensesByCategory[category.Category] = listExpenses.Expenses!
                    .Where(expense => expense.CreationDate.Month == (int)Enum.Parse<Months>(filteredMonth)
                            && expense.CreationDate.Year == filteredYear 
                            && expense.Category == category.Category)
                    .Sum(expense => expense.Amount);
            }
        }

        private void GetFilteredCategoryLimitByMonth()
        {
            filteredCategoryLimits = allCategoryLimitsList
                .Where(catlimit => catlimit.Date?.Month == Enum.Parse<Months>(filteredMonth) && catlimit.Date.Year == filteredYear)
                .ToList();

            GetExpensesAmountByCategory();
            GetAvailableCategoriesToAdd();
        }

        private async Task RefreshChartData()
        {
            User = await _userService.GetApplicationUserByIdAsync(userId);

            // Filter expenses by month and year
            var filteredExpensesByMonthYear = listExpenses.Expenses!
                .Where(exp => exp.CreationDate.ToString("MMM", CultureInfo.InvariantCulture) == filteredMonth && exp.CreationDate.Year == filteredYear);

            // Group expenses by category and calculate total amount
            var groupedExpensesByCategory = filteredExpensesByMonthYear
                .GroupBy(expense => expense.Category)
                .Select(group => new SalaryExpensesChart
                {
                    ChartDataItem = "Expenses",
                    ExpenseCategory = Enum.Parse<ExpenseCategory>(group.Key!),
                    TotalAmount = group.Sum(exp => exp.Amount)
                });

            if(!MonthlyExpenseExists)
            {
                await CreateUserMonthlyExpense(filteredCategoryLimits, groupedExpensesByCategory.ToList());
            }
            else
                await UpdateUserMonthlyExpense(groupedExpensesByCategory.ToList());

            chartData = GetSalaryExpensesChart();

            /*var monthSalary = user.MonthlyExpenseChart
                .FirstOrDefault(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear)?
                .SalaryExpensesChart?.FirstOrDefault(chart => chart.ChartDataItem == "Salary")?.Salary ?? user.Salary;*/
        }

        private void GetAvailableCategoriesToAdd()
        {
            availableCategories = Enum.GetNames(typeof(ExpenseCategory)).ToList();

            foreach (var category in filteredCategoryLimits)
            {
                availableCategories.Remove(category.Category);
            }
        }

        private async Task SalaryBarClick(ChartSeriesClickEventArgs info)
        {
            if (info.Series.Name == "Salary")
            {
                //PopupVisible = true;
                await DialogSettings.LoadStateAsync();

                await dialogService.OpenAsync<SalaryPresetPopup>("Set a Salary",
                       new Dictionary<string, object>() { 
                           { "UserId", userId },
                           { "filteredMonth", filteredMonth },
                           { "filteredYear", filteredYear }
                       },
                       new DialogOptions()
                       {
                           CssClass = "userdetails-dialog",
                           CloseDialogOnOverlayClick = false
                       });

                await DialogSettings.SaveStateAsync();

                await RefreshChartData();
            }

            else
            {
                monthExpenses = listExpenses.Expenses!
                    .Where(Expenses => Expenses.CreationDate.Month == (int)Enum.Parse<Months>(filteredMonth)
                            && Expenses.CreationDate.Year == filteredYear
                            && Expenses.Category == info.Series.Name).ToList();

                foreach(var expense in monthExpenses)
                {
                    Console.WriteLine(expense.Description);
                }

                PopupVisible = true;
            }
            
        }

        private async Task PreviousMonth()
        {
            if (Months.IndexOf(filteredMonth) == 0)
            {
                filteredMonth = Months.Last();
                filteredYear -= 1;
            }
            else
            {
                filteredMonth = Months[Months.IndexOf(filteredMonth) - 1];
            }

            GetFilteredCategoryLimitByMonth();
            await RefreshChartData();

            //filteredMonth = (DateTime.Now.Month - 1).ToString("MMM");
            //filteredYear = (DateTime.Now.Year - 1).ToString();
        }
        
        private async Task NextMonth()
        {
            if (Months.IndexOf(filteredMonth) == Months.Count - 1)
            {
                filteredMonth = Months.First();
                filteredYear += 1;
            }
            else
            {
                filteredMonth = Months[Months.IndexOf(filteredMonth) + 1];
            }

            GetFilteredCategoryLimitByMonth();
            await RefreshChartData();
        }

        private List<ExpenseCategory> GetExpensesCategoriesMonthFilter() =>
            listExpenses.Expenses!
            .Where(expense => expense.CreationDate.Month == (int)Enum.Parse<Months>(filteredMonth)
                    && expense.CreationDate.Year == filteredYear)
            .Select(expense => Enum.Parse<ExpenseCategory>(expense.Category!))
            .Distinct()
            .ToList();

        private int GetLimitPercentage(int limit) => (limit * 100) / 1200;
        private int GetCategoryExpensePercentage(decimal expense, int limit) => Convert.ToInt16((expense * 100) / limit);
        
        private bool CheckHasChanges() => hasChanges = !initialCategoryLimitList.SequenceEqual(allCategoryLimitsList, new CategoryLimitComparer());
        
        private void OnSelectedCategory(DropDownButtonItemClickEventArgs args) => selectedAddCategory = args.ItemInfo.Text;
        
        private async Task UpdateUserMonthlyExpense(List<SalaryExpensesChart> dataChart)
        {
            User = await _userService.GetApplicationUserByIdAsync(userId);

            // Find the MonthlyExpense for the current filtered month and year
            var monthlyExpense = User?.MonthlyExpenseChart
                .FirstOrDefault(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear);

            if (monthlyExpense != null)
            {
                /*// Update salary in the existing SalaryExpensesChart
                var chart = monthlyExpense.SalaryExpensesChart
                    .FirstOrDefault(chart => chart.ChartDataItem == "Salary");
                if (chart != null)
                {
                    chart.Salary = user.Salary_Preset_Enabled ? user.Salary_Preset : chart.Salary;
                }
                else
                {
                    // If Salary chart data does not exist, add it
                    monthlyExpense.SalaryExpensesChart.Add(new SalaryExpensesChart
                    {
                        ChartDataItem = "Salary",
                        Salary = user.Salary_Preset == null ? 200 : user.Salary_Preset
                    });
                }*/

                // Update the expense chart in the existing SalaryExpensesChart
                var expenseChart = User?.MonthlyExpenseChart
                    .FirstOrDefault(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear)?.SalaryExpensesChart;

                expenseChart?.RemoveAll(chart => chart.ChartDataItem == "Expenses");

                expenseChart?.AddRange(dataChart);
                await _userService.UpdateUserAsync(User!);
            }
        }
        
        private async Task CreateUserMonthlyExpense(List<CategoryLimit> categoryLimitsFiltered, List<SalaryExpensesChart> dataChart)
        {
            var newDataChart = new List<SalaryExpensesChart>()
            {
                new SalaryExpensesChart{ ChartDataItem = "Salary", Salary = User?.Salary_Preset is null ? 200 : User.Salary_Preset }                               
            };

            newDataChart.AddRange(dataChart);

            MonthlyExpenseChart monthlyExpense = new MonthlyExpenseChart
            {
                Date = new DateDto { Month = Enum.Parse<Months>(filteredMonth), Year = filteredYear },
                CategoryLimits = categoryLimitsFiltered,
                SalaryExpensesChart = newDataChart
            };
            User?.MonthlyExpenseChart.Add(monthlyExpense);

            await _userService.UpdateUserAsync(User!);
        }

        private List<SalaryExpensesChart> GetSalaryExpensesChart()
        {
            return User?.MonthlyExpenseChart
                .FirstOrDefault(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear)?
                .SalaryExpensesChart ?? new List<SalaryExpensesChart>();
        }

        private bool MonthlyExpenseExists => User!.MonthlyExpenseChart
            .Any(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear);
        private bool IsCategoryLimitListEmpty => filteredCategoryLimits.Count == 0;

        
    }
}
