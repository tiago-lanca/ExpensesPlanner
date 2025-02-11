using ExpensesPlanner.Data;
using ExpensesPlanner.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace ExpensesPlanner.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;
        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Expenses()
        {
            var allExpenses = _context.Expenses.ToList();
            var totalExpenses = allExpenses.Sum(expense => expense.Amount);

            ViewBag.Expenses = totalExpenses;
            return View(allExpenses);
        }

        public IActionResult CreateEditExpenses(int? id)
        {
            if(id != null)
            {
                // edit - load the form by id expense
                var expense = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
               
                return View(expense);
            }

            ViewBag.NextID = _context.Expenses.Any() ? _context.Expenses.Last().Id + 1 : 0;

            return View();
        }

        public IActionResult CreateEditExpensesForm(Expense model)
        {
            if(model.Id == 0)
            {
                // Create new expense
                _context.Expenses.Add(model);
            }
            else
            {
                // Edit an existing expense
                _context.Expenses.Update(model);
            }
            
            _context.SaveChanges();

            return RedirectToAction("Expenses");
        }

        public IActionResult DeleteExpense(int id)
        {
            var expense = _context.Expenses.SingleOrDefault(expense => expense.Id == id);
            _context.Expenses.Remove(expense);
            _context.SaveChanges();
            return RedirectToAction("Expenses");
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
