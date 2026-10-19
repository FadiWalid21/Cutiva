# Discovery — Resources / Products / Costing (Phase 0)

**Date:** 18 October 2026  
**Branch (current WIP):** `feature/auth-and-premium-ui`  
**Target branch (after WIP landed):** `feature/resources-products-costing`  
**Status:** Discovery complete — modeling decisions locked. No production schema from this doc alone.

---

## 1. WIP split proposal (awaiting Fadi confirm before any commit)

Do **not** commit until Fadi explicitly confirms. Suggested focused commits / PRs on `feature/auth-and-premium-ui`:

| Slice | Scope (approx.) | Suggested commit message |
|---|---|---|
| A — Auth / admin / SaaS | Admin shell, clients, plans, seats, payment, Brevo, forgot-password, signup disabled, subscriptions migrations | `feat: admin clients, plans, seats, and password-reset OTP` |
| B — Quotes + accessories | ClientQuote, Accessories API/UI, section accessories, production price | `feat: client quotes and workshop accessories` |
| C — PWA | manifest, icons, ngsw, install banner/service | `feat: PWA install shell and service worker` |
| D — Landing + polish | landing page, i18n, shell/theme, dashboard remnants polish | `feat: public landing and workshop UI polish` |

**After those land:** create/switch to `feature/resources-products-costing` for Phases 1–6.

---

## 2. Current domain (as implemented)

```text
Workshop
 ├── Project (WorkshopId)
 │    ├── ProjectSection (ProjectId, SectorId, W/H/Qty)
 │    │    ├── RequiredCut (MaterialId, LengthMm, Qty)
 │    │    └── ProjectSectionAccessory (AccessoryId, Qty)
 │    └── ClientQuote (CuttingPlanId?, surcharges)
 ├── Accessory (WorkshopId, Name, UnitPrice, IsActive)
 └── CuttingPlan (WorkshopId, MaterialId) → bars / pieces / unallocated

Sector (global) ← Material (global) ← StockBar (global, LengthMm, UnitPrice, Qty, IsRemnant)
```

### Key entity notes

| Entity | Workshop-scoped? | Notes |
|---|---|---|
| Material | No (global) | `SectorId`, `Code`, `Name`, `Description` — no Category/Thickness |
| Sector | No (global) | Unique `Code`; optional `MaterialId` = default profile. **Not** a physical workshop zone |
| StockBar | No (global) | Catalog + remnants via `IsRemnant` (remnants not keyed by WorkshopId) |
| Accessory | Yes | Priced catalog; already on Materials page |
| Project / CuttingPlan / ClientQuote | Yes (direct or via Project) | |
| Glass | — | **None** in Domain; briefly added as `Material.Kind` then removed (`RemoveGlassFeature`) |

### BR-9

BRD: required cut material must be compatible with section sector.  
**Code:** `CreateRequiredCutHandler` only checks section + material exist — **not enforced**.

**Phase 4 plan:** auto-create generated `ProjectSection`(s) for product-derived cuts; add nullable `SourceProjectItemId` / `SourcePartRole` on `RequiredCut`. Do not tighten the old manual path yet.

### Optimizer

- Path: `ICuttingPlanOptimizer` / `CuttingPlanOptimizer` (FFD, kerf between pieces, remnants preferred)
- Multi-length stock for one material: **yes**
- Alternatives: cost vs waste; recommendations cost-then-waste and waste-then-cost
- **Do not modify** for Resources V2 — feed new `RequiredCut`s only

### Feature flags

None today. Introduce `Features:ResourcesV2` and `Features:ProductTemplates`.

### Frontend surfaces affected

- `/materials` — materials + stock bars + accessories (keep working)
- Project workspace — creates RequiredCuts from section W×H
- Plan generate / detail → approve → quote upsert
- Shell nav (new Resources / Templates routes behind flags)

---

## 3. Modeling recommendation (locked)

| Resource | Decision | Reason |
|---|---|---|
| Aluminum | Extend **Material** + StockBar with `Category` (default Aluminum) + `ThicknessMm?` | Already linear/cuttable; multi-length works |
| Glass | New **GlassType** / **GlassSpec** (workshop-scoped) | m² pricing ≠ bars; Material.Kind glass was rolled back |
| Accessories | Extend **Accessory** with `Brand?` + `Unit` | Already workshop catalog + API |
| Unified Resource table | **Rejected** | Duplicates Accessory, higher risk, ignores glass rollback |

**Sector:** leave alone; Resources UI tabs are Aluminum \| Glass \| Accessories, not Sector-driven.

**Tenancy for new Glass:** workshop-scoped (like Accessory). Aluminum Material stays global (as today).

