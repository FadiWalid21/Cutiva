# CuttingOptimizer — Business Requirements Document (BRD)

**Document status:** Draft — initial requirements  
**Prepared:** 18 October 2026  
**Product:** CuttingOptimizer  
**Purpose:** Define the business problem, scope, workflows, requirements, and acceptance criteria for the first usable version of CuttingOptimizer.

> **Important:** This BRD is based on the existing project summary and decisions documented so far. Items marked **TBD** require confirmation from real workshop workflows. It does not claim that unverified API behavior or the cutting optimization engine is already implemented.

## 1. Executive Summary

CuttingOptimizer is a system for workshops that cut aluminum profiles and other linear materials into pieces of specified lengths. It is intended to organize workshop projects, materials, stock, and required cuts, then generate cutting plans that reduce material waste and material cost.

The first release will focus on aluminum profiles. The system should establish reliable workshop and inventory data management, then provide a cutting-plan workflow that accounts for stock availability and saw blade kerf.

The initial optimization algorithm is a starting heuristic, not a guarantee of the mathematically optimal plan. Business rules must be validated with workshop users before the optimizer is treated as production-ready.

## 2. Business Problem

Workshops need to cut stock bars into multiple requested lengths. Planning cuts manually can make it difficult to consistently:
- Satisfy every requested piece and quantity.
- Respect the available quantity of each stock length.
- Account for material removed by the saw blade (kerf).
- Understand the leftover length from each stock bar.
- Estimate the material cost required for a job.
- Distinguish pieces that can be produced from current stock from pieces that require additional stock.

CuttingOptimizer aims to make these decisions more consistent, visible, and measurable.

## 3. Business Objectives

1. Organize workshops and their projects.
2. Model project sections, sectors, materials, stock bars, and required cuts.
3. Generate a cutting plan that allocates requested pieces to available stock.
4. Account for saw kerf when determining whether cuts fit on a stock bar.
5. Report stock usage, leftover lengths, unallocated pieces, and estimated material cost.
6. Establish a foundation for later support of reusable offcuts and additional linear-material types.

### Success Measures — TBD

Potential measures to establish with workshop users:
- Percentage of requested pieces successfully allocated.
- Total stock bars used per job.
- Total leftover length and leftover material value.
- Estimated material cost per job.
- Difference between planned and actual material consumption.
- Time required to prepare a cutting plan.

Baseline values and target thresholds have not yet been defined.

## 4. Stakeholders and Users

| Stakeholder | Responsibility |
|---|---|
| Workshop owner / manager | Oversees jobs, material costs, and stock availability. |
| Workshop employee / planner | Enters required cuts and prepares or reviews cutting plans. |
| Inventory manager | Maintains material and stock quantities and prices. |
| System administrator | Manages system configuration and access, if included in the first release. |

**TBD:** Confirm actual user roles, permissions, and whether a single account can manage multiple workshops.

## 5. Product Scope

### 5.1 Initial Scope
- Workshop management.
- Project management within a workshop.
- Sector management.
- Project-section management.
- Material catalog management.
- Stock-bar inventory records.
- Required-cut management.
- Cutting-plan generation.
- Kerf-aware fit calculations.
- Reporting of used stock, leftovers, unallocated pieces, and estimated cost.

### 5.2 Out of Scope for the Initial Release

Unless explicitly approved later:
- Reusing offcuts as inventory for subsequent jobs.
- Automatically consuming or reserving stock when a plan is generated.
- A globally optimal solver guaranteeing the best possible plan.
- Support for every material type from day one.
- Purchasing, supplier management, invoicing, accounting integrations.
- Mobile applications and advanced reporting dashboards.
- Authentication, subscription plans, and multi-tenant behavior beyond what the existing codebase already supports.

These exclusions must be revisited if current implementation or business needs require them.

## 6. Domain Concepts

The following concepts are reflected in the current project summary.

### 6.1 Workshop
Represents a workshop and contains projects.
- `Id`
- `Name`

### 6.2 Project
Represents a job or project belonging to a workshop.
- `Id`
- `WorkshopId`
- `Name`
- Optional `CustomerName`

A project contains sections.

### 6.3 Sector
Represents a sector or category used to group related project sections and materials.
- `Id`
- `Code`
- `Name`

The exact business meaning of a sector and its relationship to a workshop should be confirmed with users.

### 6.4 ProjectSection
Represents a section within a project and references a sector.
- `Id`
- `ProjectId`
- `SectorId`
- `Name`

A section contains required cuts.

### 6.5 Material
Represents a material/profile type associated with a sector.
- `Id`
- `SectorId`
- `Code`
- `Name`
- Optional `Description`

A material has stock records and can be referenced by required cuts.

