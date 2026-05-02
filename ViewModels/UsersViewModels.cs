using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels.Users;
public class CreateHostProfileModel
{
    public CreateHostProfileModel() { }
    public CreateHostProfileModel(UserManager<ApplicationUser> userManager) { }
    public InputModel Input { get; set; } = new();
    public bool AlreadyHost { get; set; }
    public string[] Industries => AppConstants.Industries;

    public class InputModel
    {
        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string Company { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string Industry { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Bio { get; set; }
    }
}

public class CreateStudentProfileModel
{
    public CreateStudentProfileModel() { }
    public CreateStudentProfileModel(UserManager<ApplicationUser> userManager) { }
    public InputModel Input { get; set; } = new();
    public bool AlreadyStudent { get; set; }
    public string[] Industries => AppConstants.Industries;
    public string[] GradeLevels => AppConstants.GradeLevels;

    public class InputModel
    {
        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [Required, Display(Name = "Grade level")]
        public string GradeLevel { get; set; } = string.Empty;

        [Required, Display(Name = "Career interest")]
        public string CareerInterest { get; set; } = string.Empty;
    }
}

public class DeleteAccountModel
{
    public DeleteAccountModel() { }
    public DeleteAccountModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, ApplicationDbContext context) { }
}

public class EditProfileModel
{
    public EditProfileModel() { }
    public EditProfileModel(UserManager<ApplicationUser> userManager) { }
    public InputModel Input { get; set; } = new();
    public bool IsStudent { get; set; }
    public bool IsHost { get; set; }
    public string[] Industries => AppConstants.Industries;
    public string[] GradeLevels => AppConstants.GradeLevels;

    public class InputModel
    {
        [Required, StringLength(50), Display(Name = "First name")]
        public string FirstName { get; set; } = string.Empty;

        [Required, StringLength(50), Display(Name = "Last name")]
        public string LastName { get; set; } = string.Empty;

        [StringLength(20), Display(Name = "Phone number")]
        public string? PhoneNumber { get; set; }

        [Required, StringLength(50)]
        public string City { get; set; } = string.Empty;

        [StringLength(20), Display(Name = "Grade level")]
        public string? GradeLevel { get; set; }

        [StringLength(50), Display(Name = "Career interest")]
        public string? CareerInterest { get; set; }

        [StringLength(100)]
        public string? Company { get; set; }

        [StringLength(50)]
        public string? Industry { get; set; }

        [StringLength(500)]
        public string? Bio { get; set; }
    }
}

public class MyProfileModel
{
    public MyProfileModel() { }
    public MyProfileModel(UserManager<ApplicationUser> userManager) { }
    public ApplicationUser? CurrentUser { get; set; }
}
