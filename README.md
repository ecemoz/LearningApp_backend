# LearningApp Backend

![Platform](https://img.shields.io/badge/platform-.NET%2010-512BD4)
![Database](https://img.shields.io/badge/database-PostgreSQL-336791)
![API](https://img.shields.io/badge/API-REST-0A7EA4)
![Auth](https://img.shields.io/badge/auth-JWT-222222)

Backend API for a learning platform where users can:
- explore topics and lessons,
- complete lessons and track progress,
- take quizzes,
- unlock achievements,
- and (as Admin) manage learning content.

## Table of Contents

- [What This Project Does](#what-this-project-does)
- [Why This Project Is Useful](#why-this-project-is-useful)
- [Project Structure](#project-structure)
- [How To Get Started](#how-to-get-started)
- [Usage Examples](#usage-examples)
- [Where To Get Help](#where-to-get-help)
- [Who Maintains And Contributes](#who-maintains-and-contributes)

## What This Project Does

LearningApp Backend is an ASP.NET Core Web API built with a layered architecture:
- API layer for HTTP endpoints and authentication.
- Infrastructure layer for persistence, migrations, seeding, and security services.
- Domain layer for core entities (User, Topic, Lesson, Quiz, Achievement, etc.).
- Application layer reserved for evolving business-use-case orchestration.

The API uses PostgreSQL with Entity Framework Core, JWT-based authentication, Swagger/OpenAPI, and role-based authorization for admin operations.

Useful entry points in the repository:
- [src/LearningApp.API/Program.cs](src/LearningApp.API/Program.cs)
- [src/LearningApp.Infrastructure/Persistence/AppDbContext.cs](src/LearningApp.Infrastructure/Persistence/AppDbContext.cs)
- [src/LearningApp.Infrastructure/Persistence/Seed/SeedData.cs](src/LearningApp.Infrastructure/Persistence/Seed/SeedData.cs)
- [LearningApp_backend.slnx](LearningApp_backend.slnx)

## Why This Project Is Useful

Key benefits for product teams and contributors:

- Fast MVP foundation:
  Includes topics, lessons, quizzes, achievements, and progress tracking out of the box.
- Built-in auth and authorization:
  JWT auth for users plus role-based admin endpoints.
- Auto-migrate on startup:
  The API applies pending EF Core migrations at boot.
- Seeded starter data:
  Development environment starts with demo topics, lessons, quizzes, achievements, and an admin user.
- Clear separation of concerns:
  Easier to maintain and evolve as features grow.

## Project Structure

```text
src/
  LearningApp.API/            # Controllers, DTOs, startup/config
  LearningApp.Application/    # Application layer (currently lightweight)
  LearningApp.Domain/         # Core entities
  LearningApp.Infrastructure/ # EF Core, auth services, seeding, migrations
tests/
  LearningApp.UnitTests/      # xUnit test project
```

Main endpoint groups:
- Auth: register/login/me
- Learner: topics, lessons, progress, quizzes, achievements
- Admin: dashboard + CRUD for topics/lessons/quizzes/achievements

## How To Get Started

### 1. Prerequisites

- .NET 10 SDK
- PostgreSQL (local or remote)

Optional (for manual migration workflows):
- EF Core CLI

```bash
dotnet tool install --global dotnet-ef
```

### 2. Clone And Restore

```bash
git clone <your-repository-url>
cd LearningApp_backend
dotnet restore
```

### 3. Configure Environment

Default settings live in [src/LearningApp.API/appsettings.json](src/LearningApp.API/appsettings.json), but the app expects the database connection to come from environment variables or a local `.env` file.

For local development, copy [.env.example](.env.example) to `.env` and fill in your Supabase password:

```bash
cp .env.example .env
```

Then set your values in `.env`:

```bash
ConnectionStrings__DefaultConnection="Host=aws-1-eu-central-1.pooler.supabase.com;Port=5432;Database=postgres;Username=postgres.bqazpdtajuronszrzypj;Password=YOUR_PASSWORD;Ssl Mode=Require;Trust Server Certificate=true"
Jwt__Key="replace-with-a-strong-secret-key-at-least-32-characters"
Jwt__Issuer="LearningAppAPI"
Jwt__Audience="LearningAppClient"
Jwt__ExpiryMinutes="60"
```

Render should use the same `ConnectionStrings__DefaultConnection` value as an environment variable; do not put it in source control.

### 4. Run The API

```bash
dotnet run --project src/LearningApp.API
```

Default local URLs from launch profile:
- http://localhost:5253
- https://localhost:7037

Swagger UI:
- http://localhost:5253/swagger

### 5. Seeded Development Admin Account

The app seeds an admin user on startup (if it does not already exist):

- Email: admin@learningapp.com
- Password: Admin123!

For non-local environments, change seeded credentials and secrets before deployment.

### 6. Run Tests

```bash
dotnet test
```

## Usage Examples

### Register

```bash
curl -X POST http://localhost:5253/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "userName": "learner1",
    "email": "learner1@example.com",
    "password": "StrongPassword123!"
  }'
```

### Login

```bash
curl -X POST http://localhost:5253/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin@learningapp.com",
    "password": "Admin123!"
  }'
```

### Access Authorized Endpoint

```bash
curl -X GET http://localhost:5253/api/progress/summary \
  -H "Authorization: Bearer <JWT_TOKEN>"
```

### Endpoint Overview

| Group | Base Path | Auth |
|---|---|---|
| Auth | /api/auth | Public (except /me) |
| Topics & Lessons | /api/topics, /api/lessons | Mixed |
| Progress | /api/progress | User token required |
| Quiz | /api/topics/{topicId}/quiz, /api/quizzes/{quizId}/submit | Mixed |
| Achievements | /api/achievements | User token required |
| Admin | /api/admin/* | Admin role required |

## Where To Get Help

- Swagger/OpenAPI for interactive exploration: /swagger when running locally.
- Source-level references:
  - [src/LearningApp.API/Controllers](src/LearningApp.API/Controllers)
  - [src/LearningApp.Infrastructure/Persistence/Migrations](src/LearningApp.Infrastructure/Persistence/Migrations)
  - [src/LearningApp.API/LearningApp.API.http](src/LearningApp.API/LearningApp.API.http)
- For bugs and feature requests:
  Open an issue in the repository and include reproduction steps, expected behavior, and logs.

## Who Maintains And Contributes

Maintainer:
- @ecemnurozen

Contribution guidelines (quick version):

1. Fork and create a feature branch.
2. Keep changes focused and include tests when behavior changes.
3. Run formatting/build/tests locally before opening a PR.
4. Open a PR with a clear description and sample requests/responses for API changes.

Suggested local validation before PR:

```bash
dotnet build
dotnet test
dotnet run --project src/LearningApp.API
```
