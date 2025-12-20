# Coupons Frontend - Project Overview

## 📋 Resumen del Proyecto

Frontend SPA desarrollado en **Blazor Server (.NET 8)** para el Sistema de Cupones Promocionales. Incluye interfaz de usuario para canje de cupones y panel administrativo para gestión masiva.

## 🏗️ Arquitectura

```
┌─────────────────┐
│   Frontend      │
│  (Blazor SPA)   │
└────────┬────────┘
         │
         ▼
┌─────────────────┐
│   API Gateway   │
│     (APIM)      │
└────────┬────────┘
         │
         ├──────────────┐
         │              │
         ▼              ▼
┌──────────────┐  ┌──────────────┐
│   Redeem     │  │  Campaign    │
│   Service    │  │  Service     │
│   (AKS)      │  │  (AKS)       │
└──────────────┘  └──────┬───────┘
                         │
                         ▼
                  ┌──────────────┐
                  │ Coupon Gen   │
                  │    (ACI)     │
                  └──────────────┘
```

## 📁 Estructura del Proyecto

```
coupons-frontend-dotnet/
├── .github/workflows/          # CI/CD pipelines
│   ├── deploy.yml             # Azure App Service deployment
│   └── docker.yml             # Docker build & push
├── Components/                # Blazor components
│   ├── Layout/
│   │   ├── MainLayout.razor
│   │   └── NavMenu.razor
│   └── Pages/
│       ├── Home.razor         # Landing page
│       ├── Redeem.razor       # Coupon redemption
│       └── Admin.razor        # Admin panel
├── Services/                  # Business logic
│   ├── ICouponApiService.cs
│   └── CouponApiService.cs    # HTTP client for APIM
├── wwwroot/                   # Static files
├── Program.cs                 # App configuration
├── appsettings.json          # Configuration
├── Dockerfile                 # Container definition
├── azure-app-service.json    # ARM template
├── deploy-azure.ps1          # Deployment script
├── README.md                  # Documentation
├── COMMANDS.md               # Useful commands
└── .gitignore

```

## 🎯 Características Principales

### 1. Canjear Cupón (`/redeem`)
- ✅ Formulario simple con validación
- ✅ Llamada a API Gateway → Redeem Service
- ✅ Feedback visual del resultado
- ✅ Muestra descuento aplicado

### 2. Panel Admin (`/admin`)
- ✅ Generación masiva de cupones
- ✅ Configuración de campaña y cantidad
- ✅ Historial de generaciones
- ✅ Estado de trabajos (ACI)

### 3. Servicio de API
- ✅ HttpClient configurado con Dependency Injection
- ✅ Integración con API Gateway (APIM)
- ✅ Manejo de errores
- ✅ Modelos tipados (records)

## 🚀 Deployment

### Azure App Service (Recomendado)

**Características:**
- ✅ 2 Slots: `staging` + `production`
- ✅ Autoscale: 1-5 instancias
- ✅ Swap sin downtime
- ✅ HTTPS forzado
- ✅ Logs integrados

**Quick Deploy:**
```powershell
.\deploy-azure.ps1 `
    -ResourceGroupName "coupons-rg" `
    -AppName "coupons-frontend" `
    -ApiGatewayUrl "https://your-apim.azure-api.net"
```

### Docker (Alternativo)

```bash
docker build -t coupons-frontend:latest .
docker run -p 8080:8080 \
  -e ApiGateway__BaseUrl=https://your-apim \
  coupons-frontend:latest
```

## 🔧 Configuración

### Variables de Entorno

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ApiGateway__BaseUrl` | URL del API Gateway | `https://apim.azure-api.net` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente | `Production` / `Staging` |

### appsettings.json

```json
{
  "ApiGateway": {
    "BaseUrl": "https://localhost:7001"
  }
}
```

## 📊 Flujo de Datos

### Canje de Cupón
```
User → Frontend → APIM → Redeem Service → Validation → Response
```

### Generación Masiva
```
Admin → Frontend → APIM → Campaign Service → ACI Job → Bulk Generation
```

