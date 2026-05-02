using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

[Authorize]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _context = context;
    }

    public async Task<IActionResult> MyProfile()
    {
        var model = new Janus.ViewModels.Users.MyProfileModel(_userManager)
        {
            CurrentUser = await _userManager.GetUserAsync(User)
        };

        return View(model);
    }

    public async Task<IActionResult> CreateStudentProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        var model = new Janus.ViewModels.Users.CreateStudentProfileModel(_userManager)
        {
            AlreadyStudent = await _userManager.IsInRoleAsync(user, AppConstants.StudentRole),
            Input = new Janus.ViewModels.Users.CreateStudentProfileModel.InputModel
            {
                City = user.City ?? string.Empty,
                GradeLevel = user.GradeLevel ?? string.Empty,
                CareerInterest = user.CareerInterest ?? string.Empty
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateStudentProfile([Bind(Prefix = "Input")] Janus.ViewModels.Users.CreateStudentProfileModel.InputModel input)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Users.CreateStudentProfileModel(_userManager)
            {
                AlreadyStudent = await _userManager.IsInRoleAsync(user, AppConstants.StudentRole),
                Input = input
            });
        }

        user.IsStudent = true;
        user.City = input.City;
        user.GradeLevel = input.GradeLevel;
        user.CareerInterest = input.CareerInterest;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        if (!await _userManager.IsInRoleAsync(user, AppConstants.StudentRole))
        {
            await _userManager.AddToRoleAsync(user, AppConstants.StudentRole);
        }
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Student profile enabled.";
        return RedirectToAction(nameof(MyProfile));
    }

    public async Task<IActionResult> CreateHostProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        var model = new Janus.ViewModels.Users.CreateHostProfileModel(_userManager)
        {
            AlreadyHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole),
            Input = new Janus.ViewModels.Users.CreateHostProfileModel.InputModel
            {
                City = user.City ?? string.Empty,
                Company = user.Company ?? string.Empty,
                Industry = user.Industry ?? string.Empty,
                Bio = user.Bio
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateHostProfile([Bind(Prefix = "Input")] Janus.ViewModels.Users.CreateHostProfileModel.InputModel input)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Users.CreateHostProfileModel(_userManager)
            {
                AlreadyHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole),
                Input = input
            });
        }

        user.IsHost = true;
        user.City = input.City;
        user.Company = input.Company;
        user.Industry = input.Industry;
        user.Bio = input.Bio;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        if (!await _userManager.IsInRoleAsync(user, AppConstants.HostRole))
        {
            await _userManager.AddToRoleAsync(user, AppConstants.HostRole);
        }
        await _signInManager.RefreshSignInAsync(user);

        TempData["Success"] = "Host profile enabled.";
        return RedirectToAction(nameof(MyProfile));
    }

    public async Task<IActionResult> EditProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        var isStudent = await _userManager.IsInRoleAsync(user, AppConstants.StudentRole);
        var isHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole);

        var model = new Janus.ViewModels.Users.EditProfileModel(_userManager)
        {
            IsStudent = isStudent,
            IsHost = isHost,
            Input = new Janus.ViewModels.Users.EditProfileModel.InputModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                City = user.City ?? string.Empty,
                GradeLevel = user.GradeLevel,
                CareerInterest = user.CareerInterest,
                Company = user.Company,
                Industry = user.Industry,
                Bio = user.Bio
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile([Bind(Prefix = "Input")] Janus.ViewModels.Users.EditProfileModel.InputModel input)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return NotFound();
        }

        var isStudent = await _userManager.IsInRoleAsync(user, AppConstants.StudentRole);
        var isHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole);

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Users.EditProfileModel(_userManager)
            {
                IsStudent = isStudent,
                IsHost = isHost,
                Input = input
            });
        }

        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        user.PhoneNumber = input.PhoneNumber;
        user.City = input.City;

        if (isStudent)
        {
            user.GradeLevel = input.GradeLevel;
            user.CareerInterest = input.CareerInterest;
        }

        if (isHost)
        {
            user.Company = input.Company;
            user.Industry = input.Industry;
            user.Bio = input.Bio;
        }

        user.UpdatedAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        TempData["Success"] = "Profile updated.";
        return RedirectToAction(nameof(MyProfile));
    }

    public IActionResult DeleteAccount()
    {
        return View(new Janus.ViewModels.Users.DeleteAccountModel(_userManager, _signInManager, _context));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [ActionName("DeleteAccount")]
    public async Task<IActionResult> DeleteAccountConfirmed()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return NotFound();
        }

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var opportunities = await _context.Opportunities
            .Where(o => o.HostUserId == user.Id && o.IsActive)
            .ToListAsync();

        foreach (var opportunity in opportunities)
        {
            opportunity.IsOpen = false;
            opportunity.UpdatedAt = DateTime.UtcNow;
        }

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();
        await _signInManager.SignOutAsync();

        TempData["Success"] = "Your account has been deleted.";
        return RedirectToAction("Index", "Home");
    }
}
