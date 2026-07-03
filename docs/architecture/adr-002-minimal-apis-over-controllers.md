# ADR-002: Minimal APIs Over MVC Controllers

## Status

Accepted

## Context

[PRODUCT_ARCHITECTURE_PROMPT.md](../../PRODUCT_ARCHITECTURE_PROMPT.md) references the ChitStack pattern which uses MVC Controllers. The legacy ClinicCare codebase uses Minimal APIs with module-based endpoint registration (`EndpointsRegistry`, `Modules/{Feature}/{Feature}Endpoints.cs`).

## Decision

HomoeoDesk **keeps Minimal APIs** instead of converting to MVC Controllers.

Endpoint modules remain thin:
- `MapGroup` with route prefix
- Inject `IMediator` and dispatch Commands/Queries
- Use `.RequireAuthorization()` and `.WithOpenApi()`
- Return standardized `{ message, data, errors }` response shapes

## Rationale

- Existing ~25 endpoint modules already follow this pattern correctly
- No functional benefit from controller conversion — both dispatch to MediatR
- Avoids ~1 week of rewrite risk during in-place migration
- Same composition-root-only rule as ChitStack controllers

## Consequences

- ChitStack reference controller examples are not copied verbatim
- API projects use `Endpoints/EndpointsRegistry.cs` + `Modules/` layout
- OpenAPI generation continues via Swashbuckle + minimal API metadata
