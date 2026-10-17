# CuttingOptimizer — Project Summary

**Last updated:** 17 October 2026  
**Current stage:** Backend foundation and several API features are written. The latest `dotnet build` succeeded, but tests, current migration state, and runtime API behavior have not yet been verified.

## 1. Project Idea

CuttingOptimizer is a system for workshops that process aluminum profiles and other linear materials. It aims to organize workshop projects, materials, stock bars, and required cuts, then eventually generate efficient cutting plans that reduce material waste and cost.

Planned capabilities:

- Track required piece lengths and quantities.
- Allocate requested pieces to available stock bars.
- Account for saw blade kerf.
- Calculate leftover lengths and estimated material cost.
- Potentially reuse suitable offcuts in later jobs.
- Distinguish available inventory from additional stock that needs to be purchased.

**Important:** The cutting optimization engine has not been implemented yet. Kerf rules, offcut reuse policy, and the optimization objective still need to be defined using real workshop requirements.

## 2. Example Use Case

Suppose a workshop needs:

| Piece length | Quantity |
|---:|---:|
| 2,500 mm | 2 |
| 1,800 mm | 2 |
| 1,200 mm | 1 |

The stock bars are 6,000 mm long. The future optimizer should compare valid allocations, account for kerf, calculate leftovers, and estimate cost. This is an illustrative example, not a tested result.

## 3. Scope and Future Expansion

### Initial scope
- Aluminum profiles.
- Workshop/project/section management.
- Material catalog and stock records.
- Required cuts and quantities.
- Cutting-plan optimization.

### Potential future scope
- Structural glass and related materials.
- Wood and other linear materials.
- Material-specific rules and calculations.

The first version should focus on aluminum profiles instead of attempting to support every industry from the beginning.

## 4. Technology Stack

| Technology | Purpose |
|---|---|
| C# / .NET 10 | Backend platform |
| ASP.NET Core Web API | HTTP endpoints |
| Clean Architecture | Separation of responsibilities |
| Entity Framework Core 10 | Data access and persistence |
| PostgreSQL 17 | Relational database |
| Docker Compose | Local database environment |
| MediatR | Dispatching commands and queries |
| FluentValidation | Input validation |
| Dependency Injection | Service registration and composition |
| Test projects | Automated tests; execution results are not yet known |

## 5. Solution Structure

```text
CuttingOptimizer/
├── src/
│   ├── CuttingOptimizer.Api/
│   ├── CuttingOptimizer.Application/
│   ├── CuttingOptimizer.Domain/
│   └── CuttingOptimizer.Infrastructure/
├── tests/
│   ├── CuttingOptimizer.Domain.Tests/
│   └── CuttingOptimizer.Application.Tests/
├── compose.yaml
├── .env
└── CuttingOptimizer.sln
```

Project dependencies:

- `Domain`: independent of the other application layers.
- `Application`: depends on `Domain`.
- `Infrastructure`: depends on `Application` and `Domain`.
- `Api`: depends on `Application` and `Infrastructure`.
- Test projects target the layers they test.

## 6. Domain Entities

### Workshop
- `Id`
- `Name`
- Collection of `Projects`

### Project
- `Id`
- `WorkshopId`
- `Name`
- Optional `CustomerName`
- Reference to `Workshop`
- Collection of `Sections`

### Sector
- `Id`
- `Code`
- `Name`
- Collection of `ProjectSections`

### ProjectSection
- `Id`
- `ProjectId`
- `SectorId`
- `Name`
- References to `Project` and `Sector`
- Collection of `RequiredCuts`

### Material
- `Id`
- `SectorId`
- `Code`
- `Name`
- Optional `Description`
- Reference to `Sector`
- Collections of `StockBars` and `RequiredCuts`

### StockBar
- `Id`
- `MaterialId`
- `LengthMm`
- `UnitPrice`
- `AvailableQuantity`
- `IsActive`
- Reference to `Material`

### RequiredCut
- `Id`
- `ProjectSectionId`
- `MaterialId`
- `Name`
- `LengthMm`
- `Quantity`
- References to `ProjectSection` and `Material`

