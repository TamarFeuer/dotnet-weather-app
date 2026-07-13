import { Component, inject } from '@angular/core';
import { Store } from '@ngrx/store';
import { selectTypicalInfo } from '../../store/typical/typical.selectors';

@Component({
  selector: 'app-temperature-display',
  templateUrl: './temperature-display.html',
  styleUrl: './temperature-display.css',
})
export class TemperatureDisplay {
  // Read the weather info straight from the store. selectSignal turns a
  // selector into a signal, so the template still reads it as info() and stays
  // reactive - no @Input needed anymore.
  private store = inject(Store);
  readonly info = this.store.selectSignal(selectTypicalInfo);
}
