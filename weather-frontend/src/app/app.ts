import { Component, signal } from '@angular/core';

@Component({
  selector: 'app-root',
  imports: [],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('weather-frontend');

  // The component CLASS holds the data; the template (app.html) reads from here
  // to draw the screen. These are the 12 months the dropdown lists - the
  // template loops over this array to create one <option> per month.
  protected readonly months = [
    'January', 'February', 'March', 'April', 'May', 'June',
    'July', 'August', 'September', 'October', 'November', 'December',
  ];
}
