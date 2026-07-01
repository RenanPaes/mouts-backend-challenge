# Developer Evaluation Project

## Challenge Considerations (For Reviewer)

This solution implements the **Sales** domain as a complete CRUD API, built as a vertical slice on top of the provided template and following its existing patterns. Full endpoint documentation is in [Sales API](/.doc/sales-api.md).

**What was implemented**
- **Sales CRUD + cancellation**: create, get, list, update, delete, cancel sale, cancel item (`/api/sales`).
- **Business rules (in the domain)**: 4–9 items → 10% discount, 10–20 → 20%, below 4 → none, above 20 → rejected.
- **DDD & External Identities**: `Sale`/`SaleItem` aggregate with denormalized customer, branch and product data.
- **Design patterns**: CQRS with Mediator (MediatR), Repository, Specification, AutoMapper, FluentValidation.
- **Persistence**: EF Core + PostgreSQL, dedicated mappings and migrations.
- **Domain events** (nice-to-have): `SaleCreated`, `SaleModified`, `SaleCancelled`, `ItemCancelled` published via MediatR and written to the application log.
- **API concerns**: pagination/ordering (`_page`/`_size`/`_order`), standardized `type`/`error`/`detail` error handling, consistent response envelope.
- **Tests**: unit (domain + handlers), integration (repository) and functional (full HTTP) — the last two use Testcontainers, so they require Docker.
- **Process**: Git Flow (`feature/sales-domain`) with semantic commits, one commit per layer.

**How to run**
- `docker compose up` (PostgreSQL) → `dotnet ef database update` → `dotnet run` in `Ambev.DeveloperEvaluation.WebApi` → open `/swagger`.
- Tests: `dotnet test` (integration/functional need Docker running).

---

`READ CAREFULLY`

## Use Case
**You are a developer on the DeveloperStore team. Now we need to implement the API prototypes.**

As we work with `DDD`, to reference entities from other domains, we use the `External Identities` pattern with denormalization of entity descriptions.

Therefore, you will write an API (complete CRUD) that handles sales records. The API needs to be able to inform:

* Sale number
* Date when the sale was made
* Customer
* Total sale amount
* Branch where the sale was made
* Products
* Quantities
* Unit prices
* Discounts
* Total amount for each item
* Cancelled/Not Cancelled

It's not mandatory, but it would be a differential to build code for publishing events of:
* SaleCreated
* SaleModified
* SaleCancelled
* ItemCancelled

If you write the code, **it's not required** to actually publish to any Message Broker. You can log a message in the application log or however you find most convenient.

### Business Rules

* Purchases above 4 identical items have a 10% discount
* Purchases between 10 and 20 identical items have a 20% discount
* It's not possible to sell above 20 identical items
* Purchases below 4 items cannot have a discount

These business rules define quantity-based discounting tiers and limitations:

1. Discount Tiers:
   - 4+ items: 10% discount
   - 10-20 items: 20% discount

2. Restrictions:
   - Maximum limit: 20 items per product
   - No discounts allowed for quantities below 4 items

## Overview
This section provides a high-level overview of the project and the various skills and competencies it aims to assess for developer candidates. 

See [Overview](/.doc/overview.md)

## Tech Stack
This section lists the key technologies used in the project, including the backend, testing, frontend, and database components. 

See [Tech Stack](/.doc/tech-stack.md)

## Frameworks
This section outlines the frameworks and libraries that are leveraged in the project to enhance development productivity and maintainability. 

See [Frameworks](/.doc/frameworks.md)

## API Structure
This section includes links to the detailed documentation for the implemented API resource:
- [Sales API](/.doc/sales-api.md)

<!-- 
The following resources are part of the broader DeveloperStore API definition:
- [API General](/.doc/general-api.md)
- [Products API](/.doc/products-api.md)
- [Carts API](/.doc/carts-api.md)
- [Users API](/.doc/users-api.md)
- [Auth API](/.doc/auth-api.md)
-->

## Project Structure
This section describes the overall structure and organization of the project files and directories. 

See [Project Structure](/.doc/project-structure.md)
