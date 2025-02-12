using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpensesPlanner.Models;
using ExpensesPlanner.Data;

namespace ExpensesPlanner.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ExpensesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            var allExpenses = _context.Expenses.ToList();
            var totalExpenses = allExpenses.Sum(expense => expense.Amount);

            ViewBag.Expenses = totalExpenses;

            return View(await _context.Expenses.ToListAsync());
        }

        public IActionResult Details(int? id)
        {
            var expense = _context.Expenses.FirstOrDefault(expense => expense.Id == id);
            return View(expense);
        }
    }
}
