using Janus.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Janus.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Opportunity> Opportunities => Set<Opportunity>();
    public DbSet<OpportunityApplication> OpportunityApplications => Set<OpportunityApplication>();
    public DbSet<MarketInsight> MarketInsights => Set<MarketInsight>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Opportunity>()
            .HasOne(o => o.HostUser)
            .WithMany(u => u.HostedOpportunities)
            .HasForeignKey(o => o.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityApplication>()
            .HasOne(a => a.StudentUser)
            .WithMany(u => u.OpportunityApplications)
            .HasForeignKey(a => a.StudentUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityApplication>()
            .HasOne(a => a.Opportunity)
            .WithMany(o => o.Applications)
            .HasForeignKey(a => a.OpportunityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<OpportunityApplication>()
            .HasIndex(a => new { a.StudentUserId, a.OpportunityId })
            .IsUnique();
    }
}
