# Configuration Guide

## Frontend Setup

### Copy example file
```bash
# From the repository root
cp appsettings.Development.json.example appsettings.Development.json
```

Edit the file with your API service URLs if they differ from the defaults.

## Backend Services Setup

### 1. Copy example files
```bash
# From the repository root
cp coupons-backend-dotnet/src/CampaignService/CampaignService/appsettings.Development.json.example \
   coupons-backend-dotnet/src/CampaignService/CampaignService/appsettings.Development.json

cp coupons-backend-dotnet/src/RedeemService/RedeemService/appsettings.Development.json.example \
   coupons-backend-dotnet/src/RedeemService/RedeemService/appsettings.Development.json
```

### 2. Configure using .NET User Secrets (Recommended)

#### Campaign Service:
```bash
cd coupons-backend-dotnet/src/CampaignService/CampaignService
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:CampaignsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=campaigns-db;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet user-secrets set "ConnectionStrings:CouponsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=coupons-db;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

#### Redeem Service:
```bash
cd coupons-backend-dotnet/src/RedeemService/RedeemService
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:CouponsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=coupons-db;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### 3. Alternative: Edit appsettings.Development.json directly (Not Recommended)

⚠️ **WARNING**: This file is gitignored. Never commit credentials!

Edit the files with your actual connection strings, but **DO NOT COMMIT THEM**.

## Production Deployment

Use Azure Key Vault or App Service Configuration for production credentials.
