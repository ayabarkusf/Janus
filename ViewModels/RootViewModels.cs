using Janus.Data;
using Janus.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace Janus.ViewModels;
public class IndexModel
{
    public IndexModel() { }
    public IndexModel(ApplicationDbContext context) { }
    public IList<Opportunity> FeaturedOpportunities { get; set; } = new List<Opportunity>();
}

public class AboutModel { }
public class AccessDeniedModel { }
public class BotModel { }
public class ErrorModel { }
public class PrivacyModel { }

