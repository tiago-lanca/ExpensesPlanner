using ExpensesPlanner.Data;
using ExpensesPlanner.Extensions;
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
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AccountController(UserManager<User> userManager, SignInManager<User> signInManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _httpContextAccessor = httpContextAccessor;
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

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return View("Error");

            var userDetailsViewModel = new UserDetailsViewModel()
            {
                Id = _httpContextAccessor.HttpContext.User.GetUserID(),
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Phone = user.Phone,
                Address = user.Address,
                City = user.City
            };

            return View(userDetailsViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(UserDetailsViewModel userDetailsViewModel)
        {
            if (!ModelState.IsValid)
            {
                ModelState.AddModelError("", "Failed to edit account profile.");
                return View("Edit", userDetailsViewModel);
            }

            var user = await _userManager.FindByIdAsync(userDetailsViewModel.Id);
            
            if (user == null) return View("Error");

            MapUserEditAccount(user, userDetailsViewModel);
            
            var result = await _userManager.UpdateAsync(user);

            if (result.Succeeded) 
                if(_httpContextAccessor.HttpContext!.User.IsInRole(UserRoles.Admin))
                    return RedirectToAction("Index", "AdminDashboard");
                else
                    return RedirectToAction("Index", "Home");

            else 
                return View("Edit", "Failed to edit account profile after trying to update.");
        }

        private void MapUserEditAccount(User user, UserDetailsViewModel userDetailsVM)
        {
            user.Id = userDetailsVM.Id;
            user.FirstName = userDetailsVM.FirstName;
            user.LastName = userDetailsVM.LastName;
            user.Email = userDetailsVM.Email;
            user.Phone = userDetailsVM.Phone;
            user.Address = userDetailsVM.Address;
            user.City = userDetailsVM.City;
        }
    }
}
