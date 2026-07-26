// Production configuration.
//
// The backend URL is not known until deploy time (it contains the branch's
// unique hash), so the Deploy stage in azure-pipelines.yml replaces the
// placeholder below with the real backend URL just before it builds the app.
export const environment = {
	apiBaseUrl: 'API_BASE_URL_PLACEHOLDER',
};
