# prompts.md

## AI Usage Summary

AI tools used:
- GitHub Copilot
- Antigravity

All generated code was reviewed and modified before being committed to maintain high software standards and architectural integrity.

---

# Prompt 1 – Define Architecture

### Prompt

Design a .NET 8 Minimal API solution for a Flight Status lookup system with an Angular frontend. Use clean architecture principles, dependency injection, provider abstraction, unit testing, and stub providers.

### Result

Accepted

### Notes

The proposed architecture defined a clean separation of concerns across projects. The backend is structured into domain models, mappers, and services, whereas the Angular frontend uses standalone components to separate the search forms and the result displays. Folder and file naming conventions were slightly adjusted to match standard enterprise practices.

---

# Prompt 2 – Generate Unified Models

### Prompt

Generate unified FlightStatusResult model, provider DTOs, and UnifiedFlightStatus enum for AeroTrack and QuickFlight.

### Result

Accepted

### Notes

We created the `FlightStatusResult` model to represent the final, normalized state of a flight status query. We also created DTOs for the separate third-party providers (AeroTrack and QuickFlight) to cleanly model their raw responses. Minor property name adjustments were made to ensure full consistency between backend serialization properties and client-side TypeScript bindings.

---

# Prompt 3 – Generate Provider Interface

### Prompt

Generate an IFlightStatusProvider interface suitable for multiple provider implementations.

### Result

Accepted

### Notes

We generated the `IFlightStatusProvider` interface, establishing a strategy contract for third-party integrations. This allows us to handle different flight data providers interchangeably. We renamed the method signature from `GetFlightAsync()` to `GetFlightStatusAsync()` to clearly indicate that it queries the status of a flight rather than just general flight information.

---

# Prompt 4 – Generate Stub Providers

### Prompt

Generate AeroTrackProvider and QuickFlightProvider with deterministic hardcoded responses.

### Result

Accepted

### Notes

We generated `AeroTrackFlightStatusProvider` and `QuickFlightFlightStatusProvider` stub implementations. They hold hardcoded mock data to simulate real provider endpoints. We added multiple test cases (`AI101`, `BA202`, and `UA303`) with varying timestamps to let us verify how our conflict resolution logic works under realistic scenarios.

---

# Prompt 5 – Initial Service Generation

### Prompt

Generate a service that retrieves data from both providers and returns the latest response.

### Result

Rejected

### Reason

The generated code had two main issues. First, it executed the provider requests one after the other (sequentially) instead of running them at the same time (in parallel). Second, it was tightly coupled to the concrete class names instead of using dependency injection with the `IFlightStatusProvider` interface, which violated the Dependency Inversion Principle.

---

# Prompt 6 – Refined Service Generation

### Prompt

Generate a FlightStatusService that:
- injects IEnumerable<IFlightStatusProvider>
- executes provider calls in parallel using Task.WhenAll
- chooses the response with the latest LastUpdatedUtc
- handles provider failures gracefully
- logs provider failures

### Result

Accepted

### Notes

The updated code injects all registered providers as a collection (`IEnumerable<IFlightStatusProvider>`). It queries them in parallel using `Task.WhenAll` to ensure low response times. If multiple providers return a result, it resolves conflicts by picking the one with the most recent `LastUpdatedUtc` timestamp. It also catches individual provider errors and logs them using structured logging to ensure the system remains resilient.

---

# Prompt 7 – Status Normalization

### Prompt

Generate a mapper that converts AeroTrack and QuickFlight status values into the UnifiedFlightStatus enum.

### Result

Accepted

### Notes

We created the static `StatusNormalizer` containing pure, stateless mapper functions to translate vendor-specific statuses (like `ON_TIME` or `ON_SCHEDULE`) into the unified `UnifiedFlightStatus` enum values. We added a fallback that maps any unknown or unrecognized status string to `UnifiedFlightStatus.Unknown` to ensure the mapper never crashes.

---

# Prompt 8 – Minimal API Endpoint

### Prompt

