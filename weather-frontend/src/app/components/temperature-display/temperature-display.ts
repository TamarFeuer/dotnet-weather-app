import { Component, input } from '@angular/core';
import { WeatherInfo } from '../../weather.service';

@Component({
  selector: 'app-temperature-display',
  templateUrl: './temperature-display.html',
  styleUrl: './temperature-display.css',
})
export class TemperatureDisplay {
  // An INPUT: the parent passes the weather info DOWN into this component.
  // input() is Angular's modern way to declare data a component receives;
  // it's a signal, so the template reads it as info().
  readonly info = input<WeatherInfo | null>(null);
}
