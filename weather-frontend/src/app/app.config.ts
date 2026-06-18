import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideHttpClient } from '@angular/common/http';
import { provideStore } from '@ngrx/store';
import { provideEffects } from '@ngrx/effects';

export const appConfig: ApplicationConfig = {
	providers: [
		provideBrowserGlobalErrorListeners(),
		provideHttpClient(),
		// NgRx: set up the (empty for now) store and the effects runner.
		// We'll add the weather state slice and the effect in later steps.
		provideStore(),
		provideEffects(),
	]
};
