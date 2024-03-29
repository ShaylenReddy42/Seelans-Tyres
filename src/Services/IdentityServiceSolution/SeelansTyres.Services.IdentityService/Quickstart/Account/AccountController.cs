// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityModel;
using IdentityServer4;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using IdentityServer4.Stores;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using SeelansTyres.Services.IdentityService.Data.Entities;

namespace IdentityServerHost.Quickstart.UI;

/// <summary>
/// This sample controller implements a typical login/logout/provision workflow for local and external accounts.
/// The login service encapsulates the interactions with the user data store. This data store is in-memory only and cannot be used for production!
/// The interaction service provides a way for the UI to communicate with identityserver for validation and context retrieval
/// </summary>
[SecurityHeaders]
[AllowAnonymous]
public class AccountController : Controller
{
    private readonly IIdentityServerInteractionService _interaction;
    private readonly IClientStore _clientStore;
    private readonly IAuthenticationSchemeProvider _schemeProvider;
    private readonly IEventService _events;
    private readonly SignInManager<Customer> _signInManager;

    public AccountController(
        IIdentityServerInteractionService interaction,
        IClientStore clientStore,
        IAuthenticationSchemeProvider schemeProvider,
        IEventService events,
        SignInManager<Customer> signInManager)
    {
        // if the TestUserStore is not in DI, then we'll just use the global users collection
        // this is where you would plug in your own custom identity management library (e.g. ASP.NET Identity)
        _interaction = interaction;
        _clientStore = clientStore;
        _schemeProvider = schemeProvider;
        _events = events;
        _signInManager = signInManager;
    }

    /// <summary>
    /// Entry point into the login workflow
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Login(string returnUrl)
    {
        // build a model so we know what to show on the login page
        var vm = await BuildLoginViewModelAsync(returnUrl);

        if (vm.IsExternalLoginOnly)
        {
            // we only have one option for logging in and it's an external provider
            return RedirectToAction("Challenge", "External", new { scheme = vm.ExternalLoginScheme, returnUrl });
        }

        return View(vm);
    }

