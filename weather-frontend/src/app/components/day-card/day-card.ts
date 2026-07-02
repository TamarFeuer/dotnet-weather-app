import { Component, input } from '@angular/core';
import { ForecastDay } from '../../forecast.service';

@Component({
  selector: 'app-day-card',
  templateUrl: './day-card.html',
  styleUrl: './day-card.css',
})
export class DayCard {
  // A presentational component: it holds NO store logic. One day's data flows
  // IN through input() (the signal input we met in Phase A), and the card just
  // renders it. required means the parent (ForecastGrid) must supply a day.
  readonly day = input.required<ForecastDay>();

  // Turn the ISO date string ("2026-07-03") into a friendly label ("Fri 3 Jul").
  label(iso: string): string {
    return new Date(iso).toLocaleDateString('en-GB', {
      weekday: 'short',
      day: 'numeric',
      month: 'short',
    });
  }
}
