# Coupons Frontend - .NET 8 Blazor

Frontend SPA simple para el Sistema de Cupones Promocionales.

## Características

- 🎫 **Canjear Cupones**: Interfaz para que los clientes canjeen sus cupones
- ⚙️ **Panel Admin**: Gestión de campañas y generación masiva de cupones
- 🚀 **Blazor Server**: Renderizado interactivo del lado del servidor
- 📱 **Responsive**: Diseño adaptativo con Bootstrap 5

## Tecnologías

- .NET 8
- ASP.NET Core Blazor Server
- Bootstrap 5
- HttpClient para consumo de APIs

## Estructura del Proyecto

```
├── Components/
│   ├── Pages/
│   │   ├── Home.razor          # Página principal
│   │   ├── Redeem.razor        # Página de canje de cupones
│   │   └── Admin.razor         # Panel administrativo
│   └── Layout/                 # Componentes de layout
├── Services/
│   ├── ICouponApiService.cs    # Interfaz del servicio
│   └── CouponApiService.cs     # Implementación del cliente HTTP
├── Program.cs                  # Configuración de la aplicación
└── appsettings.json           # Configuración (API Gateway URL)

```

## Ejecutar Localmente

```bash
# Restaurar dependencias
dotnet restore

# Ejecutar la aplicación
dotnet run

# Acceder a: https://localhost:5001
```

## Configuración

Editar `appsettings.json` para configurar la URL del API Gateway:

```json
{
  "ApiGateway": {
    "BaseUrl": "https://your-apim-gateway.azure-api.net"
  }
}
```

## Páginas Disponibles

### 1. Home (`/`)
Página de bienvenida con acceso rápido a las funcionalidades principales.

### 2. Canjear Cupón (`/redeem`)
- Formulario para ingresar código de cupón y usuario
- Validación en tiempo real
- Feedback inmediato del resultado

### 3. Panel Admin (`/admin`)
- Generación masiva de cupones
- Historial de generaciones
- Control de cantidad y campaña

## Docker

```bash
# Build
docker build -t coupons-frontend:latest .

# Run
docker run -d -p 8080:8080 \
  -e ApiGateway__BaseUrl=https://your-api-gateway \
  coupons-frontend:latest
```

## Deployment a Azure App Service

### Usando Azure CLI:

```bash
# Login
az login

# Crear App Service Plan
az appservice plan create --name coupons-frontend-plan \
  --resource-group coupons-rg \
  --sku B1 --is-linux

# Crear Web App
az webapp create --name coupons-frontend \
  --resource-group coupons-rg \
  --plan coupons-frontend-plan \
  --runtime "DOTNETCORE:8.0"

# Deploy desde código
az webapp deployment source config-zip \
  --resource-group coupons-rg \
  --name coupons-frontend \
  --src publish.zip

# Crear slot de staging
az webapp deployment slot create \
  --name coupons-frontend \
  --resource-group coupons-rg \
  --slot staging

# Configurar autoscale
az monitor autoscale create \
  --resource-group coupons-rg \
  --resource coupons-frontend \
  --resource-type Microsoft.Web/sites \
  --name autoscale-frontend \
  --min-count 1 \
  --max-count 5 \
  --count 2
```

## Variables de Entorno

| Variable | Descripción | Ejemplo |
|----------|-------------|---------|
| `ApiGateway__BaseUrl` | URL del API Gateway | `https://apim.azure-api.net` |
| `ASPNETCORE_ENVIRONMENT` | Ambiente | `Production` |

## Integración con APIM

El frontend consume estos endpoints del API Gateway:

- `POST /api/coupons/redeem` - Canjear cupón
- `GET /api/coupons/{code}` - Consultar cupón
- `POST /api/campaigns/{id}/generate` - Generar cupones masivamente

## Licencia

MIT
