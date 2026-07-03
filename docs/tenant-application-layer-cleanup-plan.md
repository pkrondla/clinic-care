# Tenant application-layer cleanup: 5 targeted fixes

## Context

A deep-dive into `homoeodesk.tenant.application` surfaced five recurring anti-patterns: business logic wrongly embedded/coupled in the wrong handler, no input validation despite FluentValidation being registered but unused, duplicated inline authorization checks, "command calling command" chains through MediatR, and a documented no-op pipeline behavior sitting where real tenant-safety logic should be. None of these are bugs users can see today, but they're the kind of gaps that turn into real incidents (silent cross-tenant writes, bad prescription data reaching inventory, inconsistent 403s) as the product grows past its current single-tenant-per-stamp deployment model. Each fix below is independent and can be shipped/reviewed separately.

All file paths are relative to the repo root (`homoeodesk.tenant/...`).

---

## Implementation status

| # | Item | Status |
|---|------|--------|
| 1 | Prescription → Inventory decoupling via domain events | **In progress** — see checklist below |
| 2 | FluentValidation validators + `ValidationBehaviour` | Not started |
| 3 | Shared `EnsureRole` guard for Users handlers | Not started |
| 4 | Remove command-calling-command chains | Not started |
| 5 | `TenantService` fail-closed + `TenantBehaviour` fail-fast gate | Not started |

### Item 1 checklist (in progress)

- [x] `homoeodesk.tenant.domain/Modules/Prescriptions/Events/PrescriptionCreatedEvent.cs` created — carries a `Prescription` entity reference (not a copied `int`), exposing `PrescriptionId => Prescription.Id` computed lazily so it reflects the ID assigned by EF Core after save.
- [x] `AppointmentCreatedEvent.cs` fixed the same way — it previously captured `appointment.Id` at construction time, before the entity was ever saved, so it would always carry `AppointmentId = 0` once dispatch was wired up. Now holds the `Appointment` entity and reads `Id` lazily.
- [x] `Appointment.Create()` (`homoeodesk.tenant.domain/Modules/Appointments/Entities/Appointment.cs`) updated to pass the entity itself (`new AppointmentCreatedEvent(appointment)`) instead of `appointment.Id`.
- [x] `TenantDbContext` constructor now takes `MediatR.IPublisher` (no new package reference needed — `homoeodesk.tenant.infrastructure` already project-references `homoeodesk.tenant.application`, which pulls in `MediatR` 13.0.0 transitively).
- [ ] `TenantDbContext.SaveChangesAsync` does **not yet** dispatch domain events — the field/constructor wiring is done, but the dispatch loop (iterate `ChangeTracker.Entries<BaseEntity>()`, publish via `_publisher.Publish(...)`, then `ClearDomainEvents()`) still needs to be added after the `base.SaveChangesAsync(...)` call.
- [ ] `CreatePrescriptionHandler.cs` still has the original inline `DeductStockFromInventoryAsync` method and its silent `catch (Exception ex) { }` — not yet refactored to raise `PrescriptionCreatedEvent` instead.
- [ ] New `DeductInventoryOnPrescriptionCreatedHandler.cs` (under `homoeodesk.tenant.application/Features/Prescriptions/Events/`) not yet created.

**Net effect right now:** the code still compiles and behaves exactly as before (stock deduction still happens inline in `CreatePrescriptionHandler`) — none of the new event plumbing is wired into the actual save/dispatch path yet, so this is safe to pause on.

---

## 1. Decouple Prescription creation from Inventory deduction via domain events

**Problem:** `CreatePrescriptionHandler.cs` (`homoeodesk.tenant.application/Features/Prescriptions/Commands/CreatePrescription/CreatePrescriptionHandler.cs`, handler lines 35–172) calls a private `DeductStockFromInventoryAsync` (lines 177–312) directly, wrapped in a bare `catch (Exception ex) { }` that swallows every failure with no logging. Domain events already exist as infrastructure (`BaseEntity.DomainEvents`, `homoeodesk.tenant.domain/Common/BaseEntity.cs`) but were never dispatched anywhere — `TenantDbContext.SaveChangesAsync` stamps timestamps but never publishes `entry.Entity.DomainEvents`. `Appointment`'s events (`AppointmentCreatedEvent` etc.) are raised into a list that nothing ever reads.