### 6.6 StockBar
Represents an inventory record for stock of a particular material, length, price, and quantity. It does **not** currently represent an individually serialized physical bar.
- `Id`
- `MaterialId`
- `LengthMm`
- `UnitPrice`
- `AvailableQuantity`
- `IsActive`

### 6.7 RequiredCut
Represents a requested piece to be cut for a project section from a material.
- `Id`
- `ProjectSectionId`
- `MaterialId`
- `Name`
- `LengthMm`
- `Quantity`

The current project summary describes validation for positive lengths and quantities. The application also checks that a required cut's material belongs to the same sector as its project section.

## 7. Core Business Workflow

1. Create or select a workshop.
2. Create a project for the workshop.
3. Define or select the relevant sector.
4. Create project sections and associate them with sectors.
5. Create or select materials in the relevant sectors.
6. Record available stock-bar lengths, prices, and quantities.
7. Enter required cuts and quantities for each project section.
8. Request a cutting plan for the relevant material and required cuts.
9. Review the proposed allocation, kerf usage, leftover lengths, stock usage, cost, and any unallocated pieces.
10. Approve the plan operationally outside the system until a stock-reservation/consumption workflow is explicitly implemented.

**TBD:** Confirm whether a cutting plan is generated for one project section, an entire project, or a selected set of sections. Confirm whether multiple materials can be optimized in one request.

## 8. Functional Requirements

Priority definitions: **P0** = required for the initial usable release; **P1** = important follow-up; **P2** = future enhancement.

### FR-01 — Workshop Management (P0)
The system shall support creating, reading, updating, and deleting workshops, subject to existing relationship constraints.

**Acceptance criteria**
- A workshop can be created with valid required data.
- A saved workshop can be retrieved.
- Workshop details can be updated.
- Deletion follows defined relationship rules and does not silently remove protected related data.

### FR-02 — Project Management (P0)
The system shall support project creation and retrieval, with update and delete operations as provided by the current API.

**Acceptance criteria**
- A project references an existing workshop.
- Project details can be retrieved and updated.
- Invalid workshop references are rejected.
- Deletion follows relationship constraints.

### FR-03 — Sector and Project-Section Management (P0)
The system shall support sector CRUD and project-section creation and retrieval.

**Acceptance criteria**
- Sector codes are unique.
- A project section references an existing project and sector.
- Invalid references are rejected.
- Relationship constraints are respected.

### FR-04 — Material Management (P0)
The system shall support creating and retrieving materials, including an optional sector filter when listing materials.

**Acceptance criteria**
- A material references a valid sector.
- Material codes are unique within a sector.
- Invalid or duplicate material codes are rejected.

### FR-05 — Stock-Bar Management (P0)
The system shall support creating, listing, updating, and deactivating stock-bar records.

Known endpoints from the project summary:
- `POST /api/stock-bars`
- `GET /api/stock-bars`
- `PUT /api/stock-bars/{id}`
- `PATCH /api/stock-bars/{id}/deactivate`

The list operation may filter by `materialId`, `sectorId`, and `isActive`.

**Acceptance criteria**
- Stock length is greater than zero.
- Unit price is not negative.
- Available quantity is not negative.
- Inactive stock is excluded from optimization.
- The optimizer never allocates more bars than the available quantity.

### FR-06 — Required-Cut Management (P0)
The system shall support creating, listing, updating, and deleting required cuts.

**Acceptance criteria**
- Cut length and quantity are greater than zero.
- Each cut references a valid project section and material.
- The material sector matches the project section sector.
- The optimizer accounts for the requested quantity of each cut.

### FR-07 — Cutting-Plan Generation (P0)
The system shall generate a proposed cutting plan for a defined set of required cuts and compatible stock.

**Acceptance criteria**
- Every allocated piece corresponds to a requested piece.
- No piece is allocated more than the requested quantity.
- Every allocated piece fits within its stock bar after accounting for kerf.
- Only active stock records with available quantity may be allocated.
- Unallocated demand is explicitly reported; it must not be silently omitted.
- Generating a plan does not itself consume or reserve inventory.

**TBD:** Confirm whether the plan should use persisted RequiredCut records, request-supplied cuts, or support both.

### FR-08 — Kerf Calculation (P0)
The system shall account for the length removed by the saw blade.

**Proposed initial convention:** For a bar with `n` pieces, charge kerf between consecutive pieces, resulting in `max(n - 1, 0)` kerf gaps. Confirm this against the workshop's cutting process and machine behavior before production use.

**Acceptance criteria**
- Kerf is explicit and non-negative.
- Pieces that exceed usable stock length after kerf are not allocated.
- Remaining length is calculated consistently using the agreed convention.
- One-piece and exact-fit cases produce consistent results.

