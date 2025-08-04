# Exchange Rate Offers API 💱

Una API robusta para obtener las mejores ofertas de cambio de divisa comparando múltiples proveedores en tiempo real.

## 🚀 Características

- **Comparación Multi-Proveedor**: Consulta simultánea a múltiples APIs de tipo de cambio
- **Mejor Oferta**: Selección automática de la tasa más favorable
- **Arquitectura Limpia**: Implementación usando Clean Architecture y DDD
- **Validación Robusta**: Validación de entrada usando FluentValidation
- **Documentación**: API completamente documentada con Swagger
- **Logging**: Logging estructurado con Serilog
- **Contenedorización**: Completamente dockerizado para fácil despliegue
- **Testing**: Suite completa de tests unitarios e integración

## 🏗️ Arquitectura

```
┌─────────────────────┐
│  Presentation.Api   │  ← Controllers, Middleware
├─────────────────────┤
│  Core.Application   │  ← Services, DTOs, Validators
├─────────────────────┤
│    Core.Domain      │  ← Entities, Exceptions
├─────────────────────┤
│Infrastructure.Shared│  ← External APIs, HttpClients
└─────────────────────┘
```

## 🐳 Instalación y Ejecución con Docker

### Prerrequisitos

- Docker Desktop
- Git

### Pasos de Instalación

1. **Clonar el repositorio**
```bash
git clone https://github.com/GeraldSilverio/ExchangeRateOffers.git
cd ExchangeRateOffers
```

2. **Construir y ejecutar con Docker Compose**
```bash
docker-compose up --build
```

3. **Verificar que todos los servicios estén ejecutándose**
```bash
docker ps
```

### Servicios Disponibles

| Servicio | Puerto | URL | Descripción |
|### Credenciales de Prueba

Para testing y desarrollo, puedes usar estas credenciales:

| Campo | Valor |
|-------|-------|
| **Email** | `banreservas-test@gmail.com` |
| **Password** | `1234567@` |
| **API Key** | `AIzaSyBzSwJ77sMtB_HXCLWahlv7jBWs7ifFJfA` |

> ⚠️ **Nota**: Estas son credenciales de prueba. En producción, usa tus propias credenciales de Firebase.

----------|--------|-----|-------------|
| API Principal | 5000 | http://localhost:5000 | API principal de Exchange Rate |
| Swagger UI | 5000 | http://localhost:5000/swagger | Documentación interactiva |
| Mock Provider 1 | 5002 | http://localhost:5002 | Simulador de proveedor 1 |
| Mock Provider 2 | 5003 | http://localhost:5003 | Simulador de proveedor 2 |
| Mock Provider 3 | 5004 | http://localhost:5004 | Simulador de proveedor 3 |

## 🔐 Autenticación

La API utiliza Firebase Authentication para proteger los endpoints. Debes obtener un token de acceso antes de realizar peticiones a la API.

### Obtener Token de Autenticación

**Endpoint de Login:**
```
POST https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyBzSwJ77sMtB_HXCLWahlv7jBWs7ifFJfA
```

**Request:**
```bash
curl --location 'https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyBzSwJ77sMtB_HXCLWahlv7jBWs7ifFJfA' \
--header 'Content-Type: application/json' \
--header 'api-key: {{apiKey}}' \
--data-raw '{
    "email": "banreservas-test@gmail.com",
    "password": "1234567@",
    "returnSecureToken": true
}'
```

**Response de Autenticación:**
```json
{
  "kind": "identitytoolkit#VerifyPasswordResponse",
  "localId": "user_local_id",
  "email": "banreservas-test@gmail.com",
  "displayName": "",
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6IjY5ZGE5...",
  "registered": true,
  "refreshToken": "refresh_token_here",
  "expiresIn": "3600"
}
```

### Usar el Token en las Peticiones

Una vez obtenido el `idToken`, úsalo en el header `Authorization` de tus peticiones:

