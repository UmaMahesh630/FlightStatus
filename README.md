# SkyRoute Flight Status Lookup System

A modern, high-performance, full-stack application built for support agents to query, aggregate, and display normalized flight status information from multiple third-party providers.

---

## 1. Project Structure

The repository is organized following clean architecture guidelines separating concerns across tiers:

```text
FlightStatus/                          # Repository Root
├── README.md                          # Main project guide (this file)
├── spec.md                            # Technical specification & system architecture
├── prompts.md                         # Detailed log of AI prompts used
├── reflection.md                      # Post-submission feedback & modifications
├── FlightStatus.sln                   # .NET Solution file
│
├── FlightStatus.Api/                  # .NET 8 Minimal API Backend
│   ├── Domain/
│   │   ├── Enums/
│   │   │   └── UnifiedFlightStatus.cs # Normalized status enum
│   │   └── Models/
│   │       └── FlightStatusResult.cs  # Unified internal domain model
│   ├── Dtos/
│   │   ├── AeroTrackResponse.cs       # Raw AeroTrack response payload
│   │   ├── QuickFlightResponse.cs     # Raw QuickFlight response payload
│   │   └── FlightStatusRequest.cs     # API Query parameter model with Data Annotations
│   ├── Middleware/
│   │   ├── GlobalExceptionHandler.cs  # Native .NET 8 Exception Handler (RFC 7807)
│   │   └── ValidationFilter.cs        # Reusable Endpoint Filter running validations
│   ├── Providers/
│   │   ├── IFlightStatusProvider.cs   # Strategy Pattern interface for vendors
│   │   ├── AeroTrackFlightStatusProvider.cs # Deterministic provider stub
│   │   └── QuickFlightFlightStatusProvider.cs# Deterministic provider stub
│   ├── Services/
│   │   ├── IFlightStatusService.cs    # Orchestration service interface
│   │   ├── FlightStatusService.cs     # Parallel lookups & timestamp selection
│   │   └── StatusNormalizer.cs        # Pure mapping methods for normalization
│   ├── Program.cs                     # API Routes, DI registrations, and Middlewares
│   └── Properties/
│       └── launchSettings.json        # API profiles (configured on port 5253)
│
├── FlightStatus.Tests/                # xUnit Test Suite
│   ├── StatusNormalizerTests.cs       # Verifies provider status translations
│   ├── FlightStatusServiceTests.cs    # Verifies selection, resilience, and fallback rules
│   ├── FlightStatusRequestValidatorTests.cs # Verifies input validation bounds
│   └── FlightStatusIntegrationTests.cs # End-to-end API integration tests (in-memory)
│
└── flight-status-ui/                  # Angular 19 SPA Frontend
    ├── src/
    │   ├── app/
    │   │   ├── components/
    │   │   │   ├── search/            # Form search component (Reactive Forms)
    │   │   │   └── result/            # Dynamic visual flight result card
    │   │   ├── models/
    │   │   │   └── flight-status.model.ts # TypeScript interfaces & enums
    │   │   ├── services/
    │   │   │   └── flight-status.service.ts # HttpClient client wrapper (RxJS)
    │   │   ├── app.config.ts          # Application providers configuration
    │   │   ├── app.routes.ts          # Angular SPA route definitions
    │   │   └── app.ts                 # Main application root bootstrap
    │   └── styles.css                 # Global theme, Outfit fonts, and scrollbars
    └── package.json                   # Angular dependency registry
```

---

## 2. Architecture & Design Decisions

