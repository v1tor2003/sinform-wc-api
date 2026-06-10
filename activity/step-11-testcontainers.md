# Step 11 - Testcontainers Integration for Integration Testing

We migrated the integration test suite from in-memory SQLite and in-memory caching to real, isolated Docker instances of PostgreSQL and Redis using **Testcontainers**.

## Accomplishments

1. **NuGet Dependencies:**
   - Added `Testcontainers.PostgreSql` and `Testcontainers.Redis` (version `3.10.0`) to the test suite.

2. **Testcontainers Lifecycle Management:**
   - Created `TestcontainersFixture` implementing `IAsyncLifetime` to boot and teardown a PostgreSQL (`postgres:15-alpine`) and Redis (`redis:7-alpine`) container.
   - Leveraged xUnit's native Collection Fixture mechanism (`[CollectionDefinition("SharedContainers")]`) to ensure the containers spin up exactly once for the entire test session, maximizing performance and avoiding Docker resource leaks.

3. **Dynamic Configuration Injection:**
   - Updated `IntegrationTestBase` to override connection string configuration (`ConnectionStrings:DefaultConnection` and `ConnectionStrings:RedisConnection`) with dynamically allocated Docker host ports.
   - Refactored `AppDbContext` registration in integration tests to use Npgsql.

4. **Test Isolation & Cleaning:**
   - Replaced database recreation with a database-level `TRUNCATE` query with `RESTART IDENTITY CASCADE` on PostgreSQL tables. This avoids the "cannot drop the currently open database" concurrency constraint.
   - Programmatically flushed Redis cache data using StackExchange.Redis `ConnectionMultiplexer` with `allowAdmin=true` (calling `FlushAllDatabases()`) before each test run.

## Verification

All 16 tests executed and passed successfully against the Dockerized database and caching services:
```bash
Passed!  - Failed:     0, Passed:    16, Skipped:     0, Total:    16, Duration: 8 s - tests.dll (net10.0)
```
