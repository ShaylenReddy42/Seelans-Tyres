using Microsoft.AspNetCore.Authentication;               // SignOutAsync()
using Microsoft.AspNetCore.Authentication.Cookies;       // CookieAuthenticationDefaults
using Microsoft.AspNetCore.Authentication.OpenIdConnect; // OpenIdConnectDefaults
using Microsoft.AspNetCore.Authorization;                // Authorize
using SeelansTyres.Frontends.Mvc.HttpClients;            // IAddressServiceClient, ICustomerServiceClient, IOrderServiceClient
using SeelansTyres.Frontends.Mvc.Models;                 // ResetPasswordModel
using SeelansTyres.Frontends.Mvc.Services;               // IMailService
using SeelansTyres.Frontends.Mvc.ViewModels;             // AccountViewModel, ResetPasswordViewModel
using System.Security.Cryptography;                      // RandomNumberGenerator

namespace SeelansTyres.Frontends.Mvc.Controllers;

public class AccountController : Controller
{
    private readonly ILogger<AccountController> logger;
    private readonly IAddressServiceClient addressServiceClient;
    private readonly ICustomerServiceClient customerServiceClient;
    private readonly IOrderServiceClient orderServiceClient;
    private readonly IMailService mailService;

    private readonly Stopwatch stopwatch;
    private readonly Guid customerId; 

    public AccountController(
        ILogger<AccountController> logger,
        IAddressServiceClient addressServiceClient,
        ICustomerServiceClient customerServiceClient,
        IOrderServiceClient orderServiceClient,
        IMailService mailService)
    {
        this.logger = logger;
        this.addressServiceClient = addressServiceClient;
        this.customerServiceClient = customerServiceClient;
        this.orderServiceClient = orderServiceClient;
        this.mailService = mailService;

        stopwatch = new();
        customerId = Guid.Parse(User.Claims.Single(claim => claim.Type.EndsWith("nameidentifier")).Value);
    }

    [Authorize]
    public async Task<IActionResult> Index()
    {
        stopwatch.Start();
        
        logger.LogInformation(
            "Controller => Retrieving customer details, addresses and orders for customer {CustomerId}",
            customerId);

        var customer = customerServiceClient.RetrieveSingleAsync(customerId);

        var addresses = addressServiceClient.RetrieveAllAsync(customerId);

        var orders = orderServiceClient.RetrieveAllAsync(customerId: customerId);

        await Task.WhenAll(customer, addresses, orders);

        var accountViewModel = new AccountViewModel
        {
            Customer = customer.Result,
            Addresses = addresses.Result!,
            Orders = orders.Result!
        };

        stopwatch.Stop();

        logger.LogInformation(
            "Building the Account View Model for customer {CustomerId} took {StopwatchElapsedTime}ms to complete",
            customerId, stopwatch.ElapsedMilliseconds);

        return View(accountViewModel);
    }

    [Authorize]
    public IActionResult Login() =>
        RedirectToAction("Index", "Home");

    public async Task Logout()
    {
        var isAdmin = User.IsInRole("Administrator");

        if (isAdmin)
        {
            logger.LogInformation("Logging out the administrator");
        }
        else
        {
            logger.LogInformation(
                "Logging out customer {CustomerId}",
                customerId);
        }

        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
    }

