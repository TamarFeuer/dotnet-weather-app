// ============================================================================
// Branch environment - infrastructure as code (Bicep)
// ============================================================================
// This file DESCRIBES the Azure resources a feature branch needs. It creates
// nothing on its own: a deployment (later, from the pipeline) reads this file
// and makes the resources. Everything is named after the branch, so each branch
// gets its own isolated copy and nothing collides.
//
// Starting small: for now this is only the PostgreSQL database. The app follows.

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
    }
  }
}
