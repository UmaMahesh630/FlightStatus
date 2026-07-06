namespace FlightStatus.Api.Domain.Enums;

/// <summary>
/// Represents the canonical flight status values normalized within our application.
/// </summary>
/// <remarks>
/// ARCHITECTURE & DESIGN DECISION: Decoupled Domain Vocabulary
/// - **Strategy**: Clean Architecture / Domain-Driven Design (DDD).
/// - **Rationale**: External providers use conflicting vocabularies (e.g., "LATE" vs "DELAY", "CX" vs "CANCELLED").
///   Defining a canonical enum inside our Domain layer prevents provider-specific naming conventions from leaking 
///   into our business logic or frontend.
/// - **SOLID Principle**: Single Responsibility Principle (SRP). This enum does exactly one thing: define our 
///   internal business statuses.
/// </remarks>
public enum UnifiedFlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Diverted,
    Unknown
}
