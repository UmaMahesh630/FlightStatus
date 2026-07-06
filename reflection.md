# Engineering Reflections & Design Decisions

This document captures assumptions, architectural tradeoffs, challenges, and production considerations identified during the implementation of the Flight Status lookup system.

---

## 1. Assumptions Made

* **Query Binding Case-Insensitivity**: Flight numbers are treated as case-insensitive (e.g., `ai101` maps to the same stub record as `AI101`). Inputs are cleaned (trimmed and uppercase-normalized) at both validation and provider lookup boundaries.
* **ISO 8601 Date Formatting**: The API expects dates in the strict format `yyyy-MM-dd`. Validation filters reject other formats (e.g., `MM-dd-yyyy`) to prevent timezone offset discrepancies during parsing.
* **Coexistence of Mock Data**: The mock data in the provider stubs has been engineered with specific timestamp offsets (QuickFlight newer for `AI101`, AeroTrack newer for others) to assert and test that the service's timestamp conflict resolution logic behaves correctly.
* **Frontend Port Stability**: The Angular frontend is assumed to run on the standard development port `http://localhost:4200`. The API's CORS policy is configured explicitly to permit requests from this origin.

---

## 2. Technical Tradeoffs

### 2.1. Pure Static Mapping vs. DI-Injected Mapper Services
* **Static Utility Chosen (`StatusNormalizer`)**: Mappers are implemented as stateless, pure static methods.
  * *Pros*: Zero object allocation, high performance, clean testability without requiring mock mapper setups, and zero DI container clutter.
  * *Cons*: If status translation rules ever need to change dynamically (e.g., loaded from a database or remote config), a static mapper is difficult to adapt.
  * *Rationale*: Since the vendor vocabularies for these stubs are compile-time constants, static pure functions are the most efficient and readable choice.

### 2.2. Concurrency Model: Concurrent Parallel Tasking vs. Sequential Queries
* **Concurrent Execution Chosen (`Task.WhenAll`)**: Queries all registered providers in parallel.
  * *Pros*: Minimizes response latency. The search response time is bound by the slowest single responsive provider, rather than the cumulative sum of all provider lookups.
  * *Cons*: Increases temporary thread/task exhaustion under heavy load since it fires $N$ tasks per request.
  * *Rationale*: In external API queries, operations are I/O bound. Asynchronous execution yields the executing thread back to the thread pool while waiting for responses, making parallel tasks highly scalable and performance-optimal for this use case.

### 2.3. Resilient Fallbacks vs. HTTP Error Propagation
* **Fallback Return Chosen**: If all providers fail or return no records, the service returns a unified result containing `UnifiedFlightStatus.Unknown` and a clear reason, rather than throwing a `500 Server Error` or returning a `404 Not Found` response.
  * *Pros*: Prevents API crashes. The frontend can render a styled "Unknown/Unavailable" card gracefully rather than throwing raw error screens.
  * *Cons*: Requires the client to check the status field to discover that the fetch was unsuccessful, rather than relying solely on HTTP status codes.
  * *Rationale*: For search integrations, provider unavailability is a transient runtime state rather than a system crash. Treating it as a structured domain response is safer and improves UI stability.

---

## 3. Improvements & Architectural Refinements

* **Minimal API Validation Filters (`ValidationFilter`)**: By writing a custom generic `IEndpointFilter` to handle both Data Annotations and FluentValidation, we implemented a form of Aspect-Oriented Programming (AOP). Validation is executed as a cross-cutting concern, keeping endpoint route handlers down to just 3 clean lines of code.
* **Cascade Mode Stop**: Configured validators to stop evaluation on the first rule failure per property. This prevents validation waterfalling (e.g., returning both "required" and "length exceeded" errors simultaneously), resulting in cleaner responses for client parsers.
* **Target RollForward Configurations**: Configured the `<RollForward>Major</RollForward>` property in C# csproj files to ensure compilation and testing execute smoothly on developer environments that have newer SDKs (like .NET 9) while keeping target frameworks aligned to .NET 8.

---

## 4. Challenges Faced

* **.NET 9 SDK Templates**: The system's installed .NET 9 SDK automatically generated template codes utilizing .NET 9 specific features like `AddOpenApi()` and `MapOpenApi()`. Since targeting .NET 8 was required, these had to be manually refactored out in favor of standard .NET 8 compatibility methods.
* **Parameter Binding in Minimal APIs**: Minimal APIs bind parameters individually by default. To implement robust model validations, we introduced `[AsParameters] FlightStatusRequest` to bundle parameters into a cohesive record, allowing elegant integration with FluentValidation.

---

## 5. Production Considerations

If scaling this system to support millions of queries in a real-world environment:
* **Caching Layer**: Integrating a caching solution like **Redis** (with short TTLs, e.g., 2–5 minutes) is essential. Flight status updates do not change second-by-second; caching results prevents costly redundant queries to commercial third-party flight APIs.
* **Circuit Breaker Pattern (Polly)**: If an external provider undergoes downtime, hammering its endpoint degrades system resources. Integrating a circuit breaker (via Polly policies) would temporarily trip and halt calls to that provider, instantly returning cached/fallback data until the provider recovers.
* **API Authentication & Secrets Management**: The stubs do not require authentication, but real endpoints would. Credentials (API Keys, Client Secrets) must be securely managed via Azure Key Vault or AWS Secrets Manager and resolved in DI as typed configuration options.

---

## 6. Future Enhancements

* **Dynamic Provider Discovery**: Moving provider registrations from hardcoded DI lines to configuration-driven assemblies, letting us add or disable providers by modifying an `appsettings.json` file without recompiling.
* **SignalR Push Updates**: Implementing a WebSocket/SignalR connection to push live status changes (e.g., gate updates, delayed status) directly to the support agent's screen without requiring manual page refreshes.
* **Query Auditing Database**: Logging queries, provider response times, and selection outcomes into a database to audit provider SLA performance and verify that the selection algorithm is functioning correctly in production.
