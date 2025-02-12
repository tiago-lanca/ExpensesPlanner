using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ExpensesPlanner.Models;
using ExpensesPlanner.Data;
using ExpensesPlanner.Interface;

namespace ExpensesPlanner.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IExpenseRepository _expenseRepository;

        public ExpensesController(IExpenseRepository expenseRepository)
        {
            _expenseRepository = expenseRepository;
        }

        // GET: Expenses
        public async Task<IActionResult> Index()
        {
            IEnumerable<Expense> allExpenses = await _expenseRepository.GetAllExpenses();
            decimal totalExpenses = _expenseRepository.GetTotalAmount();

            ViewBag.Expenses = totalExpenses;

            return View(allExpenses);
        }

        public async Task<IActionResult> Details(int id)
        {
            Expense expense = await _expenseRepository.GetByIdAsync(id);
            return View(expense);
        }
    }
}
