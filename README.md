# Janus MVC

Janus is an ASP.NET Core MVC opportunity platform that connects students with professional shadowing experiences.

## Main features

- MVC architecture with Controllers, Views, Models and ViewModels
- SQL Server LocalDB persistence through Entity Framework Core
- ASP.NET Core Identity for authentication, password hashing and role-based authorization
- Identity roles for Student, Host and Admin permissions
- Admin area for managing users, opportunities and applications
- Seed data for demo users, opportunities, applications and market insights

## Demo accounts

- Admin: admin@janus.com / Admin123!
- Host: sarah.kim@tgh.org / Host123!
- Student: maya.johnson@email.com / Student123!

## Run locally

```bash
dotnet restore
dotnet build
dotnet ef database update
dotnet run
```
## CareerOneStop API Integration
*Implemented by Ashish Karamchandani*

The Market Insights page (`/Insights`) uses the CareerOneStop Web API
sponsored by the U.S. Department of Labor to provide live labor market data.

### What it does
- Search any occupation by keyword (e.g. Nurse, Lawyer, Software Developer)
- Displays live median annual and hourly wages from BLS OEWS 2024 data
- Results are cached for 6 hours to minimize API calls
- Falls back to seeded database records if API is unavailable

### Files added or modified
- `Services/CareerOneStopModels.cs` — API response models and settings
- `Services/CareerOneStopService.cs` — HTTP client, keyword mapping, caching
- `Program.cs` — registered HttpClient, MemoryCache and CareerOneStopService
- `appsettings.json` — added CareerOneStop config section (token left empty)
- `Controllers/InsightsController.cs` — calls the service and handles fallback
- `ViewModels/InsightsViewModels.cs` — added search result properties
- `Views/Insights/Index.cshtml` — search box, result cards, logo attribution
- `wwwroot/images/CareerOneStop-Logo.png` — required logo per API agreement

### API Details
- Provider: CareerOneStop (www.careeronestop.org)
- Endpoint: GET /v1/comparesalaries/{userId}/wage
- Data source: Bureau of Labor Statistics Occupational Employment and Wage Statistics (OEWS)
- Agreement expires: 5/1/2029

### Important — Setup for teammates
The API token is NOT stored in the repository for security reasons.
After pulling this branch you must add the token locally or the
Insights search will not work and will show the fallback data instead.

Steps:
1. Open Visual Studio
2. Right-click the Janus project in Solution Explorer
3. Click Manage User Secrets
4. Paste the following into secrets.json:

{
  "CareerOneStop:Token": "<ask Aya Bark for the token>"
}

The UserId is already in appsettings.json and does not need to be added.