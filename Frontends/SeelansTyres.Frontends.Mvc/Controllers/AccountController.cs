using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Frontends.Mvc.Models;
using SeelansTyres.Frontends.Mvc.Models.External;
using SeelansTyres.Frontends.Mvc.Services;
using SeelansTyres.Frontends.Mvc.ViewModels;
using System.Security.Cryptography;

namespace SeelansTyres.Frontends.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly IAddressService addressService;
    private readonly ICustomerService customerService;
    private readonly IOrderService orderService;
    private readonly IEmailService emailService;

    public AccountController(
        ILogger<AccountController> logger,
        IAddressService addressService,
        ICustomerService customerService,
        IOrderService orderService,
        IEmailService emailService)
    {
        this.logger = logger;
        this.addressService = addressService;
        this.customerService = customerService;
        this.orderService = orderService;
        this.emailService = emailService;
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        var customer = customerService.RetrieveSingleAsync(customerId);
        var addresses = addressService.RetrieveAllAsync(customerId);
        var orders = orderService.RetrieveAllAsync(customerId: customerId);

        await Task.WhenAll(customer, addresses, orders);

        var accountViewModel = new AccountViewModel
        {
            Customer = customer.Result,
            Addresses = addresses.Result!,
            Orders = orders.Result!
        };

        return View(accountViewModel);
    }

    [Authorize]
    public IActionResult Login() =>
        RedirectToAction("Index", "Home");

    public async Task Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
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
            var (newCustomer, succeeded, errors) = await customerService.CreateAsync(model);

            if (succeeded is true && newCustomer is not null)
            {
                return RedirectToAction(nameof(Login));
            }
            else
            {
                errors.ForEach(error => ModelState.AddModelError(string.Empty, error));
            }
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccount(AccountViewModel model)
    {
        await customerService.UpdateAsync(model.UpdateAccountModel);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount(string password)
    {
        var succeeded = await customerService.DeleteAsync(password);

        if (succeeded is true)
        {
            return RedirectToAction(nameof(Logout));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddNewAddress(AccountViewModel model)
    {
        var addressModel = model.AddressModel;

        addressModel.Id = Guid.Empty;

        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        var requestSucceeded = await addressService.CreateAsync(addressModel, customerId);

        if (requestSucceeded is false)
        {
            ModelState.AddModelError(string.Empty, "API is unavailable to add your address,\nplease try again later");
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAddressAsPreferred(Guid addressId)
    {
        var customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);

        _ = await addressService.MarkAddressAsPreferredAsync(customerId, addressId);

        return RedirectToAction(nameof(Index));
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
            var customer = await customerService.RetrieveSingleAsync(model.SendCodeModel.Email);

            if (customer is null)
            {
                ModelState.AddModelError(string.Empty, $"Customer with email {model.SendCodeModel.Email} does not exist!");
                return View(model);
            }

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256));

            HttpContext.Session.SetString("ResetPasswordToken", token);

            await emailService.SendResetPasswordTokenAsync(
                customerEmail: model.SendCodeModel.Email,
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
            var customer = await customerService.RetrieveSingleAsync(model.ResetPasswordModel.Email);

            if (model.ResetPasswordModel.Token != HttpContext.Session.GetString("ResetPasswordToken"))
            {
                ModelState.AddModelError(string.Empty, "Invalid token!");

                return View(model);
            }

            HttpContext.Session.Remove("ResetPasswordToken");

            await customerService.ResetPasswordAsync(customer!.Id, model.ResetPasswordModel.Password);

            return RedirectToAction(nameof(Login));
        }

        return View(model);
    }
}