**Remaining steps:**
1. In `TenantDbContext.SaveChangesAsync`, after `var result = await base.SaveChangesAsync(cancellationToken);`, add a `DispatchDomainEventsAsync` step:
   ```csharp
   private async Task DispatchDomainEventsAsync(CancellationToken cancellationToken)
   {
       var entitiesWithEvents = ChangeTracker.Entries<BaseEntity>()
           .Select(e => e.Entity)
           .Where(e => e.DomainEvents.Count > 0)
           .ToList();

       var domainEvents = entitiesWithEvents.SelectMany(e => e.DomainEvents).ToList();
       entitiesWithEvents.ForEach(e => e.ClearDomainEvents());

       foreach (var domainEvent in domainEvents)
           await _publisher.Publish(domainEvent, cancellationToken);
   }
   ```
   Call it after `base.SaveChangesAsync` and before `return result;`. This activates dispatch for *all* existing entities — `Appointment`'s events start firing too, not just the new one.
2. In `CreatePrescriptionHandler`, right after constructing the `prescription` object (before `_context.Prescriptions.Add(prescription)`), call `prescription.AddDomainEvent(new PrescriptionCreatedEvent(prescription));`. Because the event holds the entity reference (not a copied `int`), it will report the correct `Id` once EF Core assigns it during `SaveChangesAsync` — the same fix already applied to `AppointmentCreatedEvent`.
3. Delete the try/catch around `DeductStockFromInventoryAsync` (current lines 135–145) and the entire `DeductStockFromInventoryAsync` method (lines 177–312).
4. Create `homoeodesk.tenant.application/Features/Prescriptions/Events/DeductInventoryOnPrescriptionCreatedHandler.cs` implementing `INotificationHandler<PrescriptionCreatedEvent>` (mirrors the existing stub pattern at `Features/Appointments/Events/AppointmentCreatedHandler.cs`). Move the body of the deleted method here almost verbatim, with two changes:
   - Re-fetch the prescription (with `.Include(p => p.PrescriptionItems)`) by `notification.PrescriptionId` instead of receiving it as a parameter.
   - Use `prescription.OrganizationId` directly for the `StockTransaction.OrganizationId` field instead of `ICurrentUserService.OrganizationId` — the prescription already carries the right tenant, so the handler doesn't need `ICurrentUserService` as a dependency at all.
   - Replace the silent `catch (Exception ex) { }` blocks with `_logger.LogError(ex, "Failed to deduct inventory for prescription {PrescriptionId}", prescriptionId)` so failures are visible instead of invisible.
5. Note: this handler calls `SaveChangesAsync` itself (to persist the `Inventory`/`StockTransaction` writes), which re-enters the dispatch loop in `TenantDbContext` — harmless, since no new events are raised by that second save, so it terminates after one extra no-op pass.

**Files touched:** `TenantDbContext.cs` *(constructor/field done, dispatch method pending)*, `PrescriptionCreatedEvent.cs` *(done)*, `AppointmentCreatedEvent.cs` + `Appointment.cs` *(done — latent zero-ID bug fixed as a side effect of activating dispatch)*, new `DeductInventoryOnPrescriptionCreatedHandler.cs` *(pending)*, `CreatePrescriptionHandler.cs` *(pending)*.

---

## 2. Add FluentValidation validators (package already referenced, zero validators exist)

**Problem:** `FluentValidation` + `FluentValidation.DependencyInjectionExtensions` (v11.11.0) are already in both `.csproj` files and `AddValidatorsFromAssembly` is already called in `homoeodesk.tenant.application/DependencyInjection.cs:17`, but a repo-wide glob for `*Validator.cs` returns nothing — there is currently zero validation coverage, and no `ValidationBehaviour` pipeline exists to enforce it even if validators were added. Handlers do partial, inconsistent manual checks instead (e.g. `CreatePrescriptionHandler` never checks `Medicines` is non-empty or that `Quantity > 0`; `CreateInvoiceHandler` never guards negative price/quantity).

**Plan:**
1. Add `ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>` in `homoeodesk.tenant.application/Common/Behaviours/` (alongside the existing `TenantBehaviour.cs`), resolving `IEnumerable<IValidator<TRequest>>`, running them, and throwing `FluentValidation.ValidationException` on failure — this maps automatically to a clean 400 response because `ExceptionMiddleware.cs` (`homoeodesk.tenant.api/Middleware/ExceptionMiddleware.cs:46-56`) already has a dedicated `case ValidationException` branch producing field-level error details. No middleware changes needed.
2. Register it in `DependencyInjection.cs` alongside `TenantBehaviour<,>` (order: validation before tenant behaviour).
3. Write validators for the three highest-value commands first (confirmed gaps):
   - `CreatePrescriptionCommand`: non-empty `Medicines`, and per-`PrescriptionMedicineDto` — required `MedicineName`/`Dosage`/`Frequency`, `Quantity > 0`, `ContainerSize > 0` when `DispensingForm` is Globules.
   - `CreateInvoiceCommand`: non-empty `Items`, non-negative `ConsultationAmount`/`MedicineAmount`/`CourierCharges`, and per-`InvoiceItemCommand` — `Quantity > 0`, `UnitPrice >= 0`.
   - `CreatePurchaseOrderCommand` already has the best `DataAnnotations` coverage (`[Range]` on item quantity/price) — lowest priority; add a validator only for the `Items` non-empty / `SupplierId` positive checks that DataAnnotations don't express well.
