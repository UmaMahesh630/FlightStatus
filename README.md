# Flight Status Lookup System

A modern full-stack application built for support agents to look up and display normalized flight status information from multiple providers.

## Tech Stack
- **Backend**: .NET 8 (Minimal API) with Dependency Injection and concurrency handling.
- **Frontend**: Angular 19+ (Standalone Components, modern reactive design, vanilla CSS).
- **Testing**: xUnit with unit tests covering status normalization and provider selection logic.

---

## Getting Started

### Prerequisites
- [.NET 8 SDK or newer](https://dotnet.microsoft.com/download)
- [Node.js (v18+) and npm](https://nodejs.org/)

### Quick Start

#### 1. Running the Backend
From the root directory:
```bash
cd FlightStatus.Api
dotnet run
```
The API will start and listen on standard ports (typically `http://localhost:5000` or `https://5001`).

#### 2. Running the Frontend
From the root directory:
```bash
cd flight-status-ui
npm install
npm start
```
Open `http://localhost:4200` in your browser.

#### 3. Running Unit Tests
From the root directory:
```bash
dotnet test
```

---

## Architectural Decisions & Patterns

The codebase is organized following senior-level clean architecture principles:
- **Dependency Injection (DI)**: Interfaces decouple concrete provider implementations (`AeroTrack` and `QuickFlight`) from endpoint handlers.
- **Task Concurrency**: Multiple provider endpoints are queried concurrently using `Task.WhenAll` to ensure optimal lookup latency.
- **Normalization Layer**: Maps provider-specific domains to a single, unified status vocabulary (`OnTime`, `Delayed`, `Cancelled`, `Diverted`, `Unknown`).
- **Resilient Fallbacks**: Graceful handling of single provider failures, with smart timestamps comparison (`LastUpdatedUtc`) when both providers respond.
