# Configuration Guide

## Local Development Setup

### 1. Copy example files
```bash
cp coupons-backend-dotnet/src/CampaignService/CampaignService/appsettings.Development.json.example coupons-backend-dotnet/src/CampaignService/CampaignService/appsettings.Development.json
cp coupons-backend-dotnet/src/RedeemService/RedeemService/appsettings.Development.json.example coupons-backend-dotnet/src/RedeemService/RedeemService/appsettings.Development.json
```

### 2. Configure using .NET User Secrets (Recommended)

#### Campaign Service:
```bash
cd coupons-backend-dotnet/src/CampaignService/CampaignService
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:CampaignsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=campaigns-db;User ID=YOUR_USER;Password=YOUR_PASSWORD;..."
dotnet user-secrets set "ConnectionStrings:CouponsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=coupons-db;User ID=YOUR_USER;Password=YOUR_PASSWORD;..."
```

#### Redeem Service:
```bash
cd coupons-backend-dotnet/src/RedeemService/RedeemService
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:CouponsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=coupons-db;User ID=YOUR_USER;Password=YOUR_PASSWORD;..."
```

### 3. Alternative: Edit appsettings.Development.json directly (Not Recommended)

⚠️ **WARNING**: This file is gitignored. Never commit credentials!

Edit the files with your actual connection strings, but **DO NOT COMMIT THEM**.

## Production Deployment

Use Azure Key Vault or App Service Configuration for production credentials.
