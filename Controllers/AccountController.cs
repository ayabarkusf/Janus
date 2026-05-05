using Janus.Models;
using Janus.ViewModels.Account;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Janus.Controllers;

// MVC controller responsible for account screens and authentication actions.
public class AccountController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Login(string? returnUrl = null)
    {
        // Clear any external authentication cookie before showing the login form.
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        return View(new LoginViewModel
        {
            ReturnUrl = returnUrl ?? Url.Content("~/")
        });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        model.ReturnUrl ??= Url.Content("~/");

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Identity validates the password hash and creates the authentication cookie.
        var user = await _userManager.FindByEmailAsync(model.Email);
        if (user == null || !user.IsActive)
        {
            ModelState.AddModelError(string.Empty, "No account found with this email or password.");
            return View(model);
        }

        var result = await _signInManager.PasswordSignInAsync(
            user.UserName!,
            model.Password,
            model.RememberMe,
            lockoutOnFailure: false);

        if (result.Succeeded)
        {
            user.UpdatedAt = DateTime.UtcNow;
            await _userManager.UpdateAsync(user);
            return LocalRedirect(model.ReturnUrl);
        }

        ModelState.AddModelError(string.Empty, "No account found with this email or password.");
        return View(model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        return View(new RegisterPageViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterStudent(RegisterPageViewModel model)
    {
        foreach (var key in ModelState.Keys.Where(k => !k.StartsWith("Student.")).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            model.ActiveSection = "student";
            return View("Register", model);
        }

        var s = model.Student;
        var user = new ApplicationUser
        {
            UserName = s.Email,
            Email = s.Email,
            EmailConfirmed = true,
            FirstName = s.FirstName,
            LastName = s.LastName,
            City = s.City,
            GradeLevel = s.GradeLevel,
            CareerInterest = s.CareerInterest,
            IsStudent = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, s.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, AppConstants.StudentRole);
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Student account created.";
            return RedirectToAction("MyProfile", "Users");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        model.ActiveSection = "student";
        return View("Register", model);
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RegisterHost(RegisterPageViewModel model)
    {
        foreach (var key in ModelState.Keys.Where(k => !k.StartsWith("Host.")).ToList())
            ModelState.Remove(key);

        if (!ModelState.IsValid)
        {
            model.ActiveSection = "host";
            return View("Register", model);
        }

        var h = model.Host;
        var user = new ApplicationUser
        {
            UserName = h.Email,
            Email = h.Email,
            EmailConfirmed = true,
            FirstName = h.FirstName,
            LastName = h.LastName,
            City = h.City,
            Company = h.Company,
            Industry = h.Industry,
            Bio = h.Bio,
            IsHost = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, h.Password);
        if (result.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, AppConstants.HostRole);
            await _signInManager.SignInAsync(user, isPersistent: false);
            TempData["Success"] = "Host account created.";
            return RedirectToAction("MyProfile", "Users");
        }

        foreach (var error in result.Errors)
            ModelState.AddModelError(string.Empty, error.Description);

        model.ActiveSection = "host";
        return View("Register", model);
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public IActionResult ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Demo behavior only: in production this would create an Identity token and send an email.
        model.MessageSent = true;
        return View(model);
    }

    [HttpPost]
    [Authorize]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // SignInManager removes the authentication cookie.
        await _signInManager.SignOutAsync();
        TempData["Success"] = "You have signed out.";
        return RedirectToAction("Index", "Home");
    }
}
