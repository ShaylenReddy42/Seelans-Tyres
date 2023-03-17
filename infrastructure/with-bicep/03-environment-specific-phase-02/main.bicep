targetScope = 'subscription'

@description('Specifies the location for resources.')
param location string = deployment().location

@allowed([
  'dev'
  'stage'
  'prod'
])
@description('Used to determine how the resources would be configured, instead of passing in params for all options')
param environment string = 'dev'

@secure()
@description('Used to Seed the Admin account to the database with the Administrator role')
param adminCredentialsEmail string

@secure()
@description('Password used to create the admin account')
param adminCredentialsPassword string

@secure()
@description('Email used to send emails')
param emailCredentialsEmail string

@secure()
@description('This is a generated app password')
param emailCredentialsPassword string

@secure()
@description('The health check endpoint starting with /')
param healthCheckEndpoint string

@secure()
@description('Used to authenticate the Mvc Frontend with IdentityServer4')
param mvcClientSecret string

@secure()
@description('Used to authenticate the MvcBff Gateway with IdentityServer4')
param mvcBffClientSecret string

@secure()
@description('The admin username for the server')
param sqlServerAdminLogin string

@secure()
@minLength(12)
@description('The admin password for the server')
param sqlServerAdminPassword string

resource resourceGroup 'Microsoft.Resources/resourceGroups@2022-09-01' existing = {
  name: 'rg-seelanstyres-${environment}'
}

module functionApp '01-function-app/template.bicep' = {
  scope: resourceGroup
  name: 'functionApp'
  params: {
    location: location
    environment: environment
  }
}

module appServicesLowLoad '02-app-services-low-load/template.bicep' = {
  scope: resourceGroup
  name: 'appServicesLowLoad'
  params: {
    location: location
    environment: environment
    healthCheckEndpoint: healthCheckEndpoint
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword
  }
}

module appServicesModerateLoad '03-app-services-moderate-load/template.bicep' = {
  scope: resourceGroup
  name: 'appServicesModerateLoad'
  params: {
    location: location
    environment: environment
    healthCheckEndpoint: healthCheckEndpoint
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword
  }
}

module appServicesHighLoad '04-app-services-high-load/template.bicep' = {
  scope: resourceGroup
  name: 'appServicesHighLoad'
  params: {
    location: location
    environment: environment
    adminCredentialsEmail: adminCredentialsEmail
    adminCredentialsPassword: adminCredentialsPassword
    emailCredentialsEmail: emailCredentialsEmail
    emailCredentialsPassword: emailCredentialsPassword
    healthCheckEndpoint: healthCheckEndpoint
    mvcClientSecret: mvcClientSecret
    mvcBffClientSecret: mvcBffClientSecret
    sqlServerAdminLogin: sqlServerAdminLogin
    sqlServerAdminPassword: sqlServerAdminPassword
  }
}