### FR-09 — Cost and Leftover Reporting (P0)
The system shall report:
- Number of stock bars used.
- Allocation of pieces to each stock bar.
- Total kerf length.
- Remaining length per bar.
- Total leftover length.
- Estimated material cost.
- Any unallocated pieces.

**TBD:** Confirm whether cost is charged per stock bar, whether partial stock can be purchased, and how currency and tax are handled. The currency of current `UnitPrice` is not specified in the project summary.

### FR-10 — Optimization Strategy (P0)
The first version may use a deterministic greedy heuristic such as First-Fit Decreasing (FFD).

**Acceptance criteria**
- Pieces are considered in descending length order.
- The algorithm respects stock quantities and kerf.
- The same inputs and rules produce a reproducible plan.
- The response makes clear that a heuristic does not guarantee a globally optimal result.
- Cost and leftover metrics are reported so future strategies can be compared.

### FR-11 — Inventory Mutation (P1)
The system may later support stock reservation, consumption, and offcut creation. These operations are excluded from the first optimizer release. Plan generation must not change inventory unless a separate, explicitly approved workflow is implemented.

### FR-12 — Error Handling and Validation (P0)
The system shall reject invalid inputs and return consistent error responses using the existing validation and exception-handling configuration.

Examples:
- Non-positive piece or stock lengths.
- Negative kerf.
- Negative unit price or stock quantity.
- Invalid material or section references.
- Incompatible material/sector combinations.

## 9. Non-Functional Requirements

### NFR-01 — Architecture
The optimizer shall be independent of controllers and EF Core persistence. Business logic should follow the existing project conventions and reside in appropriate layers.

### NFR-02 — Correctness
All length and kerf calculations shall use millimeters. Numeric types should avoid unintended rounding in length calculations.

### NFR-03 — Data Integrity
Respect existing foreign keys, unique constraints, validation rules, and restrictive delete behavior.

### NFR-04 — Reliability
The API shall not report a fully successful plan if some required pieces could not be allocated.

### NFR-05 — Security and Configuration
Credentials and connection strings shall remain in environment configuration or .NET User Secrets and shall not be committed to source control.

### NFR-06 — Performance (TBD)
Expected maximum numbers of stock records, required pieces, and response-time targets have not been specified. Establish these before choosing a more advanced optimization algorithm.

### NFR-07 — Observability
Operational errors should be diagnosable through existing logging and exception handling without exposing secrets.

## 10. Business Rules

1. All cutting lengths are measured in millimeters.
2. Required piece lengths must be positive.
3. Required quantities must be positive.
4. Stock lengths must be positive.
5. Available stock quantity cannot be negative.
6. Unit price cannot be negative.
7. Inactive stock is not eligible for a new plan.
8. Allocated bars cannot exceed available quantities.
9. A required cut's material must be compatible with its project section's sector.
10. Kerf must be included according to the agreed convention.
11. Unallocated demand must be reported explicitly.
12. Generating a plan is a calculation only; it does not reserve or consume inventory in the initial release.
13. Offcut reuse is deferred until a separate inventory policy is agreed.
14. The initial heuristic does not guarantee a globally optimal result.

## 11. Illustrative Example

Assume the workshop requests:

| Piece length | Quantity |
|---:|---:|
| 2,500 mm | 2 |
| 1,800 mm | 2 |
| 1,200 mm | 1 |

Available stock bars have a length of 6,000 mm.

The optimizer should attempt to allocate all five pieces while accounting for configured kerf and available stock quantities. It should report selected bars, assigned pieces, kerf, leftover lengths, cost, and any pieces that cannot be allocated.

This is an illustrative scenario only; no particular allocation or savings claim is asserted.

## 12. API and Integration Requirements

- Follow current API route and response conventions.
- Use existing CQRS/MediatR and validation patterns where appropriate.
- Keep the core optimization algorithm as a pure service.
- Register dependencies through the existing dependency injection configuration.
- Avoid unnecessary database schema changes.
- Do not apply migrations without reviewing the migration list and database schema.
- Keep generated plans separate from inventory mutation.

The exact cutting-plan endpoint, request/response DTOs, and persisted-versus-request-supplied input model remain implementation decisions to confirm against the codebase.

## 13. Assumptions and Open Questions

These items require business confirmation:

