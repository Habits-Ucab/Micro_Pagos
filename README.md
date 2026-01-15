# Micro_Pagos (Pagos, Promociones y Facturación)

Microservicio de pagos que integra **Stripe** para checkout, maneja **promociones**, genera **facturas PDF** y ejecuta tareas programadas.

## Stack

- .NET 8 (ASP.NET Core)
- MongoDB (persistencia)
- Stripe (PaymentIntent / checkout)
- RabbitMQ (MassTransit)
- Hangfire (jobs) con `MemoryStorage` para desarrollo
- QuestPDF (generación de PDF)

## Requisitos

- .NET 8 SDK
- MongoDB (local: `mongodb://localhost:27017`)
- RabbitMQ (local: `rabbitmq://localhost`, usuario `guest`, contraseña `guest`)
- Clave secreta de Stripe (recomendado: keys de test)

## Ejecutar (local)

```cmd
dotnet restore
dotnet run --project Micro_Pagos/Pagos_API/Pagos_API.csproj
```

- URL (HTTP): `http://localhost:5079`
- Swagger (Development): `http://localhost:5079/swagger`
- Hangfire Dashboard (Development): `http://localhost:5079/hangfire`

## Configuración

Archivo: `Micro_Pagos/Pagos_API/appsettings.json`

- `MongoDb`: `CadenaConexion`, `NombreBaseDeDatos`, `Colecciones`
- `Stripe`: `ClaveSecreta`
- `RabbitMQ`: `Host`, `Usuario`, `Contrasena`

Importante:

- Evita commitear secretos (Stripe, RabbitMQ). Para producción usa variables de entorno/secret manager.

## Endpoints principales (referencia)

- `POST /api/pagos/checkout` — Checkout y creación del pago
- `GET /api/pagos/{idPago}` — Consulta estado
- `POST /api/pagos/{idPago}/sincronizar` — Sincroniza con Stripe
- `GET /api/metodos-pago/...` — Métodos de pago
- `GET|POST /api/promociones...` — Promociones
- `GET /api/facturas/...` — Descarga de facturas

## Jobs

En Development se habilita Hangfire Dashboard y se registran jobs recurrentes (ver `Pagos_API/Program.cs`).

## Pruebas

```cmd
dotnet test Micro_Pagos/Pagos_Test/Pagos_Test.csproj
```

## Integración con Gateway

El Gateway proxyea Pagos bajo:

- `/pagos/api/...`