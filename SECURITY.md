# Security Guidelines

## 🔐 Handling Sensitive Configuration

### DO NOT commit sensitive data to Git:
- Database connection strings with passwords
- API keys
- Secrets or tokens
- Any credentials

### ✅ Use .NET User Secrets for local development:

```bash
# Navigate to your project
cd coupons-backend-dotnet/src/CampaignService/CampaignService

# Initialize user secrets
dotnet user-secrets init

# Set connection strings
dotnet user-secrets set "ConnectionStrings:CampaignsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=campaigns-db;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
dotnet user-secrets set "ConnectionStrings:CouponsDb" "Server=tcp:YOUR_SERVER.database.windows.net,1433;Initial Catalog=coupons-db;Persist Security Info=False;User ID=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=False;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

### ✅ Use Azure Key Vault for production:

Configure in `Program.cs`:
```csharp
if (builder.Environment.IsProduction())
{
    var keyVaultName = builder.Configuration["KeyVaultName"];
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{keyVaultName}.vault.azure.net/"),
        new DefaultAzureCredential());
}
```

### ✅ Use Environment Variables:

Set in Azure App Service → Configuration → Application Settings:
- `ConnectionStrings__CampaignsDb`
- `ConnectionStrings__CouponsDb`

## 🚨 If Credentials Are Exposed:

1. **Rotate credentials immediately** in Azure Portal
2. **Contact security team**
3. **Review access logs** for unauthorized access
4. **Update all services** with new credentials
