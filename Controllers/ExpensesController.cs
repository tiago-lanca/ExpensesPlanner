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
using ExpensesPlanner.ViewModels;
using System.Security.Claims;

namespace ExpensesPlanner.Controllers
{
    public class ExpensesController : Controller
    {
        private readonly IExpenseRepository _expenseRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ExpensesController(IExpenseRepository expenseRepository, IHttpContextAccessor httpContextAccessor)
        {
            _expenseRepository = expenseRepository;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<IActionResult> Edit(int id)
        {
            Expense expense = await _expenseRepository.GetByIdAsync(id);
            if (expense == null) return View("Error");

            return View(expense);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, Expense expense)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit expense");
                return View("Edit", expense);
            }

            _expenseRepository.Update(expense);
            return RedirectToAction("Index");
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Create(CreateExpenseViewModel expenseVM)
        {
            if (ModelState.IsValid)
            {
                expenseVM.UserId = _httpContextAccessor.HttpContext.User.FindFirst(ClaimTypes.NameIdentifier).Value;

                Expense newExpense = new Expense
                {
                    Amount = expenseVM.Amount,
                    Description = expenseVM.Description,
                    UserId = expenseVM.UserId,
                };

                _expenseRepository.Add(newExpense);
                return RedirectToAction("Index");
            }

            else
            {
                return View(expenseVM);
            }

        }

        public async Task<IActionResult> Delete(int id)
        {
            Expense expense = await _expenseRepository.GetByIdAsync(id);
            _expenseRepository.Delete(expense);
            _expenseRepository.Save();
            return RedirectToAction("Index");
        }
    }
}
