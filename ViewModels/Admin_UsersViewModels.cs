using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Admin.Users;
public class DeleteModel
{
    public DeleteModel() { }
    public DeleteModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context) { }
    public ApplicationUser? ManagedUser { get; set; }
    public string UserId { get; set; } = string.Empty;
}

public class DetailsModel
{
    public DetailsModel() { }
    public DetailsModel(UserManager<ApplicationUser> userManager) { }
    public ApplicationUser? ManagedUser { get; set; }
}

public class EditModel
{
    public EditModel() { }
    public EditModel(UserManager<ApplicationUser> userManager) { }
    public InputModel Input { get; set; } = new();

    public class InputModel
    {
        public string Id { get; set; } = string.Empty;
        [Required] public string FirstName { get; set; } = string.Empty;
        [Required] public string LastName { get; set; } = string.Empty;
        public string? City { get; set; }
        public string? GradeLevel { get; set; }
        public string? CareerInterest { get; set; }
        public string? Company { get; set; }
        public string? Industry { get; set; }
        public string? Bio { get; set; }
        public bool IsStudent { get; set; }
        public bool IsHost { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsActive { get; set; }
    }
}

public class IndexModel
{
    public IndexModel() { }
    public IndexModel(UserManager<ApplicationUser> userManager) { }
    public IList<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
}