    public IActionResult Register()
    {
        if (User.Identity!.IsAuthenticated)
        {
            logger.LogInformation("An authenticated user tried to access the register view. Redirecting them");
            
            return RedirectToAction("Index", "Home");
        }

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Register(RegisterModel model)
    {
        if (!ModelState.IsValid)
        {
            return View();
        }

        logger.LogInformation("Controller => Attempting to create a new customer account");

        var (newCustomer, succeeded, errors) = await customerServiceClient.CreateAsync(model);

        if (succeeded && newCustomer is not null)
        {
            logger.LogInformation(
                "{Announcement}: Attempt to create a new customer account completed successfully",
                "SUCCEEDED");

            return RedirectToAction(nameof(Login));
        }

        logger.LogWarning(
            "{Announcement}: Attempt to create a new customer account was unsuccessful",
            "FAILED");

        errors.ForEach(error => ModelState.AddModelError(string.Empty, error));

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateAccount(AccountViewModel model)
    {
        logger.LogInformation(
            "Controller => Attempting to update account for customer {CustomerId}. Encryption required",
            customerId);
        
        await customerServiceClient.UpdateAsync(model.UpdateAccountModel);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAccount(string password)
    {
        logger.LogInformation(
            "Controller => Attempting to delete account for customer {CustomerId}. Encryption required",
            customerId);

        var succeeded = await customerServiceClient.DeleteAsync(password);

        if (succeeded)
        {
            logger.LogInformation(
                "{Announcement}: Attempt to delete account for customer {CustomerId} completed successfully. Logging them out",
                "SUCCEEDED", customerId);

            return RedirectToAction(nameof(Logout));
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> AddNewAddress(AccountViewModel model)
    {
        logger.LogInformation(
            "Controller => Attempting to add a new address for customer {CustomerId}",
            customerId);

        var addressModel = model.AddressModel;

        addressModel.Id = Guid.Empty;

        var requestSucceeded = await addressServiceClient.CreateAsync(addressModel, customerId);

        if (!requestSucceeded)
        {
            logger.LogError(
                "{Announcement}: Attempt to add a new address for customer {CustomerId} was unsuccessful",
                "FAILED", customerId);
            
            ModelState.AddModelError(string.Empty, "API is unavailable to add your address,\nplease try again later");
        }
        else
        {
            logger.LogInformation(
                "{Announcement}: Attempt to add a new address for customer {CustomerId} completed successfully",
                "SUCCEEDED", customerId);
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> MarkAddressAsPreferred(Guid addressId)
    {
        logger.LogInformation(
            "Controller => Attempting to mark address {AddressId} for customer {CustomerId} as preferred",
            addressId, customerId);

        await addressServiceClient.MarkAddressAsPreferredAsync(customerId, addressId);

        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> DeleteAddress(Guid addressId)
    {
        logger.LogInformation(
            "Controller => Attempting to delete address {AddressId} for customer {CustomerId}",
            addressId, customerId);

        await addressServiceClient.DeleteAsync(customerId, addressId);

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
            logger.LogInformation(
                "Controller => Starting a reset password operation for customer with email {CustomerEmail}",
                "***REDACTED***");
            
            var customer = await customerServiceClient.RetrieveSingleAsync(model.SendCodeModel.Email);

            if (customer is null)
            {
                logger.LogWarning(
                    "Customer with email {CustomerEmail} does not exist!",
                    "***REDACTED***");
                
                ModelState.AddModelError(string.Empty, $"Customer with email {model.SendCodeModel.Email} does not exist!");

                return View(model);
            }

            logger.LogDebug("Randomly generating a reset password token of size 256 bytes");

            string token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(256));

            HttpContext.Session.SetString("ResetPasswordToken", token);

            logger.LogInformation(
                "Sending token to customer with email {CustomerEmail}",
                "***REDACTED***");

             var result = 
                await mailService.SendResetPasswordTokenAsync(
                    customerEmail: model.SendCodeModel.Email,
                    firstName: customer.FirstName,
                    lastName: customer.LastName,
                    token: token);

            if (!result)
            {
                logger.LogError(
                    "{Announcement}: The system failed to send the token to customer with email {CustomerEmail}",
                    "FAILED", "***REDACTED***");
                
                ModelState.AddModelError(string.Empty, "The system failed to send you an email with the token,\nplease resubmit and try again");

                return View(model);
            }

            model.ResetPasswordModel = new ResetPasswordModel
            {
                Email = model.SendCodeModel.Email
            };

            return View(model);
        }
        else if (model.ResetPasswordModel is not null)
        {
            var customer = await customerServiceClient.RetrieveSingleAsync(model.ResetPasswordModel.Email);

            if (model.ResetPasswordModel.Token != HttpContext.Session.GetString("ResetPasswordToken"))
            {
                logger.LogError(
                    "{Announcement}: Customer with email {CustomerEmail} entered an invalid token to try and reset their password",
                    "FAILED", "***REDACTED***");
                
                ModelState.AddModelError(string.Empty, "Invalid token!");

                return View(model);
            }

            logger.LogError(
                "{Announcement}: Customer with email {CustomerEmail} entered a valid token. The reset password operation will begin",
                "SUCCEEDED", "***REDACTED***");

            HttpContext.Session.Remove("ResetPasswordToken");

            await customerServiceClient.ResetPasswordAsync(customer!.Id, model.ResetPasswordModel.Password);

            return RedirectToAction(nameof(Login));
        }

        return View(model);
    }
}
