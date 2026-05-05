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

## Changelog

### v1.2.0 — 2026-05-04

#### Authentication & Role Assignment
- `RegisterStudent` and `RegisterHost` now call `AddToRoleAsync` before `SignInAsync`, so the Identity role is included in the cookie from the very first login — previously registered users had no role claim, which broke all role-guarded pages
- All role checks across controllers and views now use `user.IsStudent` / `user.IsHost` as a fallback alongside `IsInRoleAsync`, making the app resilient to stale cookies

#### My Profile
- Added **Phone** field to the profile display (shown only when filled)
- Removed "Create Student Profile" and "Create Host Profile" buttons; replaced with **My Applications** (students) and **My Opportunities** (hosts)
- Already-student users navigating to `CreateStudentProfile` are now redirected to My Profile instead of seeing the form

#### Create Host Profile — Two-Step Flow
- Navigating to `/Users/CreateHostProfile` now shows a confirmation screen ("Create a Host Account? Yes / No") before displaying the form
- The form asks only for Company, Industry, and Bio (City is carried over silently from the existing profile)
- Already-host users are redirected to My Profile with a message

#### Opportunities Controller — Host Fixes
- Removed `[Authorize(Roles = HostRole)]` attributes from Create, Edit, and Delete actions; replaced with manual checks that accept both the role claim and the `IsHost` flag
- Fixed silent form failure on Create: `HostUserId` is `[Required]` on the model but not in the form for non-admin hosts — added `ModelState.Remove("Opportunity.HostUserId")` after setting it programmatically
- `LoadHostsAsync` now unions results from the role query and the `IsHost` flag so all host accounts appear in the admin dropdown
- Updated Create page subtitle from "Available only for hosts and admins." to "Post a new career shadow opportunity for students."

#### Apply Button — Role-Based Visibility
- Apply button on Opportunities Index and Details pages is now shown only when the user has the **Student** role (supports users who are both student and host)
- Access Denied page updated: replaced single "Go Home" button with three options — Back to Opportunities, My Applications, Go Home
- `Apply` POST action no longer uses `[Authorize(Roles = StudentRole)]`; manual check redirects to CreateStudentProfile instead of triggering Access Denied

#### Home Page — Context-Aware CTAs
- "Create Account" hero button hidden when the user is already logged in
- Student card shows "My Applications" for students, nothing for host-only users
- Host card shows "My Opportunities" for hosts, "Create Host Profile" (confirmation flow) for logged-in non-hosts, and Register link for guests

#### Mobile Responsiveness
- Opportunities table: hides Host, Industry, City, Skills columns on mobile; shows Title, Company, and action buttons only
- My Applications table: hides Host, Industry, Status, Applied columns on mobile; shows Opportunity, Company, Details button only
- Buttons no longer stretch to full width on mobile (removed global `width: 100%` from `@media (max-width: 576px)`); Home hero buttons unaffected
- Edit Profile save/cancel buttons wrapped in flex container for consistent mobile layout
- Added `.btn-sm-janus` CSS class for compact buttons inside tables

#### Navigation & Layout
- Removed **Hosts** link from nav menu and footer
- Removed **About** link from footer
- Removed demo account credentials from the Sign In page

#### Charts
- Second chart renamed from "Applications by Status" to **"Applications by Industry"**; query now joins `OpportunityApplications` with `Opportunities` to group by industry

---

### v1.1.0 — 2026-05-03

#### Authentication & Registration
- Replaced multi-step registration flow with a single combined page (`/Account/Register`) containing two tab sections: **Student Registration** (`#student`) and **Join as Host** (`#host`)
- Added `RegisterStudentViewModel`, `RegisterHostViewModel`, and `RegisterPageViewModel`
- Added `RegisterStudent` and `RegisterHost` POST actions to `AccountController`; each creates the user account and sets the appropriate role flag (`IsStudent` / `IsHost`) in one step
- Navigation: removed the "Register" link from the nav bar; renamed "Login" to "Sign In"; updated footer link to "Sign Up"

#### Homepage
- "Create Student Profile" and "Create Host Profile" buttons now link directly to `/Account/Register#student` and `/Account/Register#host`, opening the correct registration tab
- Registration page tabs scroll to the tab navigation area on arrival (anchor divs placed above the tab bar)
- Tab styling updated to site's `nav-tabs-janus` class: green/teal border and text when active, gray with no border when inactive

#### Industry field
- Added **"Other"** option to `AppConstants.Industries`, which propagates to all Industry dropdowns across the site (registration, create/edit opportunity, edit profile)
- Browse Opportunities filter excludes "Other" from the Industry dropdown (students cannot filter by "Other")

#### Admin area
- All boolean fields (`IsStudent`, `IsHost`, `IsAdmin`, `IsActive`, `IsOpen`) now display as **Yes / No** instead of True / False in all admin list and detail views (Users, Opportunities, Applications)
- Fixed explicit labels for **Open** and **Active** checkboxes on admin create/edit forms (were previously empty)

#### My Profile
- Replaced "Student: Yes/No" and "Host: No" static labels with role badges that appear only when the user holds that role (`Student`, `Host`, `Admin`)

#### Application Details
- Fixed "Back" button: students are now returned to **My Applications**; hosts are returned to **My Opportunities** (was always going to My Opportunities, causing a 403 for students)

#### Footer
- Removed My Profile, Create Student Profile, Create Host Profile, and Sign Up links from the footer
- Replaced with About and Hosts links

#### Account deletion
- Simplified confirmation message to: *"Once deleted, this account can no longer be accessed."*

---

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