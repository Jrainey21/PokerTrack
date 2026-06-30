# PokerTrack

A personal poker and blackjack session tracker I built to combine two things I care about — improving my game and learning modern .NET architecture.

I play cash games and tournaments at local casinos and wanted a way to track sessions, analyze trends, and see where I'm winning or losing over time. I used this as an opportunity to build something I'd actually use while working with technologies I wanted to get hands-on experience with.

## Live Demo

🔗 [pokertrack-app-eqe8g9chc4a6dzh7.centralus-01.azurewebsites.net](https://pokertrack-app-eqe8g9chc4a6dzh7.centralus-01.azurewebsites.net)

> Hosted on Azure's free tier, so it may take a few seconds to wake up if it's been idle — if the first load seems slow or shows an error, just refresh.

The web app and database are deployed to Azure (App Service + Azure SQL) with a GitHub Actions CI/CD pipeline that automatically deploys on every push to `main`. The Kafka/Debezium/Worker pipeline runs locally via Docker Compose — see "Running locally" below to see the full event-driven architecture in action.

## What it does

- Log poker and blackjack sessions with venue, game type, stakes, buy-in, cash-out, and duration
- Edit and delete sessions
- Paginated sessions list
- Real-time dashboard showing total profit, win rate, hourly rate, current streak, and best win
- Profit-over-time chart
- Dashboard updates automatically the moment a session is saved — no manual refresh needed
- Microsoft Entra ID SSO

## Architecture

The interesting part of this project is how the analytics pipeline works.

When a session is saved, the web app writes to SQL Server and that's it. A separate background service (the Worker) is responsible for recomputing analytics. The two pieces are completely decoupled — the web app doesn't know the Worker exists.

The pipeline: Log session → SQL Server → Debezium CDC → Kafka → Worker Service → Analytics table → Dashboard

**Debezium** watches SQL Server's transaction log and publishes a change event to Kafka every time a row is inserted, updated, or deleted in the Sessions table. It captures changes at the database level, not the application level — so it doesn't matter how the data gets there.

**Kafka** is the message backbone. The Worker subscribes to the sessions topic and processes events as they arrive.

**The Worker** recomputes all analytics for the affected user from scratch on every event. This makes processing idempotent — if a message is redelivered, the result is identical because it's always derived from the full session history rather than accumulated incrementally.

**SignalR** pushes a notification to the browser the moment the Worker finishes, triggering an automatic dashboard reload. The full round trip from saving a session to seeing updated stats on screen takes under a second.

## Tech stack

- ASP.NET Core Razor Pages (.NET 8)
- SQL Server (Docker)
- Debezium CDC (SQL Server connector)
- Apache Kafka
- .NET Worker Service
- Dapper
- SignalR
- Docker Compose
- Microsoft Entra ID SSO
- Serilog + correlation IDs, Seq, Azure Application Insights
- GitHub Actions CI/CD, deployed to Azure App Service + Azure SQL

## Why these technologies

I work with Debezium and Kafka professionally on a data normalization platform. I wanted to build something outside of work that demonstrates the same patterns in a context I can talk about publicly. This project covers CDC pipelines, event-driven architecture, background services, and real-time UI updates — patterns that show up regularly in mid-level and senior .NET roles.

Dapper was chosen over Entity Framework for full SQL control and performance on a read-heavy dashboard workload.

## Running locally

**Prerequisites**
- Docker Desktop
- .NET 8 SDK
- Visual Studio 2022
- SQL Server Management Studio (optional, for browsing data)

**Steps**

1. Clone the repo
2. Start the containers:
```bash
docker compose up -d
```
3. Wait about 60 seconds for all services to start, then verify Debezium is ready:
```bash
curl http://localhost:8083
```
4. Run the schema against SQL Server (connect to `localhost,1433` with `sa` / `YourStrong!Password`):
```sql
-- run db/schema.sql
```
5. Register the Debezium connector:
```bash
curl -X POST http://localhost:8083/connectors \
  -H "Content-Type: application/json" \
  -d @debezium/connector.json
```
6. Enable SQL Server Agent (required for CDC capture):
```bash
docker exec -it pokertrack-sql /opt/mssql/bin/mssql-conf set sqlagent.enabled true
docker restart pokertrack-sql
```
7. Open the solution in Visual Studio, set both `PokerTrack.Web` and `PokerTrack.Worker` as startup projects, and hit F5

The app runs at `http://localhost:5211`

## Project structure

```
PokerTrack/
  PokerTrack.Web/          # ASP.NET Core Razor Pages web app
  PokerTrack.Worker/       # .NET Worker Service — Kafka consumer and analytics engine
  PokerTrack.Contracts/    # Shared models referenced by both projects
  db/                      # SQL Server schema
  debezium/                # Debezium connector configuration
  docker-compose.yml       # SQL Server, Kafka, Zookeeper, Debezium
```

## Notes

A few non-obvious details that came up during the build:

**Debezium payload structure** — each CDC event contains a `payload` with `before` 
and `after` states and an `op` field indicating the operation type (`c` for insert, 
`u` for update, `d` for delete, `r` for snapshot read on connector startup). On 
delete events `after` is null — the deleted row's data only exists in `before`. 
The Worker handles this explicitly rather than assuming `after` is always present.

**Idempotency** — the Worker recomputes analytics from the full session history on 
every event rather than incrementally updating a running total. If a Kafka message 
is redelivered and processed twice, the result is identical because it's always 
derived from the source of truth in the Sessions table. This avoids the need for 
offset tracking or idempotency key tables.

**Scoped deployment** — the web app and SQL Server are deployed to Azure, but the
Kafka/Debezium/Worker pipeline currently runs locally only. Hosting a continuously
running Kafka broker and connector isn't worth the ongoing cost for a portfolio
project, so the event-driven pipeline is fully demonstrated locally via Docker
Compose rather than running live in production.

## Roadmap

- [ ] Dead letter topic for failed message processing
- [ ] Polly retry policies around Kafka consumer and SQL calls
- [ ] Health checks for Worker and dependencies
- [ ] DbUp for automatic schema migrations on startup
- [ ] Export sessions to CSV
