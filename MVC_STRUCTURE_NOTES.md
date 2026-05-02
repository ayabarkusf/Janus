# MVC Structure Notes

This version follows an ASP.NET Core MVC structure.

## Main folders

- `Controllers/` contains request-handling logic for the public application, including `AccountController` for login, logout, registration and password reset screens.
- `Areas/Admin/Controllers/` contains request-handling logic for the admin area.
- `Views/` contains MVC views organized by controller name, including `Views/Account` for account screens.
- `Areas/Admin/Views/` contains MVC views for admin controllers.
- `Models/` contains database entities.
- `ViewModels/` contains screen-specific models used by MVC views.
- `Data/` contains Entity Framework Core configuration, migrations and seed data.

## Identity and permissions

The project uses ASP.NET Core Identity for authentication and role-based authorization.

The roles are:

- `Student`
- `Host`
- `Admin`

Controller actions use role-based authorization such as:

```csharp
[Authorize(Roles = AppConstants.AdminRole)]
[Authorize(Roles = AppConstants.HostRole + "," + AppConstants.AdminRole)]
[Authorize(Roles = AppConstants.StudentRole)]
```

The `ApplicationUser` profile fields store user information, while Identity roles control access to protected actions.
