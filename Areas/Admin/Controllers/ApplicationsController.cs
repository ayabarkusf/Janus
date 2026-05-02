using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = AppConstants.AdminRole)]
public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        return View(new Janus.ViewModels.Admin.Applications.IndexModel(_context, _userManager)
        {
            Applications = await _context.OpportunityApplications
                .Include(a => a.StudentUser)
                .Include(a => a.Opportunity)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync()
        });
    }

    public async Task<IActionResult> Details(int id)
    {
        var application = await _context.OpportunityApplications
            .Include(a => a.StudentUser)
            .Include(a => a.Opportunity)
            .ThenInclude(o => o!.HostUser)
            .FirstOrDefaultAsync(a => a.Id == id);

        return application == null
            ? NotFound()
            : View(new Janus.ViewModels.Admin.Applications.DetailsModel(_context, _userManager) { Application = application });
    }

    public async Task<IActionResult> Edit(int id)
    {
        var application = await _context.OpportunityApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (application == null) return NotFound();

        return View(new Janus.ViewModels.Admin.Applications.EditModel(_context, _userManager)
        {
            Input = new Janus.ViewModels.Admin.Applications.EditModel.InputModel
            {
                Id = application.Id,
                Status = application.Status,
                CoverNote = application.CoverNote,
                IsActive = application.IsActive
            }
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit([Bind(Prefix = "Input")] Janus.ViewModels.Admin.Applications.EditModel.InputModel input)
    {
        var application = await _context.OpportunityApplications.FirstOrDefaultAsync(a => a.Id == input.Id);
        if (application == null) return NotFound();

        if (!AppConstants.ApplicationStatuses.Contains(input.Status))
        {
            ModelState.AddModelError(string.Empty, "Invalid status.");
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Admin.Applications.EditModel(_context, _userManager) { Input = input });
        }

        application.Status = input.Status;
        application.CoverNote = input.CoverNote;
        application.IsActive = input.IsActive;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Application updated.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Delete(int id)
    {
        var application = await _context.OpportunityApplications.FindAsync(id);
        if (application == null) return NotFound();

        return View(new Janus.ViewModels.Admin.Applications.DeleteModel(_context, _userManager)
        {
            Application = application,
            ApplicationId = id
        });
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int applicationId)
    {
        var application = await _context.OpportunityApplications.FindAsync(applicationId);
        if (application == null) return NotFound();

        application.IsActive = false;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Application deactivated.";
        return RedirectToAction(nameof(Index));
    }
}
