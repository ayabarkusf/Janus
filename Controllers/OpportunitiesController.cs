using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

public class OpportunitiesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OpportunitiesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(string? industry, string? city)
    {
        var model = new Janus.ViewModels.Opportunities.IndexModel(_context)
        {
            Industry = industry,
            City = city
        };

        var query = _context.Opportunities
            .Include(o => o.HostUser)
            .Where(o => o.IsActive && o.IsOpen && o.HostUser != null && o.HostUser.IsActive);

        if (!string.IsNullOrWhiteSpace(industry))
        {
            query = query.Where(o => o.Industry == industry);
        }

        if (!string.IsNullOrWhiteSpace(city))
        {
            query = query.Where(o => o.City.Contains(city));
        }

        model.Opportunities = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var model = new Janus.ViewModels.Opportunities.DetailsModel(_context)
        {
            Opportunity = await _context.Opportunities
                .Include(o => o.HostUser)
                .FirstOrDefaultAsync(o => o.Id == id && o.IsActive && o.HostUser != null && o.HostUser.IsActive)
        };

        return model.Opportunity == null ? NotFound() : View(model);
    }

    [Authorize]
    public async Task<IActionResult> MyOpportunities()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        var isHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole);
        if (!isAdmin && !isHost)
        {
            TempData["Error"] = "Create a host profile to manage opportunities.";
            return RedirectToAction("CreateHostProfile", "Users");
        }

        var model = new Janus.ViewModels.Opportunities.MyOpportunitiesModel(_context, _userManager);
        var query = _context.Opportunities
            .Include(o => o.Applications)
            .ThenInclude(a => a.StudentUser)
            .Where(o => o.IsActive);

        if (!isAdmin)
        {
            query = query.Where(o => o.HostUserId == user.Id);
        }

        model.Opportunities = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
        return View(model);
    }

    [Authorize]
    public async Task<IActionResult> Create()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return RedirectToAction("Login", "Account");
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        var isHost = await _userManager.IsInRoleAsync(user, AppConstants.HostRole);
        if (!isAdmin && !isHost)
        {
            TempData["Error"] = "Create a host profile before creating opportunities.";
            return RedirectToAction("CreateHostProfile", "Users");
        }

        var model = new Janus.ViewModels.Opportunities.CreateModel(_context, _userManager)
        {
            CanSelectHost = isAdmin,
            Opportunity = new Opportunity
            {
                IsOpen = true,
                HostUserId = isAdmin ? string.Empty : user.Id
            },
            HostOptions = await LoadHostsAsync()
        };

        return View(model);
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.HostRole + "," + AppConstants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Opportunity")] Opportunity opportunity)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return Forbid();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        if (!isAdmin)
        {
            opportunity.HostUserId = user.Id;
        }

        var selectedHost = await _userManager.FindByIdAsync(opportunity.HostUserId);
        if (selectedHost == null || !selectedHost.IsActive || !await _userManager.IsInRoleAsync(selectedHost, AppConstants.HostRole))
        {
            ModelState.AddModelError(string.Empty, "Select a valid host.");
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Opportunities.CreateModel(_context, _userManager)
            {
                Opportunity = opportunity,
                CanSelectHost = isAdmin,
                HostOptions = await LoadHostsAsync()
            });
        }

        opportunity.CreatedAt = DateTime.UtcNow;
        opportunity.UpdatedAt = DateTime.UtcNow;
        opportunity.IsActive = true;

        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Opportunity created.";
        return RedirectToAction(nameof(MyOpportunities));
    }

    [Authorize(Roles = AppConstants.HostRole + "," + AppConstants.AdminRole)]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (user == null || opportunity == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        if (!isAdmin && opportunity.HostUserId != user.Id)
        {
            return Forbid();
        }

        return View(new Janus.ViewModels.Opportunities.EditModel(_context, _userManager)
        {
            Opportunity = opportunity
        });
    }

    [HttpPost]
    [Authorize(Roles = AppConstants.HostRole + "," + AppConstants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "Opportunity")] Opportunity opportunity)
    {
        var user = await _userManager.GetUserAsync(User);
        var existing = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == opportunity.Id && o.IsActive);

        if (user == null || existing == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        if (!isAdmin && existing.HostUserId != user.Id)
        {
            return Forbid();
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Opportunities.EditModel(_context, _userManager) { Opportunity = opportunity });
        }

        existing.Title = opportunity.Title;
        existing.Industry = opportunity.Industry;
        existing.City = opportunity.City;
        existing.SkillsTaught = opportunity.SkillsTaught;
        existing.Description = opportunity.Description;
        existing.IsOpen = opportunity.IsOpen;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Opportunity updated.";
        return RedirectToAction(nameof(MyOpportunities));
    }

    [Authorize(Roles = AppConstants.HostRole + "," + AppConstants.AdminRole)]
    public async Task<IActionResult> Delete(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (user == null || opportunity == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        if (!isAdmin && opportunity.HostUserId != user.Id)
        {
            return Forbid();
        }

        return View(new Janus.ViewModels.Opportunities.DeleteModel(_context, _userManager) { Opportunity = opportunity });
    }

    [HttpPost, ActionName("Delete")]
    [Authorize(Roles = AppConstants.HostRole + "," + AppConstants.AdminRole)]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == id && o.IsActive);

        if (user == null || opportunity == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        if (!isAdmin && opportunity.HostUserId != user.Id)
        {
            return Forbid();
        }

        opportunity.IsActive = false;
        opportunity.IsOpen = false;
        opportunity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Opportunity removed.";
        return RedirectToAction(nameof(MyOpportunities));
    }

    private async Task<SelectList> LoadHostsAsync()
    {
        var hosts = await _userManager.GetUsersInRoleAsync(AppConstants.HostRole);
        var activeHosts = hosts
            .Where(u => u.IsActive)
            .OrderBy(u => u.LastName)
            .ToList();

        return new SelectList(activeHosts, "Id", "DisplayName");
    }
}
