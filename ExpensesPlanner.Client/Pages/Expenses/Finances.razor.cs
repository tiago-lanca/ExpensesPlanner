using Blazored.LocalStorage;
using DevExpress.Blazor;
using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Models;
using ExpensesPlanner.Client.Pages.Account;
using ExpensesPlanner.Client.Services;
using ExpensesPlanner.Client.Utilities;
using Microsoft.AspNetCore.Components;
using Radzen;
using System.Globalization;
using System.Threading.Tasks;

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

        private int todayYear => DateTime.Now.Year;
        private string todayMonth => DateTime.Now.ToString("MMM", CultureInfo.InvariantCulture);
        private readonly string dropdownCategoryDefaultMessage = "Select Category";
        private string userId = string.Empty;
        private bool hasChanges = false;
        public  bool isXSmallScreen { get; set; }
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
        private List<CategoryLimit> initialCategoryLimitList = new();
        private Dictionary<string, decimal> expensesByCategory = new();
        private List<CategoryLimit> allCategoryLimitsList = new();
        private List<CategoryLimit> filteredCategoryLimits = new();
        private readonly List<string> Months = Enum.GetNames(typeof(Months)).ToList();
        private List<SalaryExpensesChart> chartData = new();
        private List<ExpenseCategory> monthCategories => GetExpensesCategoriesMonthFilter();

        protected override async Task OnInitializedAsync()
        {
            base.OnInitialized();
            selectedAddCategory = dropdownCategoryDefaultMessage;
                                   
            var token = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(token)) { Navigation.NavigateTo("/"); return; }

            var user = await authService.GetCurrentUserAsync(token);
            userId = user.Id!;

            
            listExpenses = await _listExpensesService.GetListByUserIdAsync(userId);
            await InitializeCategoryLimits();
            GetFilteredCategoryLimitByMonth();


            GetAvailableCategoriesToAdd();
            RefreshChartData();
            //List<ExpenseCategory> monthCategories = filteredCategoryLimits.Select(cat => Enum.Parse<ExpenseCategory>(cat.Category)).ToList();


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
                expensesByCategory[category.Category] = listExpenses.Expenses
                    .Where(expense => expense.CreationDate.Month == (int)Enum.Parse<Months>(filteredMonth)
                            && expense.CreationDate.Year == filteredYear 
                            && expense.Category == category.Category)
                    .Sum(expense => expense.Amount);
            }
        }

        private void GetFilteredCategoryLimitByMonth()
        {
            filteredCategoryLimits = allCategoryLimitsList
                .Where(catlimit => catlimit.Date.Month == Enum.Parse<Months>(filteredMonth) && catlimit.Date.Year == filteredYear)
                .ToList();

            GetExpensesAmountByCategory();
            GetAvailableCategoriesToAdd();
        }

        private void RefreshChartData()
        {
            var filteredExpensesMonthYear = listExpenses.Expenses
                .Where(exp => exp.CreationDate.ToString("MMM", CultureInfo.InvariantCulture) == filteredMonth && exp.CreationDate.Year == filteredYear);

            var groupedExpensesCategory = filteredExpensesMonthYear
                .GroupBy(expense => expense.Category)
                .Select(group => new SalaryExpensesChart
                {
                    ChartDataItem = "Expenses",
                    ExpenseCategory = Enum.Parse<ExpenseCategory>(group.Key),
                    TotalAmount = group.Sum(exp => exp.Amount)
                });

            chartData = new List<SalaryExpensesChart>
            {
                new SalaryExpensesChart { ChartDataItem = "Salary", Salary = 1200 }                
            };

            chartData.AddRange(groupedExpensesCategory);
        }

        private void GetAvailableCategoriesToAdd()
        {
            availableCategories = Enum.GetNames(typeof(ExpenseCategory)).ToList();

            foreach (var category in filteredCategoryLimits)
            {
                availableCategories.Remove(category.Category);
            }
        }

        private void PreviousMonth()
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
            RefreshChartData();

            //filteredMonth = (DateTime.Now.Month - 1).ToString("MMM");
            //filteredYear = (DateTime.Now.Year - 1).ToString();
        }
        
        private void NextMonth()
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
            RefreshChartData();
        }

        private List<ExpenseCategory> GetExpensesCategoriesMonthFilter() =>
            listExpenses.Expenses
            .Where(expense => expense.CreationDate.Month == (int)Enum.Parse<Months>(filteredMonth)
                    && expense.CreationDate.Year == filteredYear)
            .Select(expense => Enum.Parse<ExpenseCategory>(expense.Category))
            .Distinct()
            .ToList();

        private int GetLimitPercentage(int limit) => (limit * 100) / 1200;
        private int GetCategoryExpensePercentage(decimal expense, int limit) => Convert.ToInt16((expense * 100) / 1200);
        
        private bool CheckHasChanges() => hasChanges = !initialCategoryLimitList.SequenceEqual(allCategoryLimitsList, new CategoryLimitComparer());
        
        private void OnSelectedCategory(DropDownButtonItemClickEventArgs args) => selectedAddCategory = args.ItemInfo.Text;
        
        private bool IsCategoryLimitListEmpty => allCategoryLimitsList.Count == 0;

    }
}
