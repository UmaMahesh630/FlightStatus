# AI Instructions & Engineering Standards Handbook

This document defines the strict engineering standards, architectural guidelines, and design patterns implemented in the Flight Status Lookup system. All AI coding assistants (GitHub Copilot, Antigravity, Claude, Cursor) and human developers **must** adhere to these conventions for all future modifications and enhancements.

---

## 1. Project Overview

### 1.1. Purpose
The system provides airline support agents with a real-time, consolidated, and normalized view of flight status information. It queries multiple third-party suppliers (AeroTrack and QuickFlight) concurrently, normalizes their disparate status vocabularies, resolves timing conflicts, and presents a clean visual result card.

### 1.2. High-Level Architecture
The solution follows a clean, decoupled architecture:
* **Domain Layer**: Holds the core unified models and enums.
* **Service Layer**: Houses stateless mappers, business normalizers, and orchestrators.
* **API Host Layer**: Exposes endpoints via .NET Minimal API, protected by validation filters and global exception handlers.
* **Strategy Providers**: Encapsulates custom vendor logic inside interchangeable provider modules.
* **Angular UI**: A standalone frontend client consuming the API.

---

## 2. Technology Stack

* **Backend Framework**: .NET 8 Web API (configured with `RollForward` major configuration).
* **Frontend Framework**: Angular 19+ (Bootstrap-free Vanilla CSS for glassmorphism layout).
* **OpenAPI Documentation**: Swashbuckle OpenAPI with endpoints metadata annotations.
* **Validation**: FluentValidation combined with .NET Data Annotations.
* **Testing Suite**: xUnit, FluentValidation testing framework, and NSubstitute for mocking.
* **Logging System**: Microsoft Extension Logging (`ILogger`) mapped to structured console outputs.

---

## 3. Architecture Standards

### 3.1. Layered Separation of Concerns (SoC)
* Endpoints **must** act only as routing gateways. They must not contain business validation, database mapping, or orchestration logic.
* Validation **must** occur before the endpoint execution using the Aspect-Oriented `IEndpointFilter` pattern.
* Orchestration **must** be managed by the Service layer.
* Data fetching and vendor mappers **must** reside inside individual strategy provider implementations.

### 3.2. Constructor Dependency Injection Only
* In the backend, dependency injection **must** occur via constructor injection or primary constructor parameters on classes. Do not use service locator patterns (e.g., manual calls to `HttpContext.RequestServices.GetService`).
* In the Angular frontend, use functional injection via the `inject()` token instead of constructors.

### 3.3. Anti-Corruption Layer (ACL)
* Never expose raw third-party vendor structures (`AeroTrackResponse`, `QuickFlightResponse`) to the controllers or frontend. 
* Translate vendor payloads immediately into the unified internal domain record `FlightStatusResult` inside the provider strategy.

---

## 4. SOLID Principles in Action

* **Single Responsibility Principle (SRP)**:
  * Classes have one dedicated responsibility. For instance, `StatusNormalizer` only translates status codes, `ValidationFilter` only runs schema checks, and `FlightStatusService` only coordinates lookups.
* **Open/Closed Principle (OCP)**:
  * The system is open for extension but closed for modification. Adding a new vendor is done by implementing the `IFlightStatusProvider` interface without modifying the `FlightStatusService` orchestrator or API endpoints.
* **Liskov Substitution Principle (LSP)**:
  * Any implementation of `IFlightStatusProvider` can be substituted dynamically by the DI container (`IEnumerable<IFlightStatusProvider>`) without breaking the orchestrator.
* **Interface Segregation Principle (ISP)**:
  * Interfaces are highly cohesive and narrow. `IFlightStatusProvider` contains only the status query methods; it does not mix registry or lifecycle management.
* **Dependency Inversion Principle (DIP)**:
  * The orchestration service depends entirely on abstractions (`IFlightStatusProvider`) rather than concrete provider classes.

---

## 5. Design Patterns Used

### 5.1. Strategy Pattern
* Handled through `IFlightStatusProvider`. Each vendor integration acts as a strategy. Adding or removing vendors only requires registering or unregistering their concrete class in `Program.cs`.

### 5.2. Mapper Pattern
* Encapsulated in the static `StatusNormalizer` service. It maps raw vendor status codes (`ON_TIME`, `DELAYED`, `REROUTED`, `ON_SCHEDULE`, `CANCELED`) to the system's `UnifiedFlightStatus` enum.

### 5.3. Scatter-Gather Pattern
* Implemented in `FlightStatusService` using `Task.WhenAll`. It launches asynchronous tasks concurrently to all strategy providers, waiting for all to complete or timeout in parallel.

---

## 6. Coding Standards

### 6.1. C# Naming Conventions
* **Namespaces & Classes**: `PascalCase` (e.g., `FlightStatus.Api.Services`).
* **Interface Names**: Prefix with `I` (e.g., `IFlightStatusService`).
* **Method Names**: `PascalCase` (e.g., `ExecuteLookupAsync`).
* **Private Readonly Fields**: Prefix with `_` and use camelCase (e.g., `_logger`, `_providers`).
* **DTOs & Models**: Use `record` types with `init`-only properties to ensure model immutability.

