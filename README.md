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