4. Leave existing DB-lookup business-rule checks (e.g. "clinic not found", "medicine count mismatch" in `CreatePurchaseOrderHandler`) in the handlers — FluentValidation only replaces shape/format checks, not checks requiring a database round trip.

**Files touched:** new `ValidationBehaviour.cs`, `DependencyInjection.cs`, 2–3 new `*Validator.cs` files under the respective `Commands/Create*/` folders.

---

## 3. Extract duplicated Admin-only role checks into a shared guard

**Problem:** Five handlers in `Features/Users/` — `GetUsersHandler`, `GetUserHandler`, `CreateUserHandler`, `UpdateUserHandler`, `DeleteUserHandler`, `AssignBranchAccessHandler` — each hand-roll the identical `if (currentUserRole != UserRole.Admin) return Result.Failure(...)` block, even though `ICurrentUserService` already exposes unused `IsInRole(UserRole)`/`HasAnyRole(params UserRole[])` helpers. Separately, `CancelAppointmentHandler.CheckCancelPermission` does real ownership-based authorization (Doctor/Patient can only act on their own record via a DB lookup) — not a candidate for a pure claims-based ASP.NET Core policy, so a full policy-based rebuild is not worth building for one bespoke case (there are currently zero role-based `AddAuthorization` policies in the repo — only CORS policies).

**Plan (handler-level guard, not ASP.NET Core policies):**
1. Add an extension method `CurrentUserServiceExtensions.EnsureRole(this ICurrentUserService, UserRole required)` that throws `UnauthorizedAccessException` if `!currentUser.IsInRole(required)`. This maps automatically to 401 via the existing `ExceptionMiddleware` `case UnauthorizedAccessException` branch — no new error-handling plumbing needed.
2. Replace the duplicated 3-line block in each of the 5 `Users` handlers with a single `_currentUserService.EnsureRole(UserRole.Admin);` call at the top of `Handle`.
3. Leave `CancelAppointmentHandler` structurally as-is, but tidy its Admin/Staff bypass check to use the existing `HasAnyRole(...)` helper instead of raw `==`/`||` comparisons, for consistency with the new convention.

**Files touched:** new extension method file, 5 `Users` handlers, minor tidy in `CancelAppointmentHandler.cs`.

---

## 4. Remove "command calling command" chains

**Problem:** Two flavors of the same anti-pattern, confirmed at 7 call sites:
- **Write-triggers-write via MediatR:** `ProcessPaymentWebhookHandler` builds a `PayInvoiceCommand` and does `await _mediator.Send(payCommand, ...)` instead of calling shared logic directly.
- **Read-your-own-write via MediatR:** `PayInvoiceHandler`, `ReceivePurchaseOrderHandler`, `CreatePurchaseOrderHandler`, `ApprovePurchaseOrderHandler`, `CancelPurchaseOrderHandler`, `UpdateCourierDocketHandler` all re-fetch their own response DTO by calling `_mediator.Send(new Get*Query(...))` right before returning, instead of building the DTO locally.
- **Bypasses MediatR entirely:** `UpdateNotificationPreferencesHandler` does `new GetNotificationPreferencesHandler(_context, _currentUserService)` and calls `.Handle(...)` manually — worse than the MediatR round-trip pattern above because it isn't even going through DI.

**Plan:**
1. Extract the DTO-mapping body of `GetInvoiceHandler` (pure/stateless — no side effects) into a plain scoped service `IInvoiceReadService.GetInvoiceDtoAsync(int invoiceId, int organizationId, CancellationToken)`. Both `GetInvoiceHandler` and `PayInvoiceHandler` call this directly instead of one calling the other through MediatR.
2. Extract the *write* side of `PayInvoiceHandler` (amount validation, status transition, notification) into `IInvoicePaymentService.ApplyPaymentAsync(int invoiceId, decimal amount, string method, string? reference, CancellationToken)`. `PayInvoiceHandler` becomes a thin MediatR entry point calling this service; `ProcessPaymentWebhookHandler` calls the same service directly instead of building and sending a `PayInvoiceCommand`.
3. Apply the identical "extract to plain service, call directly" fix to the 4 PurchaseOrder handlers' repeated `Get*Query` re-fetch and `UpdateCourierDocketHandler`.
4. Extract `GetNotificationPreferencesHandler`'s body (also pure/stateless) into `INotificationPreferencesReadService.GetPreferencesAsync(...)`. Both `GetNotificationPreferencesHandler` and `UpdateNotificationPreferencesHandler` call it directly — deletes the raw `new Handler(...).Handle(...)` entirely.
5. While touching these files: `InitiateOnlinePaymentHandler` and `PerformStockAuditHandler` both inject `IMediator` but never call `.Send(...)` anywhere — remove the unused constructor dependency from each.

