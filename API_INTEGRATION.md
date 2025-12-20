# Integración con API Gateway - OpenAPI

El frontend consume las siguientes APIs del API Management Gateway según la especificación OpenAPI 3.0.

## 📋 Endpoints Consumidos

### 1. Canjear Cupón
```
POST /api/v1/coupons/redeem
```

**Request:**
```json
{
  "couponCode": "CUPON10OFF",
  "userId": "user-12345"
}
```

**Response (200):**
```json
{
  "success": true,
  "message": "Cupón canjeado exitosamente",
  "discount": 10.0,
  "couponCode": "CUPON10OFF",
  "campaignId": "CAMPAIGN-2025-BlackFriday",
  "redeemedAt": "2025-12-19T18:30:00Z"
}
```

### 2. Consultar Cupón
```
GET /api/v1/coupons/{code}
```

**Response (200):**
```json
{
  "code": "CUPON10OFF",
  "status": "active",
  "valid": true,
  "redeemed": false,
  "discount": 10.0,
  "expiresAt": "2025-12-31T23:59:59Z",
  "campaignId": "CAMPAIGN-2025-BlackFriday",
  "assignedTo": null
}
```

### 3. Generar Cupones Masivamente
```
POST /api/v1/campaigns/{id}/generate
```

**Request:**
```json
{
  "quantity": 100000
}
```

**Response (202):**
```json
{
  "success": true,
  "generated": 0,
  "message": "Generación iniciada",
  "requestId": "gen-req-abcdef123",
  "campaignId": 1,
  "status": "pending",
  "estimatedCompletionTime": "2025-12-19T18:45:00Z"
}
```

## 🔑 Headers Requeridos

Todas las requests incluyen:

```
x-api-version: 1.0
Content-Type: application/json
Authorization: Bearer {JWT_TOKEN}
```

## ⚙️ Configuración

### appsettings.json
```json
{
  "ApiGateway": {
    "BaseUrl": "http://localhost:7001",
    "Timeout": 30
  }
}
```

### Variables de Entorno (Azure)
```bash
ApiGateway__BaseUrl=https://coupons-apim.azure-api.net
ApiGateway__Timeout=30
```

## 🔄 Flujo de Datos

```
Frontend (Blazor)
    ↓
ICouponApiService
    ↓
HttpClient con BaseUrl + Headers
    ↓
API Gateway (APIM) - /api/v1/*
    ↓
Backend Microservices (AKS)
```

## 🛡️ Manejo de Errores

El servicio maneja los siguientes códigos de error según OpenAPI:

| Código | Significado | Acción |
|--------|-------------|--------|
| 200 | OK | Retorna datos |
| 202 | Accepted | Proceso asíncrono iniciado |
| 400 | Bad Request | Validación fallida |
| 401 | Unauthorized | Token inválido |
| 404 | Not Found | Cupón/Campaña no existe |
| 409 | Conflict | Cupón ya canjeado o expirado |
| 429 | Too Many Requests | Rate limit excedido |
| 500 | Internal Error | Error del servidor |

## 📊 Modelos de Datos

### RedeemResponse
```csharp
public record RedeemResponse(
    bool Success, 
    string Message, 
    decimal? Discount,
    string? CouponCode,
    string? CampaignId,
    DateTime? RedeemedAt
);
```

### CouponInfo
```csharp
public record CouponInfo(
    string Code, 
    string Status, 
    DateTime? ExpiresAt, 
    decimal? Discount,
    bool Valid,
    bool Redeemed,
    string? CampaignId,
    string? AssignedTo,
    DateTime? RedeemedAt
);
```

### GenerateResponse
```csharp
public record GenerateResponse(
    bool Success, 
    int Generated, 
    string Message,
    string? RequestId,
    int? CampaignId,
    string? Status,
    DateTime? EstimatedCompletionTime
);
```

## 🧪 Testing con Mock

Para probar localmente sin backend, puedes usar un servidor mock:

```bash
# Instalar Prism (mock server)
npm install -g @stoplight/prism-cli

# Ejecutar mock basado en OpenAPI
prism mock ../coupons-apis/openapi.yaml -p 7001
```

Esto levantará un servidor en `http://localhost:7001` que responderá con ejemplos de la especificación OpenAPI.

## 📚 Referencia Completa

Ver especificación completa en:
```
../coupons-apis/openapi.yaml
```
