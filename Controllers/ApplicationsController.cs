using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Janus.Controllers;

[Authorize]
public class ApplicationsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public ApplicationsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    public async Task<IActionResult> Apply(int opportunityId)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
        {
            return RedirectToAction("Login", "Account");
        }

        var model = new Janus.ViewModels.Applications.ApplyModel(_context, _userManager);
        if (!await _userManager.IsInRoleAsync(user, AppConstants.StudentRole))
        {
            model.NeedsStudentProfile = true;
            return View(model);
        }

        model.Opportunity = await _context.Opportunities
            .Include(o => o.HostUser)
            .FirstOrDefaultAsync(o => o.Id == opportunityId && o.IsActive && o.IsOpen);

        if (model.Opportunity == null)
        {
            return NotFound();
        }

        var alreadyApplied = await _context.OpportunityApplications
            .AnyAsync(a => a.OpportunityId == opportunityId && a.StudentUserId == user.Id);

        if (alreadyApplied)
        {
            TempData["Error"] = "You have already applied to this opportunity.";
            return RedirectToAction(nameof(MyApplications));
        }

        model.Input.OpportunityId = opportunityId;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Apply([Bind(Prefix = "Input")] Janus.ViewModels.Applications.ApplyModel.InputModel input)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null || !user.IsActive)
            return RedirectToAction("Login", "Account");

        if (!user.IsStudent && !await _userManager.IsInRoleAsync(user, AppConstants.StudentRole))
        {
            TempData["Error"] = "You need a student profile to apply for opportunities.";
            return RedirectToAction("CreateStudentProfile", "Users");
        }

        var opportunity = await _context.Opportunities
            .Include(o => o.HostUser)
            .FirstOrDefaultAsync(o => o.Id == input.OpportunityId && o.IsActive && o.IsOpen);

        if (opportunity == null)
        {
            return NotFound();
        }

        var alreadyApplied = await _context.OpportunityApplications
            .AnyAsync(a => a.OpportunityId == input.OpportunityId && a.StudentUserId == user.Id);

        if (alreadyApplied)
        {
            ModelState.AddModelError(string.Empty, "You have already applied to this opportunity.");
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Applications.ApplyModel(_context, _userManager)
            {
                Opportunity = opportunity,
                Input = input
            });
        }

        var application = new OpportunityApplication
        {
            StudentUserId = user.Id,
            OpportunityId = input.OpportunityId,
            CoverNote = input.CoverNote,
            Status = "Pending",
            AppliedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.OpportunityApplications.Add(application);
        await _context.SaveChangesAsync();

        TempData["Success"] = "Application submitted.";
        return RedirectToAction(nameof(ThankYou), new { opportunityId = input.OpportunityId });
    }

    public async Task<IActionResult> Details(int id)
    {
        var user = await _userManager.GetUserAsync(User);
        var application = await LoadApplicationAsync(id);

        if (user == null || application == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        var isStudentOwner = application.StudentUserId == user.Id;
        var isHostOwner = application.Opportunity?.HostUserId == user.Id;

        if (!isAdmin && !isStudentOwner && !isHostOwner)
        {
            return Forbid();
        }

        var model = new Janus.ViewModels.Applications.DetailsModel(_context, _userManager)
        {
            Application = application,
            CanEditStatus = isAdmin || isHostOwner,
            StatusInput = new Janus.ViewModels.Applications.DetailsModel.StatusInputModel
            {
                ApplicationId = application.Id,
                Status = application.Status
            }
        };

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details([Bind(Prefix = "StatusInput")] Janus.ViewModels.Applications.DetailsModel.StatusInputModel statusInput)
    {
        var user = await _userManager.GetUserAsync(User);
        var application = await LoadApplicationAsync(statusInput.ApplicationId);

        if (user == null || application == null)
        {
            return NotFound();
        }

        var isAdmin = await _userManager.IsInRoleAsync(user, AppConstants.AdminRole);
        var isHostOwner = application.Opportunity?.HostUserId == user.Id;
        if (!isAdmin && !isHostOwner)
        {
            return Forbid();
        }

        if (!AppConstants.ApplicationStatuses.Contains(statusInput.Status))
        {
            ModelState.AddModelError(string.Empty, "Invalid status.");
        }

        if (!ModelState.IsValid)
        {
            return View(new Janus.ViewModels.Applications.DetailsModel(_context, _userManager)
            {
                Application = application,
                CanEditStatus = true,
                StatusInput = statusInput
            });
        }

        application.Status = statusInput.Status;
        application.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        TempData["Success"] = "Application status updated.";
        return RedirectToAction(nameof(Details), new { id = application.Id });
    }

    public async Task<IActionResult> MyApplications()
    {
        var user = await _userManager.GetUserAsync(User);

        if (user == null || !user.IsActive || (!user.IsStudent && !await _userManager.IsInRoleAsync(user, AppConstants.StudentRole)))
        {
            TempData["Error"] = "Create a student profile to view applications.";
            return RedirectToAction("CreateStudentProfile", "Users");
        }

        var model = new Janus.ViewModels.Applications.MyApplicationsModel(_context, _userManager)
        {
            Applications = await _context.OpportunityApplications
                .Where(a => a.IsActive)
                .Include(a => a.Opportunity)
                .ThenInclude(o => o!.HostUser)
                .Where(a => a.StudentUserId == user.Id)
                .OrderByDescending(a => a.AppliedAt)
                .ToListAsync()
        };

        return View(model);
    }

    [AllowAnonymous]
    public async Task<IActionResult> ThankYou(int opportunityId)
    {
        var model = new Janus.ViewModels.Applications.ThankYouModel(_context);
        var opportunity = await _context.Opportunities.FirstOrDefaultAsync(o => o.Id == opportunityId);
        if (opportunity != null)
        {
            model.OpportunityTitle = opportunity.Title;
        }

        return View(model);
    }

    private Task<OpportunityApplication?> LoadApplicationAsync(int id)
    {
        return _context.OpportunityApplications
            .Include(a => a.StudentUser)
            .Include(a => a.Opportunity)
            .ThenInclude(o => o!.HostUser)
            .FirstOrDefaultAsync(a => a.Id == id);
    }
}