    /// <summary>
    /// Handle postback from username/password login
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginInputModel model)
    {
        // check if we are in the context of an authorization request
        var context = await _interaction.GetAuthorizationContextAsync(model.ReturnUrl);

        if (!ModelState.IsValid)
        {
            // something went wrong, show form with error
            return View(await BuildLoginViewModelAsync(model));
        }

        var customer = await _signInManager.UserManager.FindByEmailAsync(model.Username);

        if (!await _signInManager.UserManager.CheckPasswordAsync(customer, model.Password))
        {
            await _events.RaiseAsync(new UserLoginFailureEvent(model.Username, "invalid credentials", clientId: context?.Client.ClientId));
            ModelState.AddModelError(string.Empty, AccountOptions.InvalidCredentialsErrorMessage);

            // something went wrong, show form with error
            return View(await BuildLoginViewModelAsync(model));
        }

        await _events.RaiseAsync(new UserLoginSuccessEvent(customer.UserName, customer.Id.ToString(), customer.UserName, clientId: context?.Client.ClientId));

        // only set explicit expiration here if user chooses "remember me". 
        // otherwise we rely upon expiration configured in cookie middleware.
        AuthenticationProperties? props = null;
        if (AccountOptions.AllowRememberLogin && model.RememberLogin)
        {
            props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTimeOffset.UtcNow.Add(AccountOptions.RememberMeLoginDuration)
            };
        }

        // issue authentication cookie with subject ID and username
        var isuser = new IdentityServerUser(customer.Id.ToString())
        {
            DisplayName = $"{customer.FirstName} {customer.LastName}"
        };

        await HttpContext.SignInAsync(isuser, props);

        if (context is not null)
        {
            // we can trust model.ReturnUrl since GetAuthorizationContextAsync returned non-null

            // The client is native, so this change in how to
            // return the response is for better UX for the end user.
            return context.IsNativeClient()
                ? this.LoadingPage("Redirect", model.ReturnUrl)
                : Redirect(model.ReturnUrl);
        }

        // request for a local page
        if (Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }
        else if (string.IsNullOrEmpty(model.ReturnUrl))
        {
            return Redirect("~/");
        }
        else
        {
            // user might have clicked on a malicious link - should be logged
            throw new ArgumentException("invalid return URL");
        }
    }

    
    /// <summary>
    /// Show logout page
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> Logout(string logoutId)
    {
        // build a model so the logout page knows what to display
        var vm = await BuildLogoutViewModelAsync(logoutId);

        if (!vm.ShowLogoutPrompt)
        {
            // if the request for logout was properly authenticated from IdentityServer, then
            // we don't need to show the prompt and can just log the user out directly.
            return await Logout(vm);
        }

        return View(vm);
    }

    /// <summary>
    /// Handle logout page postback
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout(LogoutInputModel model)
    {
        // build a model so the logged out page knows what to display
        var vm = await BuildLoggedOutViewModelAsync(model.LogoutId);

        if (User.Identity!.IsAuthenticated)
        {
            // delete local authentication cookie
            await _signInManager.SignOutAsync();

            // raise the logout event
            await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
        }

        // check if we need to trigger sign-out at an upstream identity provider
        if (vm.TriggerExternalSignout)
        {
            // build a return URL so the upstream provider will redirect back
            // to us after the user has logged out. this allows us to then
            // complete our single sign-out processing.
            string url = Url.Action("Logout", new { logoutId = vm.LogoutId })!;

            // this triggers a redirect to the external provider for sign-out
            return SignOut(new AuthenticationProperties { RedirectUri = url }, vm.ExternalAuthenticationScheme!);
        }

        return View("LoggedOut", vm);
    }

    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }


    /*****************************************
     * helper APIs for the AccountController *
     *****************************************/
    private async Task<LoginViewModel> BuildLoginViewModelAsync(string returnUrl)
    {
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        if (context?.IdP != null && await _schemeProvider.GetSchemeAsync(context.IdP) != null)
        {
            var local = context.IdP == IdentityServer4.IdentityServerConstants.LocalIdentityProvider;

            // this is meant to short circuit the UI and only trigger the one external IdP
            var vm = new LoginViewModel
            {
                EnableLocalLogin = local,
                ReturnUrl = returnUrl,
                Username = context.LoginHint,
            };

            if (!local)
            {
                vm.ExternalProviders = new[] { new ExternalProvider { AuthenticationScheme = context!.IdP } };
            }

            return vm;
        }

        var schemes = await _schemeProvider.GetAllSchemesAsync();

        var providers = schemes
            .Where(x => x.DisplayName != null)
            .Select(x => new ExternalProvider
            {
                DisplayName = x.DisplayName ?? x.Name,
                AuthenticationScheme = x.Name
            }).ToList();

        var allowLocal = true;
        if (context?.Client.ClientId != null)
        {
            var client = await _clientStore.FindEnabledClientByIdAsync(context.Client.ClientId);
            if (client != null)
            {
                allowLocal = client.EnableLocalLogin;

                if (client.IdentityProviderRestrictions != null && client.IdentityProviderRestrictions.Any())
                {
                    providers = providers.Where(provider => client.IdentityProviderRestrictions.Contains(provider.AuthenticationScheme)).ToList();
                }
            }
        }

        return new LoginViewModel
        {
            AllowRememberLogin = AccountOptions.AllowRememberLogin,
            EnableLocalLogin = allowLocal && AccountOptions.AllowLocalLogin,
            ReturnUrl = returnUrl,
            Username = context?.LoginHint!,
            ExternalProviders = providers.ToArray()
        };
    }

    private async Task<LoginViewModel> BuildLoginViewModelAsync(LoginInputModel model)
    {
        var vm = await BuildLoginViewModelAsync(model.ReturnUrl);
        vm.Username = model.Username;
        vm.RememberLogin = model.RememberLogin;
        return vm;
    }

    private async Task<LogoutViewModel> BuildLogoutViewModelAsync(string logoutId)
    {
        var vm = new LogoutViewModel { LogoutId = logoutId, ShowLogoutPrompt = AccountOptions.ShowLogoutPrompt };

        if (User?.Identity!.IsAuthenticated != true)
        {
            // if the user is not authenticated, then just show logged out page
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        var context = await _interaction.GetLogoutContextAsync(logoutId);
        if (context?.ShowSignoutPrompt == false)
        {
            // it's safe to automatically sign-out
            vm.ShowLogoutPrompt = false;
            return vm;
        }

        // show the logout prompt. this prevents attacks where the user
        // is automatically signed out by another malicious web page.
        return vm;
    }

    private async Task<LoggedOutViewModel> BuildLoggedOutViewModelAsync(string logoutId)
    {
        // get context information (client name, post logout redirect URI and iframe for federated signout)
        var logout = await _interaction.GetLogoutContextAsync(logoutId);

        var vm = new LoggedOutViewModel
        {
            AutomaticRedirectAfterSignOut = AccountOptions.AutomaticRedirectAfterSignOut,
            PostLogoutRedirectUri = logout?.PostLogoutRedirectUri!,
            ClientName = logout?.ClientName ?? logout?.ClientId!,
            SignOutIframeUrl = logout?.SignOutIFrameUrl!,
            LogoutId = logoutId
        };

        if (User?.Identity!.IsAuthenticated == true)
        {
            var idp = User.FindFirst(JwtClaimTypes.IdentityProvider)?.Value;
            if (idp != null && idp != IdentityServer4.IdentityServerConstants.LocalIdentityProvider)
            {
                var providerSupportsSignout = await HttpContext.GetSchemeSupportsSignOutAsync(idp);
                if (providerSupportsSignout)
                {
                    // if there's no current logout context, we need to create one
                    // this captures necessary info from the current logged in user
                    // before we signout and redirect away to the external IdP for signout
                    vm.LogoutId ??= await _interaction.CreateLogoutContextAsync();

                    vm.ExternalAuthenticationScheme = idp;
                }
            }
        }

        return vm;
    }
}
