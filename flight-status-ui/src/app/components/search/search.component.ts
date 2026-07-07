import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { FlightStatusService } from '../../services/flight-status.service';
import { FlightStatusResult } from '../../models/flight-status.model';
import { ResultComponent } from '../result/result.component';

@Component({
  selector: 'app-search',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ResultComponent],
  templateUrl: './search.component.html',
  styleUrl: './search.component.css'
})
export class SearchComponent {
  private readonly fb = inject(FormBuilder);
  private readonly flightService = inject(FlightStatusService);

  protected readonly searchForm = this.fb.group({
    flightNumber: ['', [
      Validators.required, 
      Validators.pattern(/^[a-zA-Z0-9]{3,10}$/)
    ]],
    date: [new Date().toISOString().split('T')[0], [
      Validators.required
    ]]
  });

  protected readonly loading = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly result = signal<FlightStatusResult | null>(null);

  onSubmit(): void {
    if (this.searchForm.invalid) {
      this.searchForm.markAllAsTouched();
      return;
    }

    const flight = this.searchForm.value.flightNumber!.trim().toUpperCase();
    const targetDate = this.searchForm.value.date!;

    this.loading.set(true);
    this.error.set(null);
    this.result.set(null);

    this.flightService.getFlightStatus(flight, targetDate).subscribe({
      next: (res) => {
        this.result.set(res);
        this.loading.set(false);
      },
      error: (err) => {
        this.loading.set(false);
        this.parseAndSetError(err);
      }
    });
  }

  private parseAndSetError(err: any): void {
    if (err.status === 0) {
      this.error.set('Could not connect to the server. Please verify the backend is running.');
      return;
    }

    const errorPayload = err.error;

    if (errorPayload && errorPayload.errors) {
      const errorList = Object.entries(errorPayload.errors)
        .map(([field, messages]) => {
          const list = Array.isArray(messages) ? messages.join(', ') : messages;
          return `${field}: ${list}`;
        })
        .join(' | ');
      this.error.set(errorList);
      return;
    }

    if (errorPayload && errorPayload.error) {
      this.error.set(errorPayload.error);
      return;
    }

    if (errorPayload && errorPayload.detail) {
      this.error.set(errorPayload.detail);
      return;
    }

    this.error.set(`An unexpected error occurred (HTTP ${err.status}).`);
  }
}
