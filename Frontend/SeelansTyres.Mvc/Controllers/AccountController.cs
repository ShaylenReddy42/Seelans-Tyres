﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SeelansTyres.Mvc.Data.Entities;
using SeelansTyres.Mvc.Models;
using SeelansTyres.Mvc.Services;
using SeelansTyres.Mvc.ViewModels;

namespace SeelansTyres.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly SignInManager<Customer> signInManager;
    private readonly UserManager<Customer> userManager;
    private readonly IAddressService addressService;
    private readonly ICustomerService customerService;
    private readonly IOrderService orderService;
    private readonly IEmailService emailService;
    private readonly ITokenService tokenService;

    public AccountController(
        ILogger<AccountController> logger,
        SignInManager<Customer> signInManager,
        UserManager<Customer> userManager,
        IAddressService addressService,
        ICustomerService customerService,
        IOrderService orderService,
        IEmailService emailService,
        ITokenService tokenService)
    {
        this.logger = logger;
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.addressService = addressService;
        this.customerService = customerService;
        this.orderService = orderService;
        this.emailService = emailService;
        this.tokenService = tokenService;
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

        var addresses = addressService.RetrieveAllAsync(customer.Id);
        var orders = orderService.RetrieveAllAsync(customerId: customer.Id);

        await Task.WhenAll(addresses, orders);

        var accountViewModel = new AccountViewModel
        {
            Customer = customerModel,
            Addresses = addresses.Result!,
            Orders = orders.Result!
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
                    var customer = await userManager.FindByEmailAsync(model.UserName);

                    tokenService.GenerateApiAuthToken(customer, await userManager.IsInRoleAsync(customer, "Administrator"));

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
            var (newCustomer, succeeded, errors) = await customerService.CreateAsync(model);

            if (succeeded is true && newCustomer is not null)
            {
                await signInManager.SignInAsync(newCustomer, isPersistent: false);

                tokenService.GenerateApiAuthToken(newCustomer, await userManager.IsInRoleAsync(newCustomer, "Administrator"));

                return RedirectToAction("Index", "Home");
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

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount(string password)
    {
        var succeeded = await customerService.DeleteAsync(password);

        if (succeeded is true)
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        return RedirectToAction("Index");
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

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkAddressAsPreferred(Guid addressId)
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