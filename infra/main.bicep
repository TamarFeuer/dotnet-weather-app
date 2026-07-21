// ============================================================================
// Branch environment - infrastructure as code (Bicep)
// ============================================================================
// This file DESCRIBES the Azure resources a feature branch needs. It creates
// nothing on its own: a deployment (later, from the pipeline) reads this file
// and makes the resources. Everything is named after the branch, so each branch
// gets its own isolated copy and nothing collides.
//
// It describes a whole branch environment: a PostgreSQL database, an App Service
// that runs the API, and the wiring between them (a firewall rule and the
// connection string).

// --- Inputs: values passed in from outside (the pipeline supplies them) ------

@description('The feature branch name. Used to name every resource so they are unique per branch.')
param branchName string

@description('Azure region. Defaults to the region of the resource group we deploy into.')
param location string = resourceGroup().location

@secure()
@description('The database admin password. @secure keeps it out of logs, and it is passed in rather than written here - the same rule as our connection string.')
param databasePassword string

// --- The database ------------------------------------------------------------

// Azure Database for PostgreSQL, "Flexible Server" - the managed PostgreSQL,
// the cloud equivalent of our local Docker container.
resource database 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  // CAF naming: <type>-<workload>-<environment>. The server's name becomes a
  // PUBLIC DNS name (...postgres.database.azure.com), so it must be globally
  // unique - uniqueString() adds a short deterministic hash per resource group.
  name: 'psql-weatherapi-${branchName}-${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_B1ms'             // the cheapest "Burstable" size
    tier: 'Burstable'
  }
  properties: {
    version: '17'                     // same major version as our dev container
    administratorLogin: 'weather'
    administratorLoginPassword: databasePassword
    storage: {
      storageSizeGB: 32               // the smallest allowed
    }
  }
}

// The actual database INSIDE the server. The server above is the building; this
// is the database our app connects to and EF Core migrates. Its name matches the
// Database= in the connection string. `parent: database` nests it in the server.
resource weatherDatabase 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: database
  name: 'weatherdb'
}

// A firewall rule so other Azure services (like our App Service) can reach the
// database. The special range 0.0.0.0-0.0.0.0 means "allow any Azure service" -
// the simplest option for a demo. A tighter setup would use a private network,
// which is a good "security by design" improvement for later.
resource allowAzureServices 'Microsoft.DBforPostgreSQL/flexibleServers/firewallRules@2024-08-01' = {
  parent: database
  name: 'AllowAllAzureServicesAndResourcesWithinAzureIps'
  properties: {
    startIpAddress: '0.0.0.0'
    endIpAddress: '0.0.0.0'
  }
}

// --- The compute that runs the API ------------------------------------------

// An App Service Plan is the machine that web apps run ON. One plan can host
// several apps. Linux, and the Free (F1) tier so the branch environment costs
// nothing to run.
resource plan 'Microsoft.Web/serverfarms@2024-04-01' = {
  // CAF: asp = App Service Plan (nothing to do with ASP.NET). Only unique within
  // the resource group, so no global-uniqueness hash needed.
  name: 'asp-weatherapi-${branchName}'
  location: location
  sku: {
    name: 'F1'
    tier: 'Free'
  }
  kind: 'linux'
  properties: {
    reserved: true                    // 'reserved: true' is how Azure marks a Linux plan
  }
}

// The App Service that runs the WeatherAPI backend - the cloud equivalent of
// `dotnet run`, on a public URL. It runs ON the plan above.
resource api 'Microsoft.Web/sites@2024-04-01' = {
  // CAF: app = web app. Like the database, its name becomes a public DNS name
  // (...azurewebsites.net), so it must be globally unique - hence uniqueString().
  name: 'app-weatherapi-${branchName}-${uniqueString(resourceGroup().id)}'
  location: location
  properties: {
    serverFarmId: plan.id             // reference to the plan: run this app on it
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'   // the .NET 10 runtime
      alwaysOn: false                     // the Free tier does not allow alwaysOn
      appSettings: [
        {
          // The exact environment variable the app already reads
          // (ConnectionStrings__WeatherDb), built from the server's address and
          // the password. This is how the deployed API finds its database, the
          // same mechanism the integration tests use. Ssl Mode=Require because
          // Azure PostgreSQL only accepts encrypted connections.
          name: 'ConnectionStrings__WeatherDb'
          value: 'Host=${database.properties.fullyQualifiedDomainName};Port=5432;Database=weatherdb;Username=weather;Password=${databasePassword};Ssl Mode=Require'
        }
        {
          // Let the deployed backend accept requests from the deployed frontend,
          // which lives on a different origin. The app reads this as the
          // Cors__AllowedOrigins config value; here it is the Storage static-website
          // URL. Referencing the storage account makes Bicep create it first.
          name: 'Cors__AllowedOrigins'
          value: frontend.properties.primaryEndpoints.web
        }
      ]
    }
  }
}

// --- The frontend host -------------------------------------------------------

// A Storage account whose "static website" feature serves the built Angular app
// on a public URL. Static files need no compute, so this is far cheaper than
// serving them from the App Service. Storage account names are global DNS names
// and allow only lowercase letters and digits (no hyphens), max 24 characters -
// so there is no room for the branch name. CAF prefix st + weather + fe
// (frontend), then the per-resource-group hash, which is already unique per
// branch. st(2) + weatherfe(9) + hash(13) = 24, exactly the maximum.
resource frontend 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: 'stweatherfe${uniqueString(resourceGroup().id)}'
  location: location
  sku: {
    name: 'Standard_LRS'              // cheapest redundancy: locally redundant
  }
  kind: 'StorageV2'                   // required for static website hosting
  properties: {
    allowBlobPublicAccess: true       // the static site is served anonymously
    minimumTlsVersion: 'TLS1_2'
  }
}

// Note: turning ON the static website feature is a data-plane action Bicep
// cannot express, so the pipeline enables it with `az storage blob
// service-properties update --static-website` right before it uploads the files.

// --- Outputs: values the pipeline reads back after deploying -----------------

// The App Service's generated name (it includes the uniqueString hash, so the
// pipeline cannot guess it). The deploy stage reads this to know which app to
// push the code to.
output appName string = api.name

// The public URL of the deployed API, so the pipeline can report where the
// branch environment is running.
output appUrl string = 'https://${api.properties.defaultHostName}'

// The static-website URL of the deployed frontend. The pipeline uploads the
// built Angular app here, and posts this URL on the pull request.
output frontendUrl string = frontend.properties.primaryEndpoints.web
