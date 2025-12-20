# Script para desplegar el Frontend a Azure App Service

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$AppName,
    
    [Parameter(Mandatory=$true)]
    [string]$ApiGatewayUrl,
    
    [Parameter(Mandatory=$false)]
    [string]$Location = "eastus",
    
    [Parameter(Mandatory=$false)]
    [string]$Sku = "B1"
)

Write-Host "Desplegando Frontend a Azure App Service..." -ForegroundColor Green

# Crear Resource Group si no existe
Write-Host "Verificando Resource Group..." -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location

# Desplegar usando ARM template
Write-Host "Desplegando recursos con ARM template..." -ForegroundColor Yellow
az deployment group create `
    --resource-group $ResourceGroupName `
    --template-file azure-app-service.json `
    --parameters webAppName=$AppName apiGatewayUrl=$ApiGatewayUrl sku=$Sku

# Build y publicar la aplicación
Write-Host "Compilando aplicación .NET..." -ForegroundColor Yellow
dotnet publish -c Release -o ./publish

# Crear archivo ZIP
Write-Host "Creando archivo ZIP..." -ForegroundColor Yellow
Compress-Archive -Path ./publish/* -DestinationPath ./publish.zip -Force

# Obtener el nombre real del App Service
$webAppName = az deployment group show `
    --resource-group $ResourceGroupName `
    --name azure-app-service `
    --query properties.outputs.webAppUrl.value `
    --output tsv

# Deploy a staging slot primero
Write-Host "Desplegando a slot staging..." -ForegroundColor Yellow
az webapp deployment source config-zip `
    --resource-group $ResourceGroupName `
    --name $AppName `
    --slot staging `
    --src ./publish.zip

# Swap staging to production
Write-Host "Realizando swap de staging a production..." -ForegroundColor Yellow
az webapp deployment slot swap `
    --resource-group $ResourceGroupName `
    --name $AppName `
    --slot staging `
    --target-slot production

Write-Host "Deployment completado!" -ForegroundColor Green
Write-Host "URL Production: $webAppName" -ForegroundColor Cyan

# Cleanup
Remove-Item ./publish -Recurse -Force
Remove-Item ./publish.zip -Force
