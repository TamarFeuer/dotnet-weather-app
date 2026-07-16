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
  name: 'psql-${branchName}'          // e.g. psql-feature-branch-deploy
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
