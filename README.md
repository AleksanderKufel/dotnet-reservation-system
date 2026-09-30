# Reservation System

[![CI](https://github.com/AleksanderKufel/dotnet-reservation-system/actions/workflows/ci.yml/badge.svg)](https://github.com/AleksanderKufel/dotnet-reservation-system/actions/workflows/ci.yml)

REST API for booking appointments with specialists. The main requirement is that a specialist can't be double-booked, even under concurrent requests.

**Stack:** .NET 10, ASP.NET Core Web API, EF Core, PostgreSQL, ASP.NET Core Identity, FluentValidation, Microsoft.Extensions.Http.Resilience, xUnit, Moq, Testcontainers, Respawn, WireMock.Net, Docker, GitHub Actions, Azure App Service

## Project structure

* `Domain` - `Reservation` and `Specialist` entities, cancellation rules, overlap checking, working hours and available slots
* `Application` - `ReservationService`, `SpecialistService` and repository / unit of work interfaces
* `Infrastructure` - EF Core, repositories, migrations, Identity store, public holidays client
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

Tests (integration tests start their own PostgreSQL container with Testcontainers, so only Docker needs to be running):

```bash
dotnet test
```

## Endpoints

* `POST /register`, `POST /login` - Identity API endpoints, login returns a bearer token
* `GET /api/specialists` - list of specialists
* `GET /api/specialists/{id}/slots?date=2030-01-07` - free one-hour slots on a given day
* `POST /api/reservations` - create a reservation, returns `201` with a `Location` header
* `GET /api/reservations/me?page=1&pageSize=10` - current user's reservations, paged
* `GET /api/reservations/{id}` - a single reservation of the current user
* `POST /api/reservations/{id}/cancel` - cancel a reservation, allowed up to 24 hours before it starts
* `GET /health`

Reservation endpoints require a token. Specialists and slots are public.

Working hours are Monday to Friday, 9:00-17:00 UTC, in one-hour slots, and a reservation must match one slot. Polish public holidays are closed. Three sample specialists are seeded by a migration.

Errors are returned as ProblemDetails. An overlapping or concurrently booked slot returns `409 Conflict`, a broken business rule (outside working hours, public holiday, too late to cancel) returns `400`, a missing or another user's reservation returns `404`, and `503` means public holidays couldn't be loaded.

## Public holidays

Holidays come from the [Nager.Date](https://date.nager.at) API through a typed `HttpClient` with the standard resilience handler (retries with backoff, timeouts, circuit breaker). Holidays for a year are cached in memory for 24 hours. If the API is down and nothing is cached, reservations are rejected with `503` instead of risking a booking on a holiday.

Tests don't call the real API: the client is tested against WireMock.Net, and API tests use a fake holiday provider. One smoke test (`Category=External`) calls the real API; CI skips it and a separate workflow runs it weekly.

## Double booking

Checking for overlaps and inserting the reservation run in a single `Serializable` transaction. With Read Committed, two requests could both pass the check and both insert. A row lock won't help here, because the conflicting row doesn't exist yet. When PostgreSQL detects the conflict, it aborts one transaction with `40001`, and the API maps that to 409.

`BookingConcurrencyTests` sends two requests for the same slot at the same time and checks that exactly one succeeds. Integration tests use real PostgreSQL instead of the InMemory provider, because the InMemory provider doesn't support transactions. The database is reset with Respawn before each test.

## TODO

* Exclusion constraint in PostgreSQL as an additional safeguard
* Angular frontend



