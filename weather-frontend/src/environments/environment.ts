// Development configuration (also used by the unit tests).
//
// A production build swaps this file for environment.prod.ts - see the
// fileReplacements rule in angular.json. So during development and testing the
// app talks to the local backend, and in the cloud it talks to the deployed one.
export const environment = {
	apiBaseUrl: 'http://localhost:5151/api/weather',
};
