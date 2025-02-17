using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;
using ExpensesPlanner.Repository;
using ExpensesPlanner.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ExpensesPlanner.Controllers
{
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardRepository _adminDashboardRepository;
        public AdminDashboardController(IAdminDashboardRepository adminDashboardRepository)
        {
            _adminDashboardRepository = adminDashboardRepository;
        }

        public async Task<IActionResult> Index()
        {
            if (User.IsInRole(UserRoles.Admin))
            {
                var users = await _adminDashboardRepository.GetAllUsers();
                List<AdminDashboardUserViewModel> result = new List<AdminDashboardUserViewModel>();

                foreach(User user in users)
                {
                    var userDashboard = new AdminDashboardUserViewModel()
                    {
                        Id = user.Id,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Phone = user.Phone,
                        Role = await _adminDashboardRepository.GetUserRole(user.Id),
                    };

                    result.Add(userDashboard);
                    
                }

                return View(result);
            }
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
