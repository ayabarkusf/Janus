using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Janus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppConstants.AdminRole)]
public class OpportunitiesController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public OpportunitiesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return View(new Janus.ViewModels.Admin.Opportunities.IndexModel(_context, _userManager)
        {
            Opportunities = await _context.Opportunities.Include(o => o.HostUser).OrderByDescending(o => o.CreatedAt).ToListAsync()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var opportunity = await _context.Opportunities.Include(o => o.HostUser).FirstOrDefaultAsync(o => o.Id == id);
        return opportunity == null ? NotFound() : View(new Janus.ViewModels.Admin.Opportunities.DetailsModel(_context, _userManager) { Opportunity = opportunity });
    }

    public async Task<IActionResult> Create()
    {
        return View(new Janus.ViewModels.Admin.Opportunities.CreateModel(_context, _userManager)
        {
            Opportunity = new Opportunity { IsOpen = true, IsActive = true },
            HostOptions = await LoadHostsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind(Prefix = "Opportunity")] Opportunity opportunity)
    {
        var host = await _userManager.FindByIdAsync(opportunity.HostUserId);
        if (host == null || !host.IsActive || !await _userManager.IsInRoleAsync(host, AppConstants.HostRole))
        {
            ModelState.AddModelError(string.Empty, "Select a valid host.");
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Admin.Opportunities.CreateModel(_context, _userManager)
            {
                Opportunity = opportunity,
                HostOptions = await LoadHostsAsync()
            });
        }

        opportunity.CreatedAt = DateTime.UtcNow;
        opportunity.UpdatedAt = DateTime.UtcNow;
        _context.Opportunities.Add(opportunity);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Opportunity created.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == id);
        if (opportunity == null) return NotFound();

        return View(new Janus.ViewModels.Admin.Opportunities.EditModel(_context, _userManager)
        {
            Opportunity = opportunity,
            HostOptions = await LoadHostsAsync()
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "Opportunity")] Opportunity opportunity)
    {
        var existing = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == opportunity.Id);
        if (existing == null) return NotFound();

        var host = await _userManager.FindByIdAsync(opportunity.HostUserId);
        if (host == null || !host.IsActive || !await _userManager.IsInRoleAsync(host, AppConstants.HostRole))
        {
            ModelState.AddModelError(string.Empty, "Select a valid host.");
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Admin.Opportunities.EditModel(_context, _userManager)
            {
                Opportunity = opportunity,
                HostOptions = await LoadHostsAsync()
            });
        }

        existing.HostUserId = opportunity.HostUserId;
        existing.Title = opportunity.Title;
        existing.Industry = opportunity.Industry;
        existing.City = opportunity.City;
        existing.SkillsTaught = opportunity.SkillsTaught;
        existing.Description = opportunity.Description;
        existing.IsOpen = opportunity.IsOpen;
        existing.IsActive = opportunity.IsActive;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        TempData["Success"] = "Opportunity updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var opportunity = await _context.Opportunities.FindAsync(id);
        if (opportunity == null) return NotFound();

        return View(new Janus.ViewModels.Admin.Opportunities.DeleteModel(_context, _userManager)
        {
            Opportunity = opportunity,
            OpportunityId = id
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int opportunityId)
    {
        var opportunity = await _context.Opportunities.FindAsync(opportunityId);
        if (opportunity == null) return NotFound();

        opportunity.IsActive = false;
        opportunity.IsOpen = false;
        opportunity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Opportunity deactivated.";
        return RedirectToAction(nameof(Index));
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
