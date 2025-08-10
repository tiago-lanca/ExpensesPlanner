using ExpensesPlanner.Client.DTO;
using ExpensesPlanner.Client.Enums;
using ExpensesPlanner.Client.Interfaces;
using ExpensesPlanner.Client.Models;
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
        [Inject] private NotificationService NotificationService { get; set; } = default!;
        [Parameter] public string UserId { get; set; } = string.Empty;
        [Parameter] public string filteredMonth { get; set; } = string.Empty;
        [Parameter] public int filteredYear { get; set; } = DateTime.Now.Year;

        private int MonthSalary;
        private ApplicationUser user = new();
        private CultureInfo CultureInfo = CultureInfo.GetCultureInfo("de-DE");
        private bool EnableSalaryPreset = false;

        protected override async Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            user = await _userService.GetUserByIdAsync(UserId);

            MonthSalary = user.MonthlyExpenseChart
                .FirstOrDefault(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear)?
                .SalaryExpensesChart.FirstOrDefault(chart => chart.ChartDataItem == "Salary")?.Salary ?? 0;
        }

        private async Task SaveSalaryPreset()
        {
            try
            {
                var userMonthSalary = user.MonthlyExpenseChart
                                        .FirstOrDefault(me => me.Date.Month == Enum.Parse<Months>(filteredMonth) && me.Date.Year == filteredYear)?
                                        .SalaryExpensesChart.FirstOrDefault(chart => chart.ChartDataItem == "Salary");

                userMonthSalary.Salary = MonthSalary;

                // If the salary preset is enabled, set the Salary and Salary_Preset properties for the user
                if (EnableSalaryPreset) { user.Salary_Preset = MonthSalary; user.Salary_Preset_Enabled = true; }

                var response = await _userService.UpdateUserAsync(user);

                if(response.IsSuccessStatusCode)
                {
                    NotificationMessage message = new NotificationMessage
                    {
                        Severity = NotificationSeverity.Success,
                        Summary = "Success",
                        Detail = "Salary saved successfully.",
                        Duration = 4000
                    };
                    NotificationService.Notify(message);
                    CloseSalarySetDialog();
                }
                else
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    NotificationMessage message = new NotificationMessage
                    {
                        Severity = NotificationSeverity.Error,
                        Summary = "Error",
                        Detail = $"Failed to save salary preset: {errorMessage}",
                        Duration = 4000
                    };
                    NotificationService.Notify(message);
                }
            }

            catch (Exception ex)
            {
                NotificationMessage message = new NotificationMessage
                {
                    Severity = NotificationSeverity.Error,
                    Summary = "Error",
                    Detail = $"Failed in the userservice: {ex.Message}",
                    Duration = 4000
                };
            }
                
            
        }

        private void CheckedChanged(bool value)
        {
            EnableSalaryPreset = value;
        }

        private void CloseSalarySetDialog()
        {
            DialogService.Close();
        }
    }
}