## 🧪 Testing

```bash
# Local
dotnet run
curl http://localhost:5000

# Staging
curl https://coupons-frontend-staging.azurewebsites.net

# Production
curl https://coupons-frontend.azurewebsites.net
```

## 📈 Monitoring

### Application Insights
- Request/response times
- Error rates
- Custom events
- User tracking

### App Service Logs
```bash
az webapp log tail --name coupons-frontend --resource-group coupons-rg
```

## 🔐 Seguridad

- ✅ HTTPS only
- ✅ HSTS enabled
- ✅ TLS 1.2 minimum
- ✅ Antiforgery tokens
- ✅ Input validation
- ⚠️ TODO: Implementar autenticación (JWT/OAuth2)

## 📦 Dependencias

- Microsoft.AspNetCore.Components.Web (incluido en .NET 8)
- System.Net.Http.Json (incluido en .NET 8)
- Bootstrap 5 (CDN)

## 🎨 UI/UX

- **Framework CSS**: Bootstrap 5
- **Estilo**: Responsive, mobile-first
- **Componentes**:
  - Formularios con validación
  - Spinners de carga
  - Alerts para feedback
  - Cards para organización
  - Tables para historial

## 🔄 CI/CD Pipeline

### GitHub Actions Workflow

1. **Build**
   - Restore dependencies
   - Build project
   - Run tests (si existen)
   - Publish artifacts

2. **Deploy to Staging**
   - Download artifacts
   - Deploy to staging slot
   - Smoke tests

3. **Deploy to Production**
   - Swap staging → production
   - Verify production
   - Rollback if needed

## 📝 Notas Técnicas

### Blazor Server vs WebAssembly
- **Elegido**: Blazor Server
- **Razón**: Simplicidad, menor tamaño de payload inicial
- **Trade-off**: Requiere conexión persistente (SignalR)

### Autoscale Configuration
- **Min**: 1 instancia
- **Max**: 5 instancias
- **Trigger**: CPU > 70% → scale out
- **Cool down**: 5 minutos

### Slot Configuration
- **Production**: Auto-swap deshabilitado (manual control)
- **Staging**: Testing ground para nuevos deployments
- **Swap time**: ~30 segundos sin downtime

## 🎓 Learning Resources

- [ASP.NET Core Blazor](https://learn.microsoft.com/aspnet/core/blazor/)
- [Azure App Service](https://learn.microsoft.com/azure/app-service/)
- [Deployment Slots](https://learn.microsoft.com/azure/app-service/deploy-staging-slots)
- [Autoscale](https://learn.microsoft.com/azure/app-service/manage-scale-up)

## 🤝 Integración con Otros Componentes

### Backend (AKS)
- Consume endpoints via APIM
- No conexión directa
- Todos los requests pasan por gateway

### APIM (API Management)
- Rate limiting
- JWT validation (TODO)
- Request/response transformation
- Caching policies

### ACI (Container Instances)
- Triggered indirectamente via Campaign Service
- No interacción directa desde frontend

## 🐛 Troubleshooting

### Error: Cannot connect to APIM
```bash
# Verificar configuración
az webapp config appsettings list --name coupons-frontend --resource-group coupons-rg

# Verificar conectividad
curl -v https://your-apim.azure-api.net/api/health
```

### Error: Swap fails
```bash
# Verificar slot health
az webapp show --name coupons-frontend --slot staging --resource-group coupons-rg

# Force swap
az webapp deployment slot swap --name coupons-frontend --slot staging --resource-group coupons-rg --target-slot production --preserve-vnet
```

## 📌 TODO / Mejoras Futuras

- [ ] Implementar autenticación (Azure AD B2C)
- [ ] Agregar tests unitarios e integración
- [ ] Implementar caché del lado del cliente
- [ ] Mejorar manejo de errores y retry logic
- [ ] Agregar telemetría custom
- [ ] Implementar feature flags
- [ ] Agregar soporte para i18n
- [ ] Progressive Web App (PWA) support

## 📄 Licencia

MIT License - Ver LICENSE file para detalles.