### Important modeling notes
- `StockBar` currently represents an inventory record by material, length, price, and quantity. It does not represent an individually serialized physical bar.
- The application layer checks that a required cut's material belongs to the same sector as its project section. This consistency is not currently enforced by a composite database constraint.
- Inventory consumption, stock reservation, offcut tracking, and optimizer-generated plans are not implemented yet.

## 7. API Features Written So Far

| Feature | Operations written |
|---|---|
| Workshops | CRUD |
| Projects | Create, read, update, delete |
| Project Sections | Create and read |
| Sectors | CRUD |
| Materials | Create, list, get by ID; optional sector filter |
| Stock Bars | Create, list with filters, update, deactivate |
| Required Cuts | Create, list with filters, update, delete |

### Stock Bar endpoints

```http
POST   /api/stock-bars
GET    /api/stock-bars
PUT    /api/stock-bars/{id}
PATCH  /api/stock-bars/{id}/deactivate
```

The list endpoint supports optional filters such as `materialId`, `sectorId`, and `isActive`.

### Validation and persistence rules

- Lengths must be greater than zero.
- Required-cut quantities must be greater than zero.
- Available inventory quantity cannot be negative.
- Unit price cannot be negative.
- Sector codes are unique.
- Material codes are unique within a sector.
- Several relationships use restrictive delete behavior to protect related data.
- FluentValidation pipeline and a global exception handler are configured for consistent validation and error responses.

These features have been written and compile, but their runtime behavior has not yet been verified end-to-end.

## 8. Database and Migrations

The local PostgreSQL database configuration is:

- Database: `cuttingoptimizer`
- User: `cuttingoptimizer_user`
- Local port: `5432`
- Docker Compose service binds to `127.0.0.1:5432`
- Connection string key: `ConnectionStrings:DefaultConnection`

Credentials are intended to remain in `.env` and .NET User Secrets, not in source control.

The initial migration `20261017000408_InitialCreate` was previously applied for the core tables. Later changes introduced `Materials`, `StockBars`, `RequiredCuts`, and `AvailableQuantity`.

**The current migration list and actual database schema have not yet been checked. Do not assume the database is up to date, and do not apply migrations blindly.**

## 9. Current Verified Status

| Item | Status |
|---|---|
| `dotnet build` | **Succeeded** in the latest reported output; all solution projects compiled. |
| `Microsoft.OpenApi` warning | **Unresolved / update not confirmed.** NU1903 reported a high-severity vulnerability for version `2.0.0`. Updating to `2.7.5` was suggested, but no successful result has been reported yet. |
| Automated tests | **Not run / no result reported.** Test projects compiled, but this does not prove the tests pass. |
| EF Core migrations | **Unverified.** Need to list migrations and compare with the database schema. |
| Runtime API testing | **Unverified.** Endpoints have not yet been confirmed against a running database. |
| Cutting optimizer engine | **Not implemented.** Optimization rules and algorithm remain future work. |

## 10. Recommended Next Steps

1. Confirm whether `Microsoft.OpenApi` was updated and run restore/build again.
2. Run automated tests and review any failures.
3. List EF Core migrations and inspect generated migrations.
4. Verify the database schema before applying any database updates.
5. Start PostgreSQL and the API, then test the endpoints with real HTTP requests.
6. Define workshop rules for saw kerf, cutting constraints, leftover reuse, and the optimization objective.
7. Implement the optimizer as a separate, testable service rather than putting the algorithm in controllers or EF Core.
8. Add tests for length calculations, inventory availability, cost, and cutting-plan correctness.
9. Build reporting and a user interface once the backend flow is stable.

## 11. Useful Commands

Run from the solution root:

```bash
dotnet restore
dotnet build
dotnet test
```

List migrations without changing the database:

```bash
dotnet ef migrations list \
  --project src/CuttingOptimizer.Infrastructure \
  --startup-project src/CuttingOptimizer.Api
```

## Summary

The project has a Clean Architecture solution, core domain entities, and code for several API features. The latest build succeeds. The next priority is to run tests, verify migration/database alignment, and exercise the API before beginning the cutting optimization engine—the feature that will provide the project's main business value.

This document records the latest reported project state as of 17 October 2026. It distinguishes code that compiles from features that have been tested at runtime.
