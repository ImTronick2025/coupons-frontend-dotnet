# Mock Backend para Testing Local

Este documento explica cómo ejecutar un backend simulado (mock) para probar el frontend sin necesidad de tener el backend real desplegado.

## 🎯 Opción 1: Prism Mock Server (Recomendado)

Prism genera automáticamente un servidor mock basado en la especificación OpenAPI.

### Instalación
```bash
npm install -g @stoplight/prism-cli
```

### Ejecución
```bash
# Desde el directorio raíz del proyecto
cd D:\CLOUDSOLUTIONS\cupones\coupons-apis
prism mock openapi.yaml -p 7001
```

### Salida esperada:
```
[CLI] …  awaiting  Starting Prism…
[CLI] ℹ  info      GET        http://127.0.0.1:7001/api/v1/coupons/{code}
[CLI] ℹ  info      POST       http://127.0.0.1:7001/api/v1/coupons/redeem
[CLI] ℹ  info      POST       http://127.0.0.1:7001/api/v1/campaigns/{id}/generate
[CLI] ▶  start     Prism is listening on http://127.0.0.1:7001
```

### Probar el mock:
```bash
# Canjear cupón
curl -X POST http://localhost:7001/api/v1/coupons/redeem \
  -H "Content-Type: application/json" \
  -d '{"couponCode":"CUPON10OFF","userId":"user-12345"}'

# Consultar cupón
curl http://localhost:7001/api/v1/coupons/CUPON10OFF

# Generar cupones
curl -X POST http://localhost:7001/api/v1/campaigns/1/generate \
  -H "Content-Type: application/json" \
  -d '{"quantity":1000}'
```

---

## 🎯 Opción 2: JSON Server (Más Simple)

Para un mock más simple sin validaciones.

### Instalación
```bash
npm install -g json-server
```

### Crear archivo de datos mock
Crear `mock-data.json`:
```json
{
  "coupons": [
    {
      "code": "CUPON10OFF",
      "status": "active",
      "valid": true,
      "redeemed": false,
      "discount": 10.0,
      "expiresAt": "2025-12-31T23:59:59Z",
      "campaignId": "CAMPAIGN-2025-BlackFriday"
    }
  ],
  "redeems": [],
  "generations": []
}
```

### Ejecución
```bash
json-server --watch mock-data.json --port 7001
```

---

## 🎯 Opción 3: WireMock (Más Control)

Para escenarios complejos con diferentes respuestas.

### Instalación (Docker)
```bash
docker run -it --rm \
  -p 7001:8080 \
  -v $(pwd)/wiremock:/home/wiremock \
  wiremock/wiremock
```

### Crear mappings
Crear `wiremock/mappings/redeem.json`:
```json
{
  "request": {
    "method": "POST",
    "url": "/api/v1/coupons/redeem"
  },
  "response": {
    "status": 200,
    "body": "{\"success\":true,\"message\":\"Cupón canjeado exitosamente\",\"discount\":10.0,\"couponCode\":\"CUPON10OFF\",\"campaignId\":\"CAMPAIGN-2025\",\"redeemedAt\":\"2025-12-19T20:30:00Z\"}",
    "headers": {
      "Content-Type": "application/json"
    }
  }
}
```

---

## 🎯 Opción 4: Minimal API en .NET (Custom)

Si prefieres .NET, crea un proyecto mínimo:

```bash
dotnet new web -n MockBackend
cd MockBackend
```

Editar `Program.cs`:
```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/api/v1/coupons/redeem", (RedeemRequest request) => 
{
    return Results.Ok(new 
    {
        success = true,
        message = "Cupón canjeado exitosamente",
        discount = 10.0,
        couponCode = request.CouponCode,
        campaignId = "CAMPAIGN-2025-BlackFriday",
        redeemedAt = DateTime.UtcNow
    });
});

app.MapGet("/api/v1/coupons/{code}", (string code) => 
{
    return Results.Ok(new 
    {
        code,
        status = "active",
        valid = true,
        redeemed = false,
        discount = 10.0,
        expiresAt = DateTime.UtcNow.AddDays(30),
        campaignId = "CAMPAIGN-2025-BlackFriday"
    });
});

app.MapPost("/api/v1/campaigns/{id}/generate", (int id, GenerateRequest request) => 
{
    return Results.Accepted("/api/v1/campaigns/1/jobs/mock-job-123", new 
    {
        success = true,
        generated = 0,
        message = "Generación iniciada",
        requestId = "mock-job-123",
        campaignId = id,
        status = "pending",
        estimatedCompletionTime = DateTime.UtcNow.AddMinutes(15)
    });
});

app.Run("http://localhost:7001");

record RedeemRequest(string CouponCode, string UserId);
record GenerateRequest(int Quantity);
```

Ejecutar:
```bash
dotnet run
```

---

## 🚀 Flujo de Testing con Mock

1. **Iniciar mock backend** (elegir una opción):
   ```bash
   prism mock openapi.yaml -p 7001
   ```

2. **Iniciar frontend**:
   ```bash
   cd D:\CLOUDSOLUTIONS\cupones\coupons-frontend-dotnet
   dotnet run
   ```

3. **Abrir navegador**: http://localhost:5244

4. **Probar funcionalidades**:
   - Canjear cupón → Debería retornar éxito
   - Consultar cupón → Debería mostrar info
   - Generar cupones → Debería mostrar job iniciado

---

## 🔧 Configuración del Frontend

El frontend ya está configurado para usar `http://localhost:7001` en desarrollo:

**appsettings.Development.json:**
```json
{
  "ApiGateway": {
    "BaseUrl": "http://localhost:7001"
  }
}
```

---

## 📊 Comparación de Opciones

| Opción | Pros | Contras | Mejor Para |
|--------|------|---------|-----------|
| **Prism** | ✅ Basado en OpenAPI<br>✅ Validación automática<br>✅ Ejemplos incluidos | ⚠️ Requiere Node.js | Testing completo con validación |
| **JSON Server** | ✅ Muy simple<br>✅ Rápido | ❌ Sin validación<br>❌ CRUD básico | Prototipos rápidos |
| **WireMock** | ✅ Muy flexible<br>✅ Escenarios complejos | ⚠️ Configuración manual | Testing de edge cases |
| **.NET Minimal API** | ✅ Mismo stack<br>✅ Control total | ⚠️ Más código | Custom logic |

---

## 🎓 Recomendación

Para este proyecto, usa **Prism Mock Server**:

```bash
# 1. Instalar Prism
npm install -g @stoplight/prism-cli

# 2. Ejecutar mock
cd D:\CLOUDSOLUTIONS\cupones\coupons-apis
prism mock openapi.yaml -p 7001

# 3. En otra terminal, ejecutar frontend
cd D:\CLOUDSOLUTIONS\cupones\coupons-frontend-dotnet
dotnet watch run

# 4. Abrir http://localhost:5244
```

✅ ¡Ahora puedes probar el frontend completo sin backend real!
