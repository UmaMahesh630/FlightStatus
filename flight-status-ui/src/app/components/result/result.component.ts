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
   * Helper to determine status-specific styling classes
   */
  getStatusClass(status: UnifiedFlightStatus): string {
    return status === UnifiedFlightStatus.OnTime ? 'status-green'
         : status === UnifiedFlightStatus.Delayed ? 'status-amber'
         : status === UnifiedFlightStatus.Cancelled || status === UnifiedFlightStatus.Diverted ? 'status-red'
         : 'status-grey';
  }
}
