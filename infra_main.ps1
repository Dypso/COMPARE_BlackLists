# Azure Infrastructure Deployment Script for Blacklist Benchmark
param(
    [Parameter(Mandatory=$true)]
    [string]$resourceGroupName,
    
    [Parameter(Mandatory=$false)]
    [string]$location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$appName = "blacklist-benchmark"
)

# Function to check if Azure CLI is installed and user is logged in
function Test-AzureCliPrerequisites {
    try {
        Write-Host "Checking Azure CLI installation..."
        $null = az --version
        
        Write-Host "Checking Azure CLI login status..."
        $account = az account show | ConvertFrom-Json
        Write-Host "Connected to Azure subscription: $($account.name)"
        return $true
    }
    catch {
        Write-Error "Azure CLI is not installed or you're not logged in. Please install Azure CLI and run 'az login'"
        return $false
    }
}

# Function to create or update resource group
function New-ResourceGroup {
    param($name, $location)
    
    Write-Host "Creating/updating resource group '$name' in location '$location'..."
    az group create --name $name --location $location
    
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create resource group"
    }
}

# Function to create App Service Plan
function New-AppServicePlan {
    param($resourceGroup, $name, $location)
    
    Write-Host "Creating App Service Plan..."
    az appservice plan create `
        --resource-group $resourceGroup `
        --name "$name-plan" `
        --location $location `
        --sku F1 `
        --is-linux false

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create App Service Plan"
    }
}

# Function to create Redis Cache
function New-RedisCache {
    param($resourceGroup, $name, $location)
    
    Write-Host "Creating Redis Cache..."
    az redis create `
        --resource-group $resourceGroup `
        --name "$name-redis" `
        --location $location `
        --sku Basic `
        --vm-size C0 `
        --enable-non-ssl-port

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create Redis Cache"
    }
    
    # Get Redis connection string
    $redisKeys = az redis list-keys --resource-group $resourceGroup --name "$name-redis" | ConvertFrom-Json
    return "$name-redis.redis.cache.windows.net:6379,password=$($redisKeys.primaryKey),ssl=True,abortConnect=False"
}

# Function to create Application Insights
function New-ApplicationInsights {
    param($resourceGroup, $name, $location)
    
    Write-Host "Creating Application Insights..."
    $appInsights = az monitor app-insights component create `
        --resource-group $resourceGroup `
        --app "$name-insights" `
        --location $location `
        --application-type web `
        --kind web `
        --sampling-percentage 10 | ConvertFrom-Json

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create Application Insights"
    }
    
    return $appInsights.connectionString
}

# Function to create Web App
function New-WebApp {
    param(
        $resourceGroup,
        $name,
        $location,
        $redisConnectionString,
        $appInsightsConnectionString
    )
    
    Write-Host "Creating Web App..."
    az webapp create `
        --resource-group $resourceGroup `
        --name $name `
        --plan "$name-plan" `
        --runtime "dotnet:8"

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to create Web App"
    }
    
    # Configure app settings
    Write-Host "Configuring Web App settings..."
    az webapp config appsettings set `
        --resource-group $resourceGroup `
        --name $name `
        --settings `
            APPLICATIONINSIGHTS_CONNECTION_STRING=$appInsightsConnectionString `
            ConnectionStrings__Redis=$redisConnectionString `
            ASPNETCORE_ENVIRONMENT=Production

    if ($LASTEXITCODE -ne 0) {
        throw "Failed to configure Web App settings"
    }
}

# Main deployment script
try {
    # Check prerequisites
    if (-not (Test-AzureCliPrerequisites)) {
        exit 1
    }

    # Create/update resource group
    New-ResourceGroup -name $resourceGroupName -location $location

    # Create App Service Plan
    New-AppServicePlan -resourceGroup $resourceGroupName -name $appName -location $location

    # Create Redis Cache and get connection string
    $redisConnectionString = New-RedisCache -resourceGroup $resourceGroupName -name $appName -location $location

    # Create Application Insights and get connection string
    $appInsightsConnectionString = New-ApplicationInsights -resourceGroup $resourceGroupName -name $appName -location $location

    # Create and configure Web App
    New-WebApp `
        -resourceGroup $resourceGroupName `
        -name $appName `
        -location $location `
        -redisConnectionString $redisConnectionString `
        -appInsightsConnectionString $appInsightsConnectionString

    Write-Host "`nDeployment completed successfully!"
    Write-Host "Web App URL: https://$appName.azurewebsites.net"
}
catch {
    Write-Error "Deployment failed: $_"
    exit 1
}