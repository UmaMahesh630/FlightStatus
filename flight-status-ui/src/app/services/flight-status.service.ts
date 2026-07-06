import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, catchError, throwError } from 'rxjs';
import { FlightStatusResult } from '../models/flight-status.model';

@Injectable({
  providedIn: 'root'
})
export class FlightStatusService {
  private readonly http = inject(HttpClient);
  
  // Minimal API backend endpoint URL
  private readonly apiUrl = 'http://localhost:5253/flights/status'; // We target the HTTP port from launchSettings.json

  /**
   * Queries the backend API for flight status details.
   */
  getFlightStatus(flightNumber: string, date: string): Observable<FlightStatusResult> {
    const params = new HttpParams()
      .set('flightNumber', flightNumber)
      .set('date', date);

    return this.http.get<FlightStatusResult>(this.apiUrl, { params }).pipe(
      catchError(error => {
        // Propagates error response containing ProblemDetails back to components
        return throwError(() => error);
      })
    );
  }
}