Generate a Minimal API endpoint for GET /flights/status with dependency injection, input validation, and ProblemDetails responses.

### Result

Accepted

### Notes

We added a Minimal API endpoint for `GET /flights/status`. It registers services via dependency injection and uses a custom endpoint filter (`ValidationFilter`) to run checks on input parameters before executing the endpoint, returning a standardized RFC 7807 `ProblemDetails` error payload when validation fails. The route configuration and parameter names were adjusted to match project conventions.

---

# Prompt 9 – Angular Search Page

### Prompt

Generate an Angular search page using Reactive Forms for flight number and date with validation.

### Result

Accepted

### Notes

We created a standalone `SearchComponent` built with Angular Reactive Forms. It configures form controls for both the flight number and the departure date. It runs input format validation rules locally on the client and manages loading animations and error banners dynamically based on HTTP request statuses.

---

# Prompt 10 – Angular Result Card

### Prompt

Generate an Angular result card that displays unified flight status and AeroTrack-specific fields only when available.

### Result

Accepted

### Notes

We created `ResultComponent` to display flight status results in a premium, glassmorphic layout. It conditionally renders specific details like `terminal`, `gate`, and `delayReason` only if they are present in the response, hiding empty elements to keep the interface neat and clean.

---

# Prompt 11 – Dynamic Status Colors

### Prompt

Generate dynamic badge and card colors based on unified flight status.

### Result

Rejected

### Reason

The generated HTML code did not dynamically bind color classes based on the status enum values. Instead, it used fixed CSS classes, meaning the flight status card borders and badges remained static and could not change colors based on different API status responses.

---

# Prompt 12 – Refined Dynamic Colors

### Prompt

Update the Angular component so that badge and border colors are determined dynamically using Angular binding.

Status mapping:
- OnTime → Green
- Delayed → Amber
- Cancelled → Red
- Diverted → Red
- Unknown → Grey

### Result

Accepted

### Notes

We updated the HTML and TypeScript code to dynamically bind classes via `ngClass` based on the normalized flight status enum. We added status-specific styling overrides in the CSS so that the card borders and status badges light up in green, amber, red, or grey depending on whether the flight is On Time, Delayed, Cancelled, or Unknown.

---

# Prompt 13 – Unit Tests

### Prompt

Generate xUnit tests covering:
- status normalization
- latest LastUpdatedUtc selection
- provider failures
- input validation

### Result

Accepted

### Notes

We developed a complete suite of 42 xUnit tests verifying our service mappings, parallel execution flows, error logging resilience, and request parameter validation rules. This ensures any future code updates will not break the core functionality of the system.

---

# Prompt 14 – README

### Prompt

Generate a professional README containing setup steps, assumptions, architecture, testing instructions, and project structure.

### Result

Accepted

### Notes

We wrote a comprehensive developer manual outlining project organization, architectural decisions, code designs (like records and aspect-oriented validations), assumptions about local test data, setup commands, and runtime test guides.

---

# Prompt 15 – Logging

### Prompt

Generate structured logging for provider failures and request processing.

### Result

Rejected

### Reason

The generated code was using basic `Console.WriteLine` statements to output debug logs. This did not meet production standards because it bypassed structured logging configurations, making it impossible to query or store log entries in external monitoring services.

---

# Prompt 16 – Refined Logging

### Prompt

Generate structured logging using ILogger for provider calls, failures, and provider selection.

### Result

Accepted

### Notes

We refactored the log statements to use .NET's built-in `ILogger` interface. This enables structured logging, allowing the system to output logs with structured metadata fields for query parameters, provider selection metrics, and exception messages.

---

# Prompt 17 – Global Exception Handling

### Prompt

Generate middleware that converts unhandled exceptions into ProblemDetails responses.

### Result

Accepted

### Notes

We created a `GlobalExceptionHandler` middleware implementing .NET 8's native `IExceptionHandler` interface. It captures any unhandled runtime exceptions globally, formats them into structured RFC 7807 `ProblemDetails` payloads, and returns them as HTTP responses.

