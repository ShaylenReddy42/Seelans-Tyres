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
    private readonly IEmailService emailService;
    private readonly HttpClient client;

    public AccountController(
        ILogger<AccountController> logger,
        SignInManager<Customer> signInManager,
        UserManager<Customer> userManager,
        IHttpClientFactory httpClientFactory,
        IEmailService emailService)
    {
        this.logger = logger;
        this.signInManager = signInManager;
        this.userManager = userManager;
        this.emailService = emailService;
        client = httpClientFactory.CreateClient("SeelansTyresAPI");
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

        HttpRequestMessage request = null!;
        HttpResponseMessage response = null!;

        IEnumerable<AddressModel>? addresses = new List<AddressModel>();
        IEnumerable<OrderModel>? orders = new List<OrderModel>();

        try
        {
            request = new HttpRequestMessage(HttpMethod.Get, $"api/customers/{customer.Id}/addresses");
            request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");

            response = await client.SendAsync(request);

            addresses = await response.Content.ReadFromJsonAsync<IEnumerable<AddressModel>>();

            request = new HttpRequestMessage(HttpMethod.Get, $"api/orders?customerId={customer.Id}");
            request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");

            response = await client.SendAsync(request);

            orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderModel>>();
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

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
    public async Task<IActionResult> Login(LoginModel model, string? returnUrl = null)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var result = await signInManager.PasswordSignInAsync(model.UserName, model.Password, model.RememberMe, false);

                if (result.Succeeded)
                {
                    var response = await client.PostAsync("api/authentication/login", JsonContent.Create(model));

                    if (response.IsSuccessStatusCode)
                    {
                        var token = await response.Content.ReadFromJsonAsync<string>();
                        HttpContext.Session.SetString("ApiAuthToken", token!);
                    }
                    
                    return Redirect(returnUrl ?? "~/");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Login attempt failed!");
                }
            }
            catch (InvalidOperationException ex)
            {
                logger.LogError(ex.Message);
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

                        var response = await client.PostAsync(
                            "api/authentication/login",
                            JsonContent.Create(
                                new LoginModel
                                {
                                    UserName = model.Email,
                                    Password = model.Password
                                }));

                        if (response.IsSuccessStatusCode)
                        {
                            var token = await response.Content.ReadFromJsonAsync<string>();
                            HttpContext.Session.SetString("ApiAuthToken", token!);
                        }

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
                logger.LogError(ex.Message);
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
        var createAddressModel = model.CreateAddressModel;

        var customerId = (await userManager.GetUserAsync(User)).Id;

        var jsonContent = JsonContent.Create(createAddressModel);

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Post, $"api/customers/{customerId}/addresses");
            request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");
            request.Content = jsonContent;

            await client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
            ModelState.AddModelError(string.Empty, "API is unavailable to add your address,\nplease try again later");
        }

        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> MarkAddressAsPreferred(int addressId)
    {
        var customerId = (await userManager.GetUserAsync(User)).Id;

        try
        {
            var request = new HttpRequestMessage(HttpMethod.Put, $"api/customers/{customerId}/addresses/{addressId}?markAsPreferred=true");
            request.Headers.Add("Authorization", $"Bearer {HttpContext.Session.GetString("ApiAuthToken")}");

            await client.SendAsync(request);
        }
        catch (HttpRequestException ex)
        {
            logger.LogError(ex.Message);
        }

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
