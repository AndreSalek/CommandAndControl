using DataDashboard.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Diagnostics;
using System.Net;
using System.Text.Encodings.Web;
using System.Text;
using DataDashboard.ViewModels;

namespace DataDashboard.Controllers
{
    public partial class AccountController : Controller
    {
        //Manages IdentityUser authentication
        private readonly SignInManager<IdentityUser> _signInManager;
        /*Manages communication with data access layer of Microsoft Identity tables
         https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity?view=aspnetcore-8.0&tabs=visual-studio
         */
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<AccountController> _logger;

        public AccountController(
             UserManager<IdentityUser> userManager,
             SignInManager<IdentityUser> signInManager,
             ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _logger = logger;
        }
        // GET METHODS
        public IActionResult Login()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Client");
            return View(new LoginViewModel());
        }
        public IActionResult Register()
        {
            if (_signInManager.IsSignedIn(User)) return RedirectToAction("Index", "Client");
            return View(new RegisterViewModel());
        }
        public IActionResult ForgotPassword() => View(new ResetPasswordViewModel());
        public IActionResult Lockout() => View();

        // POST METHODS
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = default)
        {
            returnUrl ??= Url.Content("/Client/Index");

            // Login attempt failed
            if (ModelState == null || !ModelState.IsValid) return View(model);

            IdentityUser? identity = await _userManager.FindByNameAsync(model.UserName);
            Microsoft.AspNetCore.Identity.SignInResult result = await _signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, lockoutOnFailure: true);
            
            if (identity == null)
            {
                // return user not found error
                return View(model);
            }
            if (result.Succeeded)
            {
                await _userManager.ResetAccessFailedCountAsync(identity);
                _logger.LogInformation("User logged in.");
                return LocalRedirect(returnUrl);
            }
            if (result.IsLockedOut)
            {
                _logger.LogWarning("User account locked out.");
                return LocalRedirect("Lockout");
            }
            else
            {
                if (identity != null) await _userManager.AccessFailedAsync(identity);
                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                return View(model);
            }
            
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = default)
        {
            returnUrl ??= Url.Content("/Account/Login");
            if (ModelState.IsValid)
            {
                var user = CreateUser();
                //string hashedPassword = _userManager.PasswordHasher.HashPassword(user, model.Password);
                //Trace.WriteLine("Password: " + hashedPassword);
                await _userManager.SetUserNameAsync(user, model.UserName);
                await _userManager.SetEmailAsync(user, model.Email);
                await _userManager.AddPasswordAsync(user, model.Password);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    _logger.LogInformation("User created a new account with password.");
                    return LocalRedirect(returnUrl);
                }
                else if (result.Errors.Count() > 0) ModelState.AddModelError(string.Empty, "Registration failed. Try again");
            }

            return View(model);
        }


        //TODO: Implement password reset
        [HttpPost]
        public IActionResult ForgotPassword(ResetPasswordViewModel model)
        {
            //Implement password reset
            //_userManager.ResetPasswordAsync();
            return View(new ResetPasswordViewModel());
        }
        [HttpPost]
        public async Task<IActionResult> Logout(string? returnUrl = default)
        {
            await _signInManager.SignOutAsync();
            returnUrl ??= Url.Content("/Account/Login");
            return LocalRedirect(returnUrl);
        }


        private IdentityUser CreateUser()
        {
            try
            {
                return Activator.CreateInstance<IdentityUser>();
            }
            catch
            {
                throw new InvalidOperationException($"Can't create an instance of '{nameof(IdentityUser)}'. " +
                    $"Ensure that '{nameof(IdentityUser)}' is not an abstract class and has a parameterless constructor, or alternatively ");
            }
        }

    }
}
