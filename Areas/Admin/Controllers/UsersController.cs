using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppConstants.AdminRole)]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UsersController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
    {
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        return View(new Janus.ViewModels.Admin.Users.IndexModel(_userManager)
        {
            Users = await _userManager.Users.OrderBy(u => u.LastName).ToListAsync()
        });
    }

    public async Task<IActionResult> Details(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        return user == null ? NotFound() : View(new Janus.ViewModels.Admin.Users.DetailsModel(_userManager) { ManagedUser = user });
    }

    public async Task<IActionResult> Edit(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        var model = new Janus.ViewModels.Admin.Users.EditModel(_userManager)
        {
            Input = new Janus.ViewModels.Admin.Users.EditModel.InputModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                City = user.City,
                GradeLevel = user.GradeLevel,
                CareerInterest = user.CareerInterest,
                Company = user.Company,
                Industry = user.Industry,
                Bio = user.Bio,
                IsStudent = await _userManager.IsInRoleAsync(user, AppConstants.StudentRole),
                IsHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole),
                IsAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole),
                IsActive = user.IsActive
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "Input")] Janus.ViewModels.Admin.Users.EditModel.InputModel input)
    {
        var user = await _userManager.FindByIdAsync(input.Id);
        if (user == null) return NotFound();

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Admin.Users.EditModel(_userManager) { Input = input });
        }

        user.FirstName = input.FirstName;
        user.LastName = input.LastName;
        user.City = input.City;
        user.GradeLevel = input.GradeLevel;
        user.CareerInterest = input.CareerInterest;
        user.Company = input.Company;
        user.Industry = input.Industry;
        user.Bio = input.Bio;
        user.IsStudent = input.IsStudent;
        user.IsHost = input.IsHost;
        user.IsAdmin = input.IsAdmin;
        user.IsActive = input.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        await _userManager.UpdateAsync(user);
        await SetRoleAsync(user, AppConstants.StudentRole, input.IsStudent);
        await SetRoleAsync(user, AppConstants.HostRole, input.IsHost);
        await SetRoleAsync(user, AppConstants.AdminRole, input.IsAdmin);

        TempData["Success"] = "User updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null) return NotFound();

        return View(new Janus.ViewModels.Admin.Users.DeleteModel(_userManager, _context)
        {
            ManagedUser = user,
            UserId = id
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return NotFound();

        user.IsActive = false;
        user.UpdatedAt = DateTime.UtcNow;

        var opportunities = await _context.Opportunities.Where(o => o.HostUserId == user.Id && o.IsActive).ToListAsync();
        foreach (var opportunity in opportunities)
        {
            opportunity.IsOpen = false;
            opportunity.UpdatedAt = DateTime.UtcNow;
        }

        await _userManager.UpdateAsync(user);
        await _context.SaveChangesAsync();

        TempData["Success"] = "User deactivated.";
        return RedirectToAction(nameof(Index));
    }

    private async Task SetRoleAsync(ApplicationUser user, string role, bool shouldHaveRole)
    {
        var hasRole = await _userManager.IsInRoleAsync(user, role);

        if (shouldHaveRole && !hasRole)
        {
            await _userManager.AddToRoleAsync(user, role);
        }
        else if (!shouldHaveRole && hasRole)
        {
            await _userManager.RemoveFromRoleAsync(user, role);
        }
    }
}
