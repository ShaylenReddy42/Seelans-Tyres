using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Data.Entities;
using SeelansTyres.Data.Models;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Services;
using SeelansTyres.Mvc.ViewModels;

namespace SeelansTyres.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly SignInManager<Customer> signInManager;
    private readonly UserManager<Customer> userManager;
    private readonly IAuthenticationService authenticationService;
    private readonly IAddressService addressService;
    private readonly IOrderService orderService;
    private readonly IEmailService emailService;

    public AccountController(
        ILogger<AccountController> logger,
        SignInManager<Customer> signInManager,
        UserManager<Customer> userManager,
        IAuthenticationService authenticationService,
        IAddressService addressService,
        IOrderService orderService,
        IEmailService emailService)
    {
        this.logger = logger;
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.authenticationService = authenticationService;
        this.addressService = addressService;
        this.orderService = orderService;
        this.emailService = emailService;
    }
    
    [Authorize]
    public async Task<IActionResult> Index()
    {
        var customer = await userManager.GetUserAsync(User);

        var customerModel = new CustomerModel
        {
            Id = customer.Id,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            Email = customer.Email,
            PhoneNumber = customer.PhoneNumber
        };

        var addresses = await addressService.RetrieveAllAsync(customer.Id);
        var orders = await orderService.RetrieveAllAsync(customerId: customer.Id);

        var accountViewModel = new AccountViewModel
        {
            Customer = customerModel,
            Addresses = addresses!,
            Orders = orders!
        };
        
        return View(accountViewModel);
    }

    public IActionResult Login()
    {
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    _ = await authenticationService.LoginAsync(model);

                    return RedirectToAction("Index", "Home");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login attempt failed!");
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The database is unavailable");
                ModelState.AddModelError(string.Empty, "Database is not connected, please try again later");
            }
        }

        return View();
    }

    public async Task<IActionResult> Logout()
    {
        await signInManager.SignOutAsync();
        HttpContext.Session.Remove("ApiAuthToken");

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Register()
    {
        if (User.Identity!.IsAuthenticated)
        {
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (ModelState.IsValid)
        {
            try
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

                        var loginModel = new LoginModel
                        {
                            UserName = model.Email,
                            Password = model.Password
                        };

                        _ = await authenticationService.LoginAsync(loginModel);

                        return RedirectToAction("Index", "Home");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex, "The database is unavailable");
                ModelState.AddModelError(string.Empty, "Database is not connected, please try again later");
            }
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccount(AccountViewModel model)
    {
        var updateAccountModel = model.UpdateAccountModel;
        
        var user = await userManager.GetUserAsync(User);

        user.FirstName = updateAccountModel.FirstName;
        user.LastName = updateAccountModel.LastName;
        user.PhoneNumber = updateAccountModel.PhoneNumber;

        await userManager.UpdateAsync(user);

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

    [HttpPost]
    public async Task<IActionResult> AddNewAddress(AccountViewModel model)
    {
        var addressModel = model.AddressModel;

        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        var requestSucceeded = await addressService.CreateAsync(addressModel, customerId);

        if (requestSucceeded is false)
        {
            ModelState.AddModelError(string.Empty, "API is unavailable to add your address,\nplease try again later");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkAddressAsPreferred(int addressId)
    {
        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        _ = await addressService.MarkAddressAsPreferredAsync(customerId, addressId);

        return RedirectToAction("Index");
    }

    public IActionResult ResetPassword()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
    {
        if (model.SendCodeModel is not null)
        {
            var customer = await userManager.FindByEmailAsync(model.SendCodeModel.Email);

            if (customer is null)
            {
                ModelState.AddModelError(string.Empty, $"Customer with email {model.SendCodeModel.Email} does not exist!");
                return View(model);
            }

            string token = await userManager.GeneratePasswordResetTokenAsync(customer);

            await emailService.SendResetPasswordTokenAsync(
                email: model.SendCodeModel.Email,
                firstName: customer.FirstName,
                lastName: customer.LastName,
                token: token);

            model.ResetPasswordModel = new ResetPasswordModel
            {
                Email = model.SendCodeModel.Email
            };

            return View(model);
        }
        else if (model.ResetPasswordModel is not null)
        {
            var customer = await userManager.FindByEmailAsync(model.ResetPasswordModel.Email);

            var resetPasswordResult =
                await userManager
                    .ResetPasswordAsync(
                        user: customer,
                        token: model.ResetPasswordModel.Token,
                        newPassword: model.ResetPasswordModel.Password);

            if (resetPasswordResult.Succeeded is false)
            {
                foreach (var error in resetPasswordResult.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }

                return View(model);
            }

            await signInManager.PasswordSignInAsync(customer, model.ResetPasswordModel.Password, false, false);

            return RedirectToAction("Index", "Home");
        }

        return View(model);
    }
}
