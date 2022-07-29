using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Entities;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.ViewModels;

namespace SeelansTyres.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly SignInManager<Customer> signInManager;
    private readonly UserManager<Customer> userManager;

    public AccountController(
        ILogger<AccountController> logger,
        SignInManager<Customer> signInManager,
        UserManager<Customer> userManager)
    {
        this.logger = logger;
        this.signInManager = signInManager;
        this.userManager = userManager;
    }
    
    [Authorize]
    public IActionResult Index()
    {
        AccountViewModel accountViewModel = new();
        
        return View(accountViewModel);
    }

    public IActionResult Login()
    {
        ViewData["Title"] = "Login";
        
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
    {
        ViewData["Title"] = "Login";

        if (ModelState.IsValid)
        {
            var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

            if (result.Succeeded)
            {
                return Redirect(returnUrl ?? "~/");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Login attempt failed!");
            }
        }

        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        ViewData["Title"] = "Register";

        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        ViewData["Title"] = "Register";

        if (ModelState.IsValid)
        {
            var user = await userManager.FindByEmailAsync(model.Email);

            if (user is not null)
            {
                ModelState.AddModelError(string.Empty, $"Customer with email {model.Email} already exists");
            }
            else
            {
                var newCustomer = new Customer()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    UserName = model.Email,
                    PhoneNumber = model.PhoneNumber
                };

                var result = await userManager.CreateAsync(newCustomer, model.Password);

                if (result.Succeeded)
                {
                    await signInManager.SignInAsync(newCustomer, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccount(AccountViewModel model)
    {
        var updateAccountModel = model.updateAccountModel;
        
        if (TryValidateModel(updateAccountModel))
        {
            var user = await userManager.GetUserAsync(User);

            user.FirstName = updateAccountModel.FirstName;
            user.LastName = updateAccountModel.LastName;
            user.PhoneNumber = updateAccountModel.PhoneNumber;

            await userManager.UpdateAsync(user);
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount(string password)
    {
        var user = await userManager.GetUserAsync(User);
        
        var passwordCorrect = await userManager.CheckPasswordAsync(user, password);

        if (passwordCorrect is true)
        {
            await signInManager.SignOutAsync();
            await userManager.DeleteAsync(user);
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index");
    }
}
