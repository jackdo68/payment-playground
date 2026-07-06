# PaymentApp

A small payment service built while working through the [C# .NET Recap](https://jackdo68.github.io/csharp-dotnet-recap/) course (Topics 5–9). Users register, log in, and transfer money between accounts — ASP.NET Core + EF Core + PostgreSQL, shipped with Docker Compose.

## Stack

- **.NET 10** / ASP.NET Core (controllers) — the API
- **EF Core + Npgsql** — data access, code-first migrations
- **PostgreSQL 17** (Docker) — `Users` and `Accounts` tables
- **JWT bearer auth** — self-issued HMAC tokens, `[Authorize]` + ownership checks
- **xUnit** — service tests against the EF in-memory provider

## Endpoints

| Method | Route | Access | Notes |
|---|---|---|---|
| POST | `/v1/register` | public | name, email, password → creates user + account ($1,000 starting balance) |
| POST | `/v1/login` | public | email + password → JWT (1h expiry) |
| GET | `/v1/account/balance` | private | *your* balance — user id comes from the token's `sub` claim |
| POST | `/v1/payments/transfer` | private | payer must be the authenticated caller (403 otherwise) |
| POST | `/v1/account/{userId}/deposit` | private | adds funds |
| GET | `/healthz` | public | liveness for containers |

## Run it

```bash
# 1. Database
cd PaymentApp
docker compose up -d db

# 2. Schema (first time, and after model changes)
dotnet ef database update

# 3. API
dotnet run          # note the port printed at startup
```

Or everything in containers: `docker compose up --build` → API on `http://localhost:8080`.

### Smoke test

```bash
PORT=8080  # or the dotnet run port

curl -X POST http://localhost:$PORT/v1/register -H "Content-Type: application/json" \
  -d '{"name":"Alice","email":"alice@bank.test","password":"Passw0rd!"}'

TOKEN=$(curl -s -X POST http://localhost:$PORT/v1/login -H "Content-Type: application/json" \
  -d '{"email":"alice@bank.test","password":"Passw0rd!"}' | jq -r .token)

curl http://localhost:$PORT/v1/account/balance -H "Authorization: Bearer $TOKEN"   # 1000
```

## Tests

```bash
dotnet test         # no Docker needed — tests use the EF in-memory provider
```

## Configuration

Config layers: `appsettings.json` holds dev defaults; env vars override them (`__` = `:`).

| Key | Env var | Purpose |
|---|---|---|
| `ConnectionStrings:PaymentDb` | `ConnectionStrings__PaymentDb` | Postgres connection (compose uses `Host=db`) |
| `Jwt:Key` | `Jwt__Key` | HMAC signing key, ≥ 32 chars — **secret in real deployments**; rotating it invalidates every token |
| `Jwt:Issuer` | `Jwt__Issuer` | `iss` claim / validation |

## Layout

```
PaymentApp/
  Controllers/       # Users (register/login), Account (balance/deposit), Payments (transfer)
  Models/            # User, Account entities + request/response DTOs
  Services/          # IPaymentService + implementation (EF Core)
  Data/              # PaymentDbContext + Migrations/
  docker-compose.yml # db + api
PaymentApp.Tests/    # xUnit service tests
```

## Notes for the curious

- Money is `decimal` end to end (→ Postgres `numeric`). Never floats.
- Passwords are hashed with ASP.NET Identity's `PasswordHasher` (salted); plaintext never stored.
- `TransferAsync` is guarded against concurrent lost updates — see course Topic 7 for the
  race demonstration, and Topic 10 for where the coordination ultimately belongs (the database).
- Dev signing key in `appsettings.json` is for local use only; anything deployed gets it from the environment.