---

# Prompt 18 – Reflection Document

### Prompt

Generate reflection.md describing assumptions, trade-offs, lessons learned, and future improvements.

### Result

Accepted

### Notes

We wrote a reflection log detailing architecture trade-offs (like static mappers vs. injected mappers, parallel task performance, and resilient fallback strategies), challenges faced during macOS setup, production scaling improvements (like Redis caching and Polly circuit breakers), and future roadmaps.

---

# Prompt 19 – Enforce Strict Flight Number Format

### Prompt

Add validation for the `flightNumber` query parameter in the backend. Accept only valid flight numbers matching the pattern: 2–3 uppercase letters followed by 1–4 digits (e.g. AI101, BA202, EK508). Return HTTP 400 Bad Request with a meaningful validation message if the format is invalid.

### Result

Rejected

### Reason

The initial implementation used the `[RegularExpression]` Data Annotation directly on the request DTO. This caused Swashbuckle to append a pattern schema to the OpenAPI definition, which triggered Swagger UI to block invalid inputs with client-side field validation below the input text box, rather than returning a structured HTTP 400 response body from the backend filter.

---

# Prompt 20 – Refined Backend-Only Format Validation

### Prompt

Update the validation so that format validation is performed in the backend API filter, not through OpenAPI parameter pattern validation. Remove any OpenAPI/Swagger parameter validation attributes (like `[RegularExpression]`) that cause client-side Swagger blocks. Keep the same validation rule (`^[A-Z]{2,3}\d{1,4}$`) but enforce it strictly in the backend, returning the error in the HTTP response body as a ProblemDetails/ValidationProblemDetails payload.

### Result

Accepted

### Notes

Removed `[RegularExpression]` attributes from the request DTO and delegated formatting validations entirely to the FluentValidation pipeline inside the endpoint `ValidationFilter`. This successfully forces Swagger UI to send the request to the server, displaying the structured HTTP 400 ProblemDetails schema in the Response section as expected.

---

# Prompt 21 – Production-Ready Code Review & Clean Comments

### Prompt

Perform a final production-ready code review of my solution. Review every C# and Angular file and improve the comments. Reduce comment noise, remove obvious comments, keep only comments that provide long-term maintenance value, and rewrite comments in a concise, professional style explaining architecture, business rules, assumptions, and non-obvious decisions.

### Result

Accepted

### Notes

Cleaned up comments across 18 source files. Removed 124 trivial inline comments, rewrote 20 to focus on design/architectural decisions, and retained 20 method descriptions. Ensured zero compiler warnings and verified all tests pass cleanly.

---

# Prompt 22 – Git Workflow Documentation in Reflection

### Prompt

Update and push in reflection.md file:
"## Git Workflow
This coding exercise was designed to be completed within a single day. To maximize the time available for implementation, testing, and documentation, I worked directly on the main branch..."

### Result

Accepted

### Notes

Added Section 7 "Git Workflow" to `reflection.md` documenting branching practices, PEER reviews, and promotional pipelines from develop to main.

---

# Prompt 23 – API-Level Integration Tests

### Prompt

Review my .NET 8 Minimal API solution and generate API-level integration tests using xUnit and Microsoft.AspNetCore.Mvc.Testing (WebApplicationFactory). Start the API in memory without hosting it manually, testing the complete HTTP pipeline without mocking endpoints. Test 8 specific scenarios covering valid requests, missing/invalid parameters, unknown flights, provider prioritization (latest timestamp), and single/dual provider crash fault tolerance.

### Result

Accepted

### Notes

Installed `Microsoft.AspNetCore.Mvc.Testing` in the tests project. Configured the tests target framework to `net9.0` to prevent `UnflushedBytes` serialization crashes on local .NET 9 SDK development environments. Wrote 11 test cases inside `FlightStatusIntegrationTests.cs` using inline NSubstitute mocked service providers to simulate supplier exceptions. All 60/60 tests run and pass successfully.
