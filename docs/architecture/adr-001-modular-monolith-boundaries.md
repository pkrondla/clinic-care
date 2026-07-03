# ADR-001: Modular Monolith Slice Boundaries

## Status

Accepted

## Context

HomoeoDesk is a multi-tenant SaaS platform for homoeopathy clinic management. The control plane (tenant registry, subscriptions, global catalog) and tenant operations (patients, appointments, billing) must remain independently deployable slices per stamp topology.

## Decision

1. **Tenant slice must not reference global slice projects.**
2. **Global slice must not reference tenant slice projects.**
3. **API projects are composition roots only** — Minimal API modules dispatch to MediatR; no business logic in endpoints.
4. **Short-term duplication is allowed** when it preserves boundaries during extraction from the legacy ClinicCare monolith.
5. **Cross-slice communication** happens via HTTP APIs or provisioning metadata in the global registry — never via direct project references.

## Dependency rules

```
homoeodesk.tenant.api → tenant.application → tenant.domain
homoeodesk.tenant.infrastructure → tenant.application + tenant.domain
homoeodesk.global.api → global.application → global.domain
homoeodesk.global.infrastructure → global.application + global.domain

FORBIDDEN: tenant → global
FORBIDDEN: global → tenant
```

## Consequences

- Two separate API hosts: `homoeodesk.global.api` and `homoeodesk.tenant.api`
- Architecture tests in CI enforce slice boundaries
- Global tenant registry (`GlobalTenant`) stores connection strings for tenant stamp provisioning