**Files touched:** `GetInvoiceHandler.cs`, `PayInvoiceHandler.cs`, `ProcessPaymentWebhookHandler.cs`, 4 PurchaseOrder handlers + their matching `Get*Handler`s, `UpdateCourierDocketHandler.cs`, `GetNotificationPreferencesHandler.cs`, `UpdateNotificationPreferencesHandler.cs`, `InitiateOnlinePaymentHandler.cs`, `PerformStockAuditHandler.cs`, plus new `IInvoiceReadService`/`IInvoicePaymentService`/`INotificationPreferencesReadService` interfaces + implementations registered in `DependencyInjection.cs`.

---

## 5. Make `TenantBehaviour` a real fail-fast gate (root cause is in `TenantService`, not the behaviour itself)

**Problem:** `TenantBehaviour<TRequest,TResponse>` is a literal `next()` pass-through — its own doc comment says tenant scoping is handled by EF query filters instead. That's true as far as it goes (repo-wide grep found zero `IgnoreQueryFilters`/raw-SQL bypasses of the filtered `DbSet`s). But `TenantService.ResolveTenantId()` (`homoeodesk.tenant.infrastructure/Services/TenantService.cs:33-73`) revealed the actual gap the behaviour should be guarding against: **when tenant resolution fails, it silently defaults to `TenantId = 1` / subdomain `"demo"`** — both when there's no `HttpContext` and when an authenticated request has no `TenantId`/`OrganizationId` claim and no `X-Tenant-Id` header. In true multi-tenant-per-process scenarios (as opposed to today's fixed-tenant-per-stamp deployment, correctly handled by the `EnableFixedTenant` branch), this is a fail-*open* bug: a misconfigured JWT or missing claim doesn't error, it silently attributes the request to tenant 1's database rows.

**Plan:**
1. In `TenantService.ResolveTenantId()`, replace both silent-default branches with `throw new UnauthorizedAccessException("Unable to resolve tenant for this request.")` — safe because `ExceptionMiddleware` already has a dedicated `UnauthorizedAccessException` → 401 branch, so the failure mode changes from "silently correct-looking wrong data" to "clean 401". The `EnableFixedTenant` branch (correct for today's real deployment model) is untouched.
2. Repurpose `TenantBehaviour` from a no-op into an explicit, early fail-fast point: call `await tenantService.GetTenantIdAsync()` at the top of `Handle`, before `next()`. This forces resolution (and thus the new exception, if any) to happen at the very start of the MediatR pipeline for every request, rather than implicitly and lazily wherever a query filter first touches `_tenantService.TenantId` deep in a handler.
3. Update the class doc comment to describe the new behavior instead of the old "this is intentionally a no-op" note.

**Files touched:** `TenantService.cs`, `TenantBehaviour.cs`.

---

## Verification

No test projects exist in this repo, so verification is manual, per change:

1. `dotnet build HomoeoDesk.sln` after each numbered section — confirms no compile breaks across the DI graph (new services registered correctly, no circular refs between `Infrastructure` and `Application` for the `IPublisher` injection in #1).
2. `dotnet run --project homoeodesk.tenant/homoeodesk.tenant.api`, then exercise via `homoeodesk.tenant.api.http` / Swagger UI (`/swagger` in dev):
   - **#1**: create a prescription with a catalog medicine, confirm `Inventory.CurrentStock` still decrements and a `StockTransaction` row is written — behavior should be unchanged from the user's perspective, just re-routed through the event handler. Force a deduction failure and confirm it now appears in logs instead of vanishing silently.
   - **#2**: `POST /api/prescriptions` with an empty `Medicines` array and with a `Quantity <= 0` item — expect 400 with field-level `details` from `ValidationException`, not a 500 or a silently-created bad record.
   - **#3**: call a `Users` endpoint (e.g. `POST /api/users`) as a non-Admin JWT — expect 401. Confirm Admin JWT still succeeds.
   - **#4**: run the full `POST /api/invoices/{id}/pay` flow and the payment webhook path (`POST /api/payments/webhook`) — confirm invoice status/balance updates identically to before, and that `GetInvoiceHandler`/`PayInvoiceHandler` return identical DTOs pre/post refactor.
   - **#5**: temporarily strip the `TenantId` claim from a test JWT (or call with no `Authorization` header against a non-skip-listed route) — expect a clean 401 instead of a 200 against tenant-1 data. Confirm normal authenticated requests are unaffected.
3. Check `logs/log-.txt` (Serilog file sink) after each manual test to confirm the new logging (event-handler failures, tenant-resolution failures) actually surfaces instead of being swallowed.
