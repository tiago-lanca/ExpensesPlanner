using ExpensesPlanner.Data;
using ExpensesPlanner.Interface;
using ExpensesPlanner.Models;
using ExpensesPlanner.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ExpensesPlanner.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ApplicationDbContext _context;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (!ModelState.IsValid) return View(loginViewModel);

            var user = await _userManager.FindByEmailAsync(loginViewModel.Email);

            if (user != null)
            {
                // User found, check password
                var passwordCkeck = await _userManager.CheckPasswordAsync(user, loginViewModel.Password);

                if (passwordCkeck)
                {
                    var result = await _signInManager.PasswordSignInAsync(user, loginViewModel.Password, false, false);

                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Expenses");
                    }
                }

                // Password checking failed
                TempData["Error"] = "Email/Password is incorrect. Please, try again.";
                return View(loginViewModel);
            }
            // User not found
            TempData["Error"] = "Email/Password is incorrect. Please, try again.";
            return View(loginViewModel);
        }

        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel registerViewModel)
        {
            if (!ModelState.IsValid) return View(registerViewModel);

            var user = await _userManager.FindByEmailAsync(registerViewModel.Email);

            if(user != null)
            {
                // User registed found
                TempData["Error"] = "The email is already in use. Please, log in.";
                return View(registerViewModel);
            }

            User newUser = new User()
            {
                UserName = registerViewModel.Email,
                FirstName = registerViewModel.FirstName,
                LastName = registerViewModel.LastName,
                Email = registerViewModel.Email,
                Phone = registerViewModel.Phone,
                Address = registerViewModel.Address,
                City = registerViewModel.City,
                NormalizedEmail = registerViewModel.Email.ToUpper()
            };

            var newUserResponse = await _userManager.CreateAsync(newUser, registerViewModel.Password);

            if (newUserResponse.Succeeded)
            {
                await _userManager.AddToRoleAsync(newUser, UserRoles.User);
                //await _signInManager.SignInAsync(newUser, false);               
            }

            var result = await _signInManager.PasswordSignInAsync(newUser, registerViewModel.Password, false, false);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}
