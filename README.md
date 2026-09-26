# Reservation System

[!\[Build and deploy](https://github.com/AleksanderKufel/dotnet-reservation-system/actions/workflows/main_reservation-system-api-dev.yml/badge.svg)](https://github.com/AleksanderKufel/dotnet-reservation-system/actions/workflows/main\_reservation-system-api-dev.yml)

REST API for booking appointments with specialists. The main requirement is that a specialist can't be double-booked, even under concurrent requests.

**Stack:** .NET 10, ASP.NET Core Web API, EF Core, PostgreSQL, ASP.NET Core Identity, FluentValidation, xUnit, Moq, Docker, GitHub Actions, Azure App Service

## Project structure

* `Domain` - `Reservation` entity, cancellation rules, overlap checking
* `Application` - `ReservationService` and repository / unit of work interfaces
* `Infrastructure` - EF Core, repositories, migrations, Identity store
* `API` - controllers, validation, exception handling (ProblemDetails)
* `UnitTests`, `IntegrationTests`

## Running locally

```bash
cd ReservationSystem
docker compose up --build

# in a second terminal - apply migrations
dotnet ef database update --project ReservationSystem.Infrastructure --startup-project ReservationSystem.API
```

Swagger: http://localhost:8080/swagger

Tests (integration tests need the database from docker compose):

```bash
dotnet test
```

## Endpoints

* `POST /register`, `POST /login` - Identity API endpoints, login returns a bearer token
* `POST /api/reservations` - create a reservation (requires token)
* `GET /health`

Errors are returned as ProblemDetails. An overlapping or concurrently booked slot returns `409 Conflict`.

## Double booking

Checking for overlaps and inserting the reservation run in a single `Serializable` transaction. With Read Committed, two requests could both pass the check and both insert. A row lock won't help here, because the conflicting row doesn't exist yet. When PostgreSQL detects the conflict, it aborts one transaction with `40001`, and the API maps that to 409.

`BookingConcurrencyTests` sends two requests for the same slot at the same time and checks that exactly one succeeds. Integration tests use real PostgreSQL instead of the InMemory provider, because the InMemory provider doesn't support transactions.

## TODO

* Testcontainers for integration tests and running them in CI
* Endpoints for listing and cancelling reservations, specialists and available slots
* Exclusion constraint in PostgreSQL as an additional safeguard
* Angular frontend