**Open defaults (section 7 of feature brief):** inspection = measure + notes + status; templates editable by Admin + WorkshopOwner; exact m²; cost then waste; placeholder formulas; no tax/currency.

---

## 4. Endpoints / handlers to extend (later phases)

| Area | Today | Phase |
|---|---|---|
| Materials / StockBars | `/api/materials`, `/api/stock-bars` | 1 — Category, ThicknessMm |
| Accessories | `/api/workshops/{id}/accessories` | 1 — Brand, Unit |
| Glass | — | 1 — new CRUD |
| Product templates | — | 2 |
| Project items | — | 3 |
| Calculate | — | 4 — `POST /api/projects/{id}/calculate` |
| Quotes | `/api/quotes` | 5 — glass/accessory lines from snapshot |

---

## 5. Smoke test checklist (current flow)

Record result before Phase 1. Environment: API `http://localhost:5216`, Angular `http://localhost:4200`, Postgres via `docker compose`.

| # | Step | Pass? | Notes |
|---|---|---|---|
| 1 | Login as workshop owner (or admin-created client) | ☐ | |
| 2 | Open Materials: list materials + stock bars | ☐ | |
| 3 | Create/open a Project | ☐ | |
| 4 | Add ProjectSection with W×H×Qty (generates RequiredCuts) | ☐ | |
| 5 | Generate cutting plan (alternatives visible) | ☐ | |
| 6 | Open plan detail; Approve | ☐ | |
| 7 | Open/create Client Quote for approved plan | ☐ | |
| 8 | Quote shows material + accessories + totals | ☐ | |

**Result:** _Not executed in Phase 0 doc authoring — run manually and tick above before merging Phase 1._

Commands:

```bash
dotnet build
dotnet test
cd frontend/cutiva-web && npm run build
```

---

## 6. Phase exit criteria

Phase 0 done when:

- [x] Discovery written (this file)
- [x] WIP split proposed (section 1) — commits still need Fadi OK
- [x] Smoke checklist recorded (section 5)
- [x] `feature/resources-products-costing` created
- [ ] Fadi OK to **apply** migration `AddResourcesProductsAndCosting` to DB
- [ ] Fadi OK to remove feature flags after workshop validation

---

## 7. Migration caution

```bash
dotnet ef migrations list \
  --project src/CuttingOptimizer.Infrastructure \
  --startup-project src/CuttingOptimizer.Api
```

Migration added (not applied): `20261018145459_AddResourcesProductsAndCosting` — Material Category/Thickness/IsActive, Accessory Brand/Unit, Glass*, Product*, ProjectItem*, RequiredCut source columns. **Never apply without Fadi approval.**

---

## 8. Implementation status (Phases 1–6 code)

| Phase | Status |
|---|---|
| 1 Resources | Done (API + `/resources` UI behind flag) |
| 2 Templates | Done (seed + `/product-templates` UI behind flag) |
| 3 Project items | Done (wizard in project workspace) |
| 4 Calculator | Done (`ProjectCostCalculator` + tests + calculate endpoint) |
| 5 Results + Quote | Done (calc results UI; quote glass/accessory from snapshots) |
| 6 Harden / docs | Docs updated; **flags kept** until explicit OK |

---

## 9. Edge cases (hardening notes)

| Case | Behavior |
|---|---|
| Inactive material/glass/accessory | Rejected on create/update item and on calculate (active-only validation) |
| Price/template changed after calculate | Old project keeps `CalculationSnapshotJson` until user re-calculates |
| Recalculate | Deletes only RequiredCuts with `SourceProjectItemId`; manual cuts untouched |
| Formula length ≤ 0 | Validation error — no negative pieces |
| Insufficient / short stock | Unallocated pieces returned in calculate result (same as optimizer) |
| Shared aluminum across items | Cost allocated by piece-length share (waste included proportionally); rule shown in calculate UI |
| Aluminum-only door | No glass slots on seeded variant |
| Feature flags off | Old `/materials` + manual section flow remain the only path |

---

## 10. V2 smoke checklist (after migration applied)

| # | Step | Pass? |
|---|---|---|
| 1 | Dev flags on: open Resources (3 tabs), add glass type/spec | ☐ |
| 2 | Open Product templates; edit a placeholder offset | ☐ |
| 3 | Project: add sliding window item + door item with per-role materials | ☐ |
| 4 | Calculate → purchase bars, glass, accessories, per-item costs | ☐ |
| 5 | Open quote → glass + accessory lines from snapshots | ☐ |
| 6 | Old flow: section → cuts → plan → approve → quote still works | ☐ |