### 2.1. Backend Design Patterns (.NET 8)
* **Strategy Pattern (`IFlightStatusProvider`)**: Encapsulates data fetching and payload mapping for individual vendors. Adding a new provider requires implementing this contract without modifying existing core logic (**Open/Closed Principle**).
* **Scatter-Gather Concurrency**: The orchestrator (`FlightStatusService`) fires off status lookups concurrently to all providers via `Task.WhenAll`. This ensures overall lookup latency is bound by the slowest single provider rather than the sum of all provider roundtrips.
* **Fault Isolation (Resilience)**: Individual provider tasks are wrapped in try-catch structures. If one provider fails (due to timeout or network crash), the service logs the error via **Structured Logging** and continues processing successful responses from remaining providers.
* **Anti-Corruption Layer (ACL)**: Raw vendor structures (`AeroTrackResponse` and `QuickFlightResponse`) are isolated as separate DTO records. Mappers parse these contracts and translate them into a unified domain model (`FlightStatusResult`), ensuring vendor schema changes do not leak into the core.
* **Aspect-Oriented Validation (AOP)**: Inputs are validated outside route handlers using an `IEndpointFilter`. The filter executes Data Annotations and FluentValidation rules, generating standardized RFC 7807 `ValidationProblem` responses. This keeps endpoints completely clean (**Single Responsibility Principle**).
* **Immutability (C# Records)**: Models are defined as `record` types with `init`-only properties, enforcing immutability and preventing accidental side effects during concurrent execution.
* **Modern Exception Lifecycle (`IExceptionHandler`)**: Uses .NET 8's native, high-performance global exception handler middleware to capture unhandled errors and format them into RFC 7807 `ProblemDetails` payloads.

### 2.2. Frontend Design Patterns (Angular 19+)
* **Standalone Architecture**: Components and routing are configured completely without legacy `NgModules` for clean dependency isolation.
* **Reactive Forms**: Form controls are managed dynamically with validation rules defined inside the component class, rather than embedded in template bindings.
* **Functional DI (`inject()`)**: Employs Angular's modern `inject` engine instead of constructor parameters.
* **Premium Theme (Vanilla CSS)**: Uses Outfit font weights, glassmorphism aesthetics (`backdrop-filter` blur, semi-transparent borders, and drop shadows), hover translations, and micro-animations to deliver a modern visual experience.

---

## 3. Assumptions & Stubs

* **Deterministic Mock Data**: The stubs respond to three specific test flight codes:
  * **`AI101`**: Configured as **OnTime** on both channels.
  * **`BA202`**: Configured as **Delayed** (90-minute delay). AeroTrack returns a terminal, gate, and delay reason; QuickFlight omits them.
  * **`UA303`**: Configured as **Cancelled** (AeroTrack returns a crew issue delay reason).
* **Conflict Resolution Offsets**: Timestamp offsets were strategically set up to test conflict resolution rules:
  * `AI101`: QuickFlight has the newer update (11:15 AM vs 11:00 AM UTC) $\rightarrow$ Service chooses QuickFlight.
  * `BA202`: AeroTrack has the newer update (3:45 PM vs 3:30 PM UTC) $\rightarrow$ Service chooses AeroTrack (including gate, terminal, and reason).
  * `UA303`: AeroTrack has the newer update (4:30 PM vs 4:00 PM UTC) $\rightarrow$ Service chooses AeroTrack (including crew reason).
* **Missing Query Parameters**: Unmapped flight requests are treated as "Not Found" (`null`) by both stubs, triggering the fallback system which returns a status of `Unknown`.

---

## 4. Setup, Run & Test Instructions

### 4.1. Prerequisites
* [.NET 8 SDK or newer](https://dotnet.microsoft.com/download)
* [Node.js (v18+) and npm](https://nodejs.org/)

### 4.2. Running the Backend API
From the root directory:
```bash
cd FlightStatus.Api
dotnet run
```
* **Ports**: The API starts on `http://localhost:5253` and `https://localhost:7174` (HTTPS).
* **Verification**: You can test the API health status by navigating to `http://localhost:5253/health`.

### 4.3. Running the Frontend UI
From the root directory:
```bash
cd flight-status-ui
npm install
npm start
```
* **Access**: Open `http://localhost:4200` in your web browser.
* **CORS Integration**: The API contains an active policy allowing requests from origin `http://localhost:4200`, preventing browser CORS blocking.

### 4.4. Running the Test Suite
From the root directory:
```bash
dotnet test
```
* Executing this command runs **60 xUnit test scenarios** with zero warnings or errors.
* **Unit Tests (49 scenarios)**: Verify request parameters validation syntax, stateless mapping normalizers, concurrent lookup orchestrators, and fallback policies in isolation.
* **Integration Tests (11 scenarios)**: Boot the application in-memory via `WebApplicationFactory<Program>` to test the full HTTP middleware pipeline (including CORS, CORS headers, ExceptionHandler, Serialization, and ValidationFilters) end-to-end.

---

## 5. Copilot & AI Usage

AI assistance was utilized systematically to review architectural designs, generate boilerplate structures, and create unit test frameworks. A detailed mapping of prompts, design decisions, and log traces is kept inside [prompts.md](file:///Users/umamahesh/Desktop/FlightStatus/prompts.md).
