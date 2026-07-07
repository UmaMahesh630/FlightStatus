namespace FlightStatus.Api.Domain.Enums;

/// <summary>
/// Canonical flight status values normalized within the application domain.
/// </summary>
/// <remarks>
/// Acts as an Anti-Corruption Layer boundary to isolate vendor status vocabularies 
/// (e.g. "LATE" vs "ON_SCHEDULE") from internal models and frontend applications.
/// </remarks>
public enum UnifiedFlightStatus
{
    OnTime,
    Delayed,
    Cancelled,
    Diverted,
    Unknown
}
