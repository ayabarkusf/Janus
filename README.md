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
