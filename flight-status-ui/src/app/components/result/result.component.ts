import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FlightStatusResult, UnifiedFlightStatus } from '../../models/flight-status.model';

@Component({
  selector: 'app-result',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './result.component.html',
  styleUrl: './result.component.css'
})
export class ResultComponent {
  @Input() result!: FlightStatusResult;

  // Expose status enum to template
  protected readonly FlightStatus = UnifiedFlightStatus;

  /**
   * Helper to determine status-specific styling classes.
   * Defensively supports both string enums and numeric representations from JSON.
   */
  getStatusClass(status: any): string {
    const isOnTime = status === UnifiedFlightStatus.OnTime || status === 0 || status === '0' || status === 'OnTime';
    const isDelayed = status === UnifiedFlightStatus.Delayed || status === 1 || status === '1' || status === 'Delayed';
    const isCancelledOrDiverted = status === UnifiedFlightStatus.Cancelled || status === 2 || status === '2' || status === 'Cancelled' ||
                                  status === UnifiedFlightStatus.Diverted || status === 3 || status === '3' || status === 'Diverted';

    if (isOnTime) return 'status-green';
    if (isDelayed) return 'status-amber';
    if (isCancelledOrDiverted) return 'status-red';
    return 'status-grey';
  }
}