### 6.2. Async / Await Guidelines
* All network, file, and database operations **must** be fully asynchronous (`async`/`await`).
* Always return `Task` or `Task<T>` rather than `void` (except for event handlers).
* Suffix all asynchronous method signatures with `Async` (e.g., `GetFlightStatusAsync`).

### 6.3. Nullable Reference Types
* Non-nullable reference types are enabled globally via `<Nullable>enable</Nullable>`. Any property that can hold a null state must be explicitly decorated with a `?` character (e.g., `string? Gate`).

---

## 7. API & Error Handling Standards

### 7.1. REST Query Conventions
* Status route: **`GET /flights/status`**
* Parameters:
  * `flightNumber`: string (Query)
  * `date`: string (Query, format `yyyy-MM-dd`)

### 7.2. Response Structures
* **Success**: Returns `200 OK` with the JSON `FlightStatusResult` model.
* **Validation Error**: Returns `400 Bad Request` with `Content-Type: application/problem+json` containing standard RFC 7807 validation details.
* **Server Error**: Returns `500 Internal Server Error` with `Content-Type: application/problem+json`.

### 7.3. Global Exception Handler
* Captured by `GlobalExceptionHandler` implementing `IExceptionHandler`. It logs the exception trace securely and returns a structured `ProblemDetails` response to the client. Never expose raw stack traces in non-development environments.

---

## 8. Validation Standards

### 8.1. Backend-Only Validation Enforcement
* Do **not** use `[RegularExpression]` or format attributes on DTO properties that generate OpenAPI schema patterns. This causes Swagger UI to apply browser client-side blocks.
* Keep the DTO properties simple and run format/regex validations exclusively inside the backend's FluentValidation filter (`ValidationFilter`).
* The validation pattern for flight numbers **must** match: `^[A-Z]{2,3}\d{1,4}$` (2 to 3 uppercase letters followed by 1 to 4 digits).

---

## 9. Logging Standards

* Use structured logging parameters instead of string interpolation (e.g., `_logger.LogInformation("Processing flight {FlightNumber}", flightNumber);` instead of `$"Processing {flightNumber}"`).
* Do **not** use `Console.WriteLine` or `Debug.WriteLine`.
* Log provider failures as warnings (`LogWarning`) and coordinate context variables securely.

---

## 10. Angular UI Standards

* **Standalone Architecture**: Component files, service files, and routes must be declared without `NgModule` declarations.
* **Reactive Forms**: Form forms must use `FormBuilder` and `FormGroup` with client-side validators matching backend rules (e.g., regex checks on flight numbers).
* **RxJS Pipeline**: Manage HttpClient streams using pipes (e.g., `pipe(catchError(...))`) and propagate errors using `throwError`.
* **Dynamic Styling**: Class highlights and layout boundaries must bind dynamically based on the normalized status string (e.g., green for `OnTime`, amber for `Delayed`, red for `Cancelled`/`Diverted`, grey for `Unknown`).
* **Conditional UI**: Hide missing optional fields (`gate`, `terminal`, `delayReason`) from the template dynamically using `*ngIf` structural directives.

---

## 11. Unit Testing Standards

* **Framework**: xUnit.
* **Pattern**: Follow the **Arrange-Act-Assert (AAA)** pattern strictly. Separate sections using comments.
* **Naming Conventions**: Test method names must be descriptive and follow the structure: `UnitOfWork_StateUnderTest_ExpectedBehavior` (e.g., `Normalizer_WithAeroTrackLateStatus_ShouldMapToDelayed`).
* **Mocking**: Use `NSubstitute` to mock dependencies (do not write manual stub classes inside tests).

---

## 12. Git & Commit Standards

* **Commit Format**: Conventional Commits style:
  * `feat: ...` for new features (e.g., `feat: add swagger support`).
  * `fix: ...` for bug fixes (e.g., `fix: resolve CORS issues`).
  * `chore: ...` for build, tool, or config updates.
  * `docs: ...` for documentation updates.

---

## 13. AI Usage Guidelines

1. **Review Requirement**: AI models may generate functional code, but human developers **must** inspect, compile, test, and understand all changes before pushing.
2. **Architecture Consistency**: Never let the AI generate sequential provider loops. Lookups must execute in parallel using `Task.WhenAll`.
3. **No Unapproved Frameworks**: Do not install third-party NuGet packages or npm libraries unless explicitly approved.

---

## 14. Code Review Checklist for Developers

- [ ] All C# projects build successfully without errors or warnings (`dotnet build`).
- [ ] All 49 unit tests pass successfully (`dotnet test`).
- [ ] The Angular application compiles successfully in production mode (`npm run build`).
- [ ] No `[RegularExpression]` schema validations exist on `FlightStatusRequest.cs` (to avoid Swagger UI browser blocks).
- [ ] All vendor status mappings fallback to `UnifiedFlightStatus.Unknown` instead of throwing exceptions.
- [ ] The `FlightStatusService` queries all providers concurrently using `Task.WhenAll`.
- [ ] CORS policies permit requests from origin `http://localhost:4200` to `http://localhost:5253`.
- [ ] No console printing calls (`Console.WriteLine`) are left in the code.
- [ ] Commit message conforms to Conventional Commits format.
