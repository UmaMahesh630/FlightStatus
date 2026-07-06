# Flight Status Lookup - Technical Specification

This document defines the data models, provider schemas, normalization rules, and interface contracts for the SkyRoute Flight Status lookup system.

---

## 1. Data Models

### 1.1. Unified Status Enum

The application normalizes provider-specific statuses into the following unified enumeration:

| Unified Status | Meaning |
| :--- | :--- |
| `OnTime` | Departing or arrived within 15 minutes of the scheduled time. |
| `Delayed` | Departure or arrival pushed beyond 15 minutes of the scheduled time. |
| `Cancelled` | The flight will not operate. |
| `Diverted` | The flight landed at a different airport. |
| `Unknown` | The provider returned no usable status. |

In C#, this is represented as:

```csharp
public enum UnifiedStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Diverted,
    Unknown
}
```

---

### 1.2. Provider Schemas (Stubs)

#### AeroTrack (Verbose Provider)
AeroTrack returns verbose status information using its own naming conventions.

**Conceptual JSON Schema:**
```json
{
  "flightCode": "string",
  "operatingDate": "string (yyyy-MM-dd)",
  "status": "string", // e.g., "ON_TIME", "LATE", "CANCELLED", "DIVERTED"
  "scheduledDeparture": "string (ISO 8601)",
  "actualDeparture": "string (ISO 8601) or null",
  "scheduledArrival": "string (ISO 8601)",
  "actualArrival": "string (ISO 8601) or null",
  "departureTerminal": "string",
  "departureGate": "string",
  "arrivalTerminal": "string",
  "arrivalGate": "string",
  "delayReason": "string or null",
  "lastUpdated": "string (ISO 8601 UTC)"
}
```

#### QuickFlight (Minimal Provider)
QuickFlight returns minimal flight details. It has faster response times but does not include gate, terminal, or delay reason details.

**Conceptual JSON Schema:**
```json
{
  "flightNum": "string",
  "date": "string (yyyy-MM-dd)",
  "statusCode": "string", // e.g., "OK", "DELAY", "CX", "DIV"
  "scheduledDep": "string (ISO 8601)",
  "scheduledArr": "string (ISO 8601)",
  "updatedAtUtc": "string (ISO 8601 UTC)"
}
```

---

### 1.3. Normalized System Response (`FlightStatusResult`)

This is the unified contract returned by the API backend to the frontend.

```csharp
public class FlightStatusResult
{
    public string FlightNumber { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public UnifiedStatus Status { get; set; }
    public string StatusText => Status.ToString();
    
    // Scheduled and actual times (in local or UTC, normalized to ISO 8601 strings)
    public DateTime ScheduledDeparture { get; set; }
    public DateTime? ActualDeparture { get; set; }
    public DateTime ScheduledArrival { get; set; }
    public DateTime? ActualArrival { get; set; }

    // Optional AeroTrack-only fields (null if absent / QuickFlight used)
    public string? Terminal { get; set; }
    public string? Gate { get; set; }
    public string? DelayReason { get; set; }

    // Metadata for auditing/selection verification
    public string DataSource { get; set; } = string.Empty; // "AeroTrack" or "QuickFlight"
    public DateTime LastUpdatedUtc { get; set; }
}
```

---

## 2. Interface Definitions

To support dependency injection and abstract the data sources, the backend uses the following interfaces:

```csharp
public interface IFlightStatusProvider
{
    /// <summary>
    /// The unique identifier/name of the provider (e.g., "AeroTrack", "QuickFlight").
    /// </summary>
    string ProviderName { get; }

    /// <summary>
    /// Queries the provider for flight status. Returns null if the flight is not found or provider fails.
    /// </summary>
    Task<FlightStatusResult?> GetStatusAsync(string flightNumber, DateOnly date);
}
```

---

## 3. Business Logic & Normalization Rules

### 3.1. Status Value Mapping

The normalization layer must map the distinct vocabulary of each provider to the unified enum values:

| Provider | Raw Value | Mapped Unified Status |
| :--- | :--- | :--- |
| **AeroTrack** | `"ON_TIME"` | `UnifiedStatus.OnTime` |
| | `"LATE"` | `UnifiedStatus.Delayed` |
| | `"CANCELLED"` | `UnifiedStatus.Cancelled` |
| | `"DIVERTED"` | `UnifiedStatus.Diverted` |
| | *Any other value or null* | `UnifiedStatus.Unknown` |
| **QuickFlight**| `"OK"` | `UnifiedStatus.OnTime` |
| | `"DELAY"` | `UnifiedStatus.Delayed` |
| | `"CX"` | `UnifiedStatus.Cancelled` |
| | `"DIV"` | `UnifiedStatus.Diverted` |
| | *Any other value or null* | `UnifiedStatus.Unknown` |

---

### 3.2. Provider Selection Algorithm

When a request for `/flights/status?flightNumber={code}&date={yyyy-MM-dd}` is received:

1. **Query Phase**: Call both registered implementations of `IFlightStatusProvider` concurrently (or sequentially with a timeout).
2. **Evaluation Phase**:
   - **Scenario A (Both respond)**: Compare the `LastUpdatedUtc` timestamps of both results. Select the result with the **later** timestamp.
   - **Scenario B (Only one responds)**: Select that single successful response.
   - **Scenario C (Neither responds / both return null / exceptions)**: Return a `404 Not Found` or a result with `UnifiedStatus.Unknown` and a clear error/status message (e.g., "Flight status currently unavailable from all providers").

---

## 4. API Endpoint Contract

* **Endpoint**: `GET /flights/status`
* **Query Parameters**:
  * `flightNumber` (string, Required): The flight designator (e.g., "AA123", "QF9").
  * `date` (string, Required, Format: `yyyy-MM-dd`): The flight date.
* **Responses**:
  * `200 OK`: Returns a normalized `FlightStatusResult` object.
  * `400 BadRequest`: Returned if `flightNumber` or `date` is missing, or if `date` is in an invalid format.
  * `500 InternalServerError`: Returned for unhandled system exceptions.
