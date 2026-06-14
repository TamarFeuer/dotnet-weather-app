import { Component, output } from '@angular/core';

@Component({
  selector: 'app-month-picker',
  templateUrl: './month-picker.html',
})
export class MonthPicker {
  // The 12 months shown in the dropdown (data this component owns).
  protected readonly months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December',
  ];

  // An OUTPUT: this component emits the chosen month UP to its parent. The
  // parent listens with (monthSelected)="...". output() is Angular's modern
  // way to declare an event a component sends out.
  readonly monthSelected = output<string>();
}
