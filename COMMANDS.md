# Comandos Útiles - Coupons Frontend

## Desarrollo Local

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar en modo desarrollo
dotnet run

# Ejecutar con watch (auto-reload)
dotnet watch run

# Build para producción
dotnet build -c Release

# Publicar
dotnet publish -c Release -o ./publish
```

## Docker

```bash
# Build imagen Docker
docker build -t coupons-frontend:latest .

# Run localmente con Docker
docker run -d -p 8080:8080 \
  -e ApiGateway__BaseUrl=https://localhost:7001 \
  --name coupons-frontend \
  coupons-frontend:latest

# Ver logs
docker logs -f coupons-frontend

# Stop y remove
docker stop coupons-frontend
docker rm coupons-frontend

# Push a Azure Container Registry
az acr login --name <your-acr-name>
docker tag coupons-frontend:latest <your-acr-name>.azurecr.io/coupons-frontend:latest
docker push <your-acr-name>.azurecr.io/coupons-frontend:latest
```

## Deployment Azure App Service

### Opción 1: Usando el script PowerShell

```powershell
# Login a Azure
az login

# Ejecutar script de deployment
.\deploy-azure.ps1 `
    -ResourceGroupName "coupons-rg" `
    -AppName "coupons-frontend" `
    -ApiGatewayUrl "https://coupons-apim.azure-api.net" `
    -Location "eastus" `
    -Sku "B1"
```

### Opción 2: Comandos manuales

```bash
# 1. Crear Resource Group
az group create --name coupons-rg --location eastus

# 2. Crear App Service Plan
az appservice plan create \
  --name coupons-frontend-plan \
  --resource-group coupons-rg \
  --sku B1 \
  --is-linux

# 3. Crear Web App
az webapp create \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --plan coupons-frontend-plan \
  --runtime "DOTNETCORE:8.0"

# 4. Configurar App Settings
az webapp config appsettings set \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --settings \
    ApiGateway__BaseUrl=https://your-apim.azure-api.net \
    ASPNETCORE_ENVIRONMENT=Production

# 5. Crear Staging Slot
az webapp deployment slot create \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging

# 6. Build y deploy
dotnet publish -c Release -o ./publish
cd publish
zip -r ../publish.zip .
cd ..

az webapp deployment source config-zip \
  --resource-group coupons-rg \
  --name coupons-frontend \
  --slot staging \
  --src publish.zip

# 7. Verificar staging
# Navegar a: https://coupons-frontend-staging.azurewebsites.net

# 8. Swap a production
az webapp deployment slot swap \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging \
  --target-slot production
```

## Autoscaling

```bash
# Configurar autoscale
az monitor autoscale create \
  --resource-group coupons-rg \
  --resource coupons-frontend \
  --resource-type Microsoft.Web/sites \
  --name autoscale-frontend \
  --min-count 1 \
  --max-count 5 \
  --count 2

# Agregar regla: Scale out cuando CPU > 70%
az monitor autoscale rule create \
  --resource-group coupons-rg \
  --autoscale-name autoscale-frontend \
  --condition "Percentage CPU > 70 avg 5m" \
  --scale out 1

# Agregar regla: Scale in cuando CPU < 30%
az monitor autoscale rule create \
  --resource-group coupons-rg \
  --autoscale-name autoscale-frontend \
  --condition "Percentage CPU < 30 avg 5m" \
  --scale in 1

# Ver configuración de autoscale
az monitor autoscale show \
  --resource-group coupons-rg \
  --name autoscale-frontend
```

## Monitoring y Logs

```bash
# Ver logs en tiempo real
az webapp log tail \
  --name coupons-frontend \
  --resource-group coupons-rg

# Descargar logs
az webapp log download \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --log-file logs.zip

# Habilitar Application Insights
az monitor app-insights component create \
  --app coupons-frontend-insights \
  --location eastus \
  --resource-group coupons-rg \
  --application-type web

# Conectar App Service con App Insights
INSTRUMENTATION_KEY=$(az monitor app-insights component show \
  --app coupons-frontend-insights \
  --resource-group coupons-rg \
  --query instrumentationKey -o tsv)

az webapp config appsettings set \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --settings APPINSIGHTS_INSTRUMENTATIONKEY=$INSTRUMENTATION_KEY
```

## Gestión de Slots

```bash
# Listar slots
az webapp deployment slot list \
  --name coupons-frontend \
  --resource-group coupons-rg

# Swap slots
az webapp deployment slot swap \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging \
  --target-slot production

# Configurar auto-swap
az webapp deployment slot auto-swap \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging

# Eliminar slot
az webapp deployment slot delete \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging
```

## Testing

```bash
# Test local
curl http://localhost:5000

# Test staging slot
curl https://coupons-frontend-staging.azurewebsites.net

# Test production
curl https://coupons-frontend.azurewebsites.net

# Test redeem endpoint (a través de APIM)
curl -X POST https://your-apim.azure-api.net/api/coupons/redeem \
  -H "Content-Type: application/json" \
  -d '{"couponCode":"TEST123","userId":"user@example.com"}'

# Test generate endpoint (a través de APIM)
curl -X POST https://your-apim.azure-api.net/api/campaigns/1/generate \
  -H "Content-Type: application/json" \
  -d '{"quantity":1000}'
```

## Cleanup

```bash
# Eliminar solo el Web App
az webapp delete \
  --name coupons-frontend \
  --resource-group coupons-rg

# Eliminar todo el Resource Group
az group delete \
  --name coupons-rg \
  --yes
```

## GitHub Actions Secrets

Para que funcione el CI/CD, configurar estos secrets en GitHub:

```bash
# Obtener publish profile para staging
az webapp deployment list-publishing-profiles \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging \
  --xml > staging-profile.xml

# Obtener credenciales de Azure para swap
az ad sp create-for-rbac \
  --name "coupons-frontend-deploy" \
  --role contributor \
  --scopes /subscriptions/{subscription-id}/resourceGroups/coupons-rg \
  --sdk-auth

# Agregar en GitHub Secrets:
# - AZURE_WEBAPP_PUBLISH_PROFILE_STAGING: contenido de staging-profile.xml
# - AZURE_CREDENTIALS: output del comando az ad sp create-for-rbac
```

## Variables de Entorno

```bash
# Local development (.env o appsettings.Development.json)
ApiGateway__BaseUrl=https://localhost:7001
ASPNETCORE_ENVIRONMENT=Development

# Azure App Service (configurar via portal o CLI)
ApiGateway__BaseUrl=https://your-apim.azure-api.net
ASPNETCORE_ENVIRONMENT=Production
```
