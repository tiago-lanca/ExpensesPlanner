using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;
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
                return View(users);
            }
            else
                return RedirectToAction("Index", "Home");
        }
    }
}