**Header requerido:**
```
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6IjY5ZGE5...
```

## 📋 API Reference

### Base URL
```
http://localhost:5000/api
```

### Endpoint Principal

#### `POST /ExchangeRate/best-offer`

Obtiene la mejor oferta de cambio comparando múltiples proveedores.

**Request Body:**
```json
{
  "sourceCurrency": "USD",
  "targetCurrency": "DOP", 
  "amount": 100
}
```

**Response (200 OK):**
```json
{
  "successfull": true,
  "message": "Consulta Exitosa",
  "error": null,
  "data": {
    "bestOfferResponse": {
      "bestOffer": {
        "providerName": "MOCK_API_3",
        "convertedAmount": 5882,
        "exchangeRate": 58.82,
        "isSuccessful": true,
        "errorMessage": null
      },
      "allOffers": [
        {
          "providerName": "MOCK_API_1",
          "convertedAmount": 5550,
          "exchangeRate": 55.5,
          "isSuccessful": true,
          "errorMessage": null
        },
        {
          "providerName": "MOCK_API_2",
          "convertedAmount": 5263,
          "exchangeRate": 52.63,
          "isSuccessful": true,
          "errorMessage": null
        },
        {
          "providerName": "MOCK_API_3",
          "convertedAmount": 5882,
          "exchangeRate": 58.82,
          "isSuccessful": true,
          "errorMessage": null
        }
      ]
    }
  }
}
```

### Validaciones

| Campo | Requerido | Validación |
|-------|-----------|------------|
| `sourceCurrency` | ✅ | Exactamente 3 caracteres (ISO 4217) |
| `targetCurrency` | ✅ | Exactamente 3 caracteres (ISO 4217) |
| `amount` | ✅ | Mayor que 0 |
| Currencies | ✅ | sourceCurrency ≠ targetCurrency |

### Códigos de Respuesta

| Código | Descripción | Ejemplo |
|--------|-------------|---------|
| `200` | ✅ Operación exitosa | Mejor oferta encontrada |
| `400` | ❌ Error de validación | Divisa inválida, monto negativo |
| `401` | 🔒 No autorizado | Token inválido o ausente |
| `503` | ⚠️ Servicio no disponible | Todos los proveedores fallaron |
| `500` | 💥 Error interno | Error inesperado del servidor |

## 🧪 Testing

### Ejecutar Tests Unitarios

```bash
# Todos los tests
dotnet test

# Con cobertura de código
dotnet test --collect:"XPlat Code Coverage"

# Tests específicos
dotnet test --filter "ExchangeRateServiceTests"
```

### Estructura de Tests

```
ExchangeRateOffers.Testings/
├── Unit/
│   ├── Controllers/
│   ├── Services/
│   └── Validators/
```

### Cobertura de Tests
![alt text](image.png)
## 🔧 Ejemplos de Uso

### cURL

```bash
# 1. Primero obtener el token de autenticación
curl --location 'https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key=AIzaSyBzSwJ77sMtB_HXCLWahlv7jBWs7ifFJfA' \
--header 'Content-Type: application/json' \
--data-raw '{
    "email": "banreservas-test@gmail.com",
    "password": "1234567@",
    "returnSecureToken": true
}'

# 2. Extraer el idToken de la respuesta y usarlo en las peticiones
export TOKEN="eyJhbGciOiJSUzI1NiIsImtpZCI6IjY5ZGE5..."

# 3. Hacer petición autenticada a la API
curl -X POST http://localhost:5000/api/ExchangeRate/best-offer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "sourceCurrency": "USD",
    "targetCurrency": "DOP",
    "amount": 100
  }'

# Ejemplo con diferentes monedas
curl -X POST http://localhost:5000/api/ExchangeRate/best-offer \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d '{
    "sourceCurrency": "EUR",
    "targetCurrency": "USD", 
    "amount": 500
  }'
```