1. Is the first customer workflow aluminum windows and doors, or a broader aluminum fabrication process?
2. What does `Sector` mean in the workshop's terminology?
3. Is kerf constant per machine, configurable per plan, or configurable per material/profile?
4. Should kerf be charged between pieces only, or also at the start/end of a bar based on machine practice?
5. Are stock lengths and prices fixed per material record, or can multiple stock options exist for one material?
6. Is optimization run per section, project, or material batch?
7. What takes priority: minimum cost, minimum waste, minimum bars, or a weighted combination?
8. Is leftover material waste, reusable inventory, or both depending on a minimum reusable length?
9. Which currency is used for `UnitPrice`? Are taxes or purchase costs included?
10. Should users be able to override a generated plan manually?
11. What should happen if there is insufficient stock to satisfy all required pieces?
12. What are the expected maximum job size and acceptable response time?
13. Are authentication, user roles, and multiple workshops per account required for the MVP?
14. Should stock be reserved or consumed only after an authorized user approves a plan?

## 14. Delivery Phases

### Phase 1 — Stabilize Existing Backend
- Verify migrations and database alignment.
- Resolve outstanding dependency vulnerability warnings.
- Start the API and manually exercise existing endpoints.
- Automated tests are currently deferred by the project owner's instruction.

### Phase 2 — First Cutting Optimizer
- Confirm kerf and cost rules.
- Define request/response contracts.
- Implement a pure heuristic engine.
- Add the application use case and API endpoint.
- Build the solution and manually validate representative scenarios.

### Phase 3 — Operational Workflow

Phase 3 is split so persistence and dashboard metrics can ship before inventory mutation.

#### Phase 3a — Persist plans and workshop dashboard (current)

- Persist each generated cutting plan as a snapshot (`CuttingPlan` with bars, pieces, and unallocated pieces).
- Generation still does **not** consume or reserve stock (FR-11 remains).
- Store length totals used for KPIs:
  - `TotalPieceLengthMm` — sum of allocated piece lengths
  - `TotalStockLengthMm` — sum of lengths of physical bar instances used
  - Identity: `TotalStockLengthMm = TotalPieceLengthMm + TotalKerfMm + TotalLeftoverMm`
- Workshop dashboard endpoint aggregates over a **recent window**:
  - **Recent window:** plans with `CreatedAtUtc >= UtcNow - 30 days`
  - **KPIs:** sum all plans in that 30-day window (DB-side `Sum` projections; do not load full plan graphs)
  - **Material efficiency** = `sum(TotalPieceLengthMm) / sum(TotalStockLengthMm)` (API exposes as percent 0–100)
  - **Waste %** = `sum(TotalKerfMm + TotalLeftoverMm) / sum(TotalStockLengthMm)` (API exposes as percent 0–100; equals `1 - efficiency` when stock > 0)
  - **Stock bars used** = `sum(TotalBarsUsed)`
  - **Estimated cost / savings proxy** = `sum(TotalMaterialCost)` as a bare decimal (no currency code; currency model TBD)
  - Guard: if `sum(TotalStockLengthMm) == 0`, efficiency and waste are `0`
  - **Recent projects list:** up to the **20** most recent distinct projects touched by plans in the 30-day window
- **Projected leftovers** (API field: `projectedLeftovers`): bar remainders from saved plans with `RemainingLengthMm > 0`. These are **not** inventoriable stock remnants. UI copy should say “projected leftover” / “متوقع من الخطة”.
- Index `(WorkshopId, CreatedAtUtc)` on `CuttingPlans` supports dashboard filters.

#### Phase 3b — Approve / consume (later)

- Add plan review and approval if required.
- Define inventory reservation/consumption rules.
- Track actual material usage and leftovers as real inventory changes.
- Only after approve/consume may leftovers become inventoriable remnants (Phase 4).

### Phase 4 — Improve Optimization
- Compare heuristic results against alternative algorithms.
- Add reusable offcuts only after defining eligibility and traceability.
- Establish performance and optimization-quality benchmarks.

### Phase 5 — Expand Product Scope
- Consider other linear materials, advanced reports, user roles, and integrations based on validated demand.

## 15. Definition of Done for the First Optimizer Release

The first optimizer release is ready for manual evaluation when:
- The API accepts a valid cutting-plan request.
- Invalid inputs are rejected with clear errors.
- All allocated pieces fit within assigned stock after kerf.
- Stock quantities are never exceeded.
- Inactive stock is never used.
- Unallocated pieces are clearly reported.
- Per-bar and aggregate leftover, kerf, bar-count, and cost metrics are internally consistent.
- Plan generation does not mutate inventory.
- The solution builds successfully.
- Representative scenarios have been manually checked.

Automated test execution is intentionally excluded from the current work sequence, but should be scheduled before production deployment.

## 16. Document Maintenance

Update this BRD when workshop rules are confirmed or scope changes. Keep a clear distinction between:
- **Confirmed requirements** — explicitly agreed by the project owner or verified from existing project behavior.
- **Proposed requirements** — recommendations awaiting approval.
- **Implementation status** — code written, build-verified, and runtime-verified should be tracked separately.

**End of document.**
