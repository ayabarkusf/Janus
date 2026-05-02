using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Janus.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IServiceProvider services)
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await context.Database.MigrateAsync();
        await EnsureRolesAsync(roleManager);

        if (!await userManager.Users.AnyAsync())
        {
            var admin = await CreateUserAsync(userManager, "admin@janus.com", "Admin123!", "Janus", "Admin", "Tampa", roles: new[] { AppConstants.AdminRole }, isAdmin: true);
            var sarah = await CreateUserAsync(userManager, "sarah.kim@tgh.org", "Host123!", "Sarah", "Kim", "Tampa", roles: new[] { AppConstants.HostRole }, isHost: true, company: "Tampa General Hospital", industry: "Healthcare", bio: "Pediatric nurse mentor helping students explore healthcare careers.");
            var priya = await CreateUserAsync(userManager, "priya.noor@techdata.com", "Host123!", "Priya", "Noor", "Clearwater", roles: new[] { AppConstants.HostRole }, isHost: true, company: "Tech Data Corp", industry: "Technology", bio: "Technology leader focused on software development and data careers.");
            var monica = await CreateUserAsync(userManager, "monica.edmond@lewishart.com", "Host123!", "Monica", "Edmond", "Orlando", roles: new[] { AppConstants.HostRole }, isHost: true, company: "Lewis & Hart Law", industry: "Law", bio: "Attorney offering legal research shadowing experiences.");
            var john = await CreateUserAsync(userManager, "john.smith@deloitte.com", "Host123!", "John", "Smith", "Miami", roles: new[] { AppConstants.HostRole }, isHost: true, company: "Deloitte", industry: "Finance", bio: "Finance mentor helping students understand analytics and client work.");

            var maya = await CreateUserAsync(userManager, "maya.johnson@email.com", "Student123!", "Maya", "Johnson", "Tampa", roles: new[] { AppConstants.StudentRole }, isStudent: true, grade: "11th", interest: "Healthcare");
            var alex = await CreateUserAsync(userManager, "alex.chen@email.com", "Student123!", "Alex", "Chen", "Clearwater", roles: new[] { AppConstants.StudentRole }, isStudent: true, grade: "12th", interest: "Technology");
            var jordan = await CreateUserAsync(userManager, "jordan.reyes@email.com", "Student123!", "Jordan", "Reyes", "Tampa", roles: new[] { AppConstants.StudentRole }, isStudent: true, grade: "11th", interest: "Law");
            var taylor = await CreateUserAsync(userManager, "taylor.brooks@email.com", "Student123!", "Taylor", "Brooks", "St. Pete", roles: new[] { AppConstants.StudentRole }, isStudent: true, grade: "10th", interest: "Finance");

            var opportunities = new List<Opportunity>
            {
                NewOpportunity(sarah.Id, "Shadow a Pediatric Nurse", "Healthcare", "Tampa", "Patient Care, Medical Terminology, Empathy", "Spend time with a pediatric nurse and learn how healthcare teams support children and families.", 0),
                NewOpportunity(sarah.Id, "Healthcare Operations Shadow", "Healthcare", "Tampa", "Documentation, Patient Care, Medical Terminology", "Explore how hospital operations support clinical care and patient experience.", 7),
                NewOpportunity(sarah.Id, "Nursing Basics Shadow", "Healthcare", "St. Pete", "Patient Care, Empathy, Documentation", "Learn the daily routine, responsibilities and communication skills of nursing work.", 14),
                NewOpportunity(priya.Id, "Software Development Internship", "Technology", "Clearwater", "Python, Data Analysis, Problem Solving", "Shadow a software team and learn how developers plan, build and test applications.", 21),
                NewOpportunity(priya.Id, "Data Science Internship", "Technology", "Tampa", "Python, Data Analysis, Research Methods", "Explore how data teams transform raw data into decisions.", 28),
                NewOpportunity(priya.Id, "Software Engineer Shadow", "Technology", "Remote", "Python, Data Analysis, Problem Solving", "A remote shadow experience focused on software engineering workflows.", 35),
                NewOpportunity(monica.Id, "Legal Research Assistant", "Law", "Orlando", "Legal Research, Writing, Attention to Detail", "Learn how legal research supports attorneys, clients and court preparation.", 42),
                NewOpportunity(john.Id, "Financial Analysis Shadow", "Finance", "Miami", "Financial Literacy, Excel, Reporting", "Observe how analysts review financial information and prepare client-ready reports.", 49),
                NewOpportunity(john.Id, "Investment Banking Shadow", "Finance", "St. Pete", "Financial Literacy, Excel, Data Analysis", "Explore valuation, market research and financial storytelling.", 56),
                NewOpportunity(monica.Id, "3rd Grade Teaching Shadow", "Education", "Tampa", "Curriculum Design, Research Methods, Empathy", "Spend time in an education environment and learn how teachers prepare and deliver lessons.", 63)
            };

            context.Opportunities.AddRange(opportunities);
            await context.SaveChangesAsync();

            context.OpportunityApplications.AddRange(
                NewApplication(maya.Id, opportunities[0].Id, "Accepted", "I want to be a pediatric nurse and would love to learn how nurses help children feel safe."),
                NewApplication(maya.Id, opportunities[1].Id, "Pending", "I am curious about how hospitals work beyond direct patient care."),
                NewApplication(alex.Id, opportunities[3].Id, "Accepted", "I have been learning Python and want to see how software teams work."),
                NewApplication(alex.Id, opportunities[4].Id, "Pending", "I enjoyed AP Statistics and want to understand data science careers."),
                NewApplication(jordan.Id, opportunities[6].Id, "Pending", "I am mock trial captain and want to learn about legal research."),
                NewApplication(taylor.Id, opportunities[8].Id, "Declined", "I am interested in markets and finance careers.")
            );

            context.MarketInsights.AddRange(
                NewInsight("Healthcare", "Market outlook", "Strong demand for clinical and operations roles", "Prepared for BLS API", null),
                NewInsight("Technology", "Market outlook", "High demand for data, software and cybersecurity skills", "Prepared for BLS API", null),
                NewInsight("Finance", "Market outlook", "Continued need for analytics, reporting and financial literacy", "Prepared for BLS API", null),
                NewInsight("Law", "Market outlook", "Stable demand for legal research and compliance support", "Prepared for BLS API", null),
                NewInsight("Education", "Market outlook", "Ongoing demand for teachers, counselors and education support", "Prepared for BLS API", null)
            );

            await context.SaveChangesAsync();
        }

        await SyncExistingRoleAssignmentsAsync(userManager);
    }

    private static async Task<ApplicationUser> CreateUserAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        string firstName,
        string lastName,
        string city,
        IEnumerable<string>? roles = null,
        bool isStudent = false,
        bool isHost = false,
        bool isAdmin = false,
        string? company = null,
        string? industry = null,
        string? bio = null,
        string? grade = null,
        string? interest = null)
    {
        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = true,
            FirstName = firstName,
            LastName = lastName,
            City = city,
            IsStudent = isStudent,
            IsHost = isHost,
            IsAdmin = isAdmin,
            IsActive = true,
            Company = company,
            Industry = industry,
            Bio = bio,
            GradeLevel = grade,
            CareerInterest = interest,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));
        }

        if (roles != null)
        {
            var roleResult = await userManager.AddToRolesAsync(user, roles);
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(string.Join("; ", roleResult.Errors.Select(e => e.Description)));
            }
        }

        return user;
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole> roleManager)
    {
        foreach (var role in AppConstants.Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }

    private static async Task SyncExistingRoleAssignmentsAsync(UserManager<ApplicationUser> userManager)
    {
        var users = await userManager.Users.ToListAsync();

        foreach (var user in users)
        {
            await SyncRoleAsync(userManager, user, AppConstants.StudentRole, user.IsStudent);
            await SyncRoleAsync(userManager, user, AppConstants.HostRole, user.IsHost);
            await SyncRoleAsync(userManager, user, AppConstants.AdminRole, user.IsAdmin);
        }
    }

    private static async Task SyncRoleAsync(UserManager<ApplicationUser> userManager, ApplicationUser user, string role, bool shouldHaveRole)
    {
        var hasRole = await userManager.IsInRoleAsync(user, role);

        if (shouldHaveRole && !hasRole)
        {
            await userManager.AddToRoleAsync(user, role);
        }
        else if (!shouldHaveRole && hasRole)
        {
            await userManager.RemoveFromRoleAsync(user, role);
        }
    }

    private static Opportunity NewOpportunity(string hostId, string title, string industry, string city, string skills, string description, int daysAgo)
    {
        return new Opportunity
        {
            HostUserId = hostId,
            Title = title,
            Industry = industry,
            City = city,
            SkillsTaught = skills,
            Description = description,
            IsOpen = true,
            IsActive = true,
            CreatedAt = DateTime.UtcNow.AddDays(-daysAgo),
            UpdatedAt = DateTime.UtcNow.AddDays(-daysAgo)
        };
    }

    private static OpportunityApplication NewApplication(string studentId, int opportunityId, string status, string note)
    {
        return new OpportunityApplication
        {
            StudentUserId = studentId,
            OpportunityId = opportunityId,
            Status = status,
            CoverNote = note,
            AppliedAt = DateTime.UtcNow.AddDays(-3),
            UpdatedAt = DateTime.UtcNow.AddDays(-3)
        };
    }

    private static MarketInsight NewInsight(string industry, string metric, string value, string source, string? url)
    {
        return new MarketInsight
        {
            Industry = industry,
            MetricName = metric,
            MetricValue = value,
            SourceName = source,
            SourceUrl = url,
            LastUpdatedAt = DateTime.UtcNow
        };
    }
}
