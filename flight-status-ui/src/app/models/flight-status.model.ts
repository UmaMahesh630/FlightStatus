export enum UnifiedFlightStatus {
  OnTime = 'OnTime',
  Delayed = 'Delayed',
  Cancelled = 'Cancelled',
  Diverted = 'Diverted',
  Unknown = 'Unknown'
}

export interface FlightStatusResult {
  flightNumber: string;
  date: string;
  status: UnifiedFlightStatus;
  statusText: string;
  scheduledDeparture: string;
  actualDeparture?: string;
  scheduledArrival: string;
  actualArrival?: string;
  terminal?: string;
  gate?: string;
  delayReason?: string;
  dataSource: string;
  lastUpdatedUtc: string;
}
