# prompts.md

## AI Usage Summary

AI tools used:
- GitHub Copilot
- Antigravity

All generated code was reviewed and modified before being committed.

---

# Prompt 1 – Define Architecture

### Prompt

Design a .NET 8 Minimal API solution for a Flight Status lookup system with an Angular frontend. Use clean architecture principles, dependency injection, provider abstraction, unit testing, and stub providers.

### Result

Accepted

### Notes

The suggested architecture matched the requirements. Folder names were slightly renamed to fit the project.

---

# Prompt 2 – Generate Unified Models

### Prompt

Generate unified FlightStatusResult model, provider DTOs, and UnifiedFlightStatus enum for AeroTrack and QuickFlight.

### Result

Accepted

### Notes

Minor property name changes were made for consistency.

---

# Prompt 3 – Generate Provider Interface

### Prompt

Generate an IFlightStatusProvider interface suitable for multiple provider implementations.

### Result

Accepted

### Notes

Changed the method name from GetFlightAsync() to GetFlightStatusAsync() to better reflect its purpose.

---

# Prompt 4 – Generate Stub Providers

### Prompt

Generate AeroTrackProvider and QuickFlightProvider with deterministic hardcoded responses.

### Result

Accepted

### Notes

Added additional hardcoded flights to improve testing.

---

# Prompt 5 – Initial Service Generation

### Prompt

Generate a service that retrieves data from both providers and returns the latest response.

### Result

Rejected

### Reason

The generated service called the providers sequentially and directly referenced the concrete provider classes. This did not satisfy the dependency injection and provider abstraction requirements.

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

This implementation aligned with the required business rules and architecture.

---

# Prompt 7 – Status Normalization

### Prompt

Generate a mapper that converts AeroTrack and QuickFlight status values into the UnifiedFlightStatus enum.

### Result

Accepted

### Notes

Added Unknown as the default value for unmapped statuses.

---

# Prompt 8 – Minimal API Endpoint

### Prompt

Generate a Minimal API endpoint for GET /flights/status with dependency injection, input validation, and ProblemDetails responses.

### Result

Accepted

### Notes

Adjusted route naming to match project conventions.

---

# Prompt 9 – Angular Search Page

### Prompt

Generate an Angular search page using Reactive Forms for flight number and date with validation.

### Result

Accepted

---

# Prompt 10 – Angular Result Card

### Prompt

Generate an Angular result card that displays unified flight status and AeroTrack-specific fields only when available.

### Result

Accepted

### Notes

Modified spacing and Bootstrap classes.

---

# Prompt 11 – Dynamic Status Colors

### Prompt

Generate dynamic badge and card colors based on unified flight status.

### Result

Rejected

### Reason

The generated HTML used fixed Bootstrap classes, causing the badge to remain red regardless of the actual status.

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

Used ngClass to bind CSS classes based on the normalized status.

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

---

# Prompt 14 – README

### Prompt

Generate a professional README containing setup steps, assumptions, architecture, testing instructions, and project structure.

### Result

Accepted

### Notes

Added screenshots and additional setup instructions manually.

---

# Prompt 15 – Logging

### Prompt

Generate structured logging for provider failures and request processing.

### Result

Rejected

### Reason

The generated logging used Console.WriteLine() instead of ILogger.

---

# Prompt 16 – Refined Logging

### Prompt

Generate structured logging using ILogger for provider calls, failures, and provider selection.

### Result

Accepted

---

# Prompt 17 – Global Exception Handling

### Prompt

Generate middleware that converts unhandled exceptions into ProblemDetails responses.

### Result

Accepted

---

# Prompt 18 – Reflection Document

### Prompt

Generate reflection.md describing assumptions, trade-offs, lessons learned, and future improvements.

### Result

Accepted
