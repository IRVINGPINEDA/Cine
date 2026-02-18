# Cine Stream (.NET MVC + API movil)

Aplicacion web de streaming con autenticacion por roles (`Admin`, `Cliente`) y API movil JWT restringida a usuarios `Cliente`.

## Stack
- ASP.NET Core MVC (.NET 10)
- ASP.NET Core Identity (cookies para web)
- EF Core + PostgreSQL (Npgsql)
- API movil JWT
- Docker Compose (app + postgres + caddy)

## Credenciales demo (seeding)
- `admin@demo.com` / `Admin123!` (rol `Admin`)
- `cliente@demo.com` / `Cliente123!` (rol `Cliente`)

## Funcionalidad clave
- Login web por email/contrasena con redireccion por rol.
- Panel Admin:
  - Registro/consulta de peliculas (crear, editar, activar/inactivar).
  - Registro de clientes (alta, lista, editar, activar/inactivar).
  - Registro de usuarios (alta con clave aleatoria, lista, actualizar, activar/inactivar, eliminar).
- Catalogo web: solo peliculas activas.
- API movil:
  - `POST /api/mobile/auth/login` (JWT).
  - `GET /api/mobile/movies` y `GET /api/mobile/movies/{id}` solo para rol `Cliente`.
  - Admin en login movil recibe `403` con mensaje: `Admins no pueden iniciar sesion en movil`.

## Ejecutar local sin Docker
### 1) Requisitos
- .NET SDK 10
- PostgreSQL local

### 2) Configurar conexion
Editar `Cine.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Port=5432;Database=cine_db;Username=postgres;Password=postgres"
}
```

### 3) Crear DB/migraciones (si aun no existen en tu entorno)
```bash
dotnet tool restore
dotnet ef database update --project Cine.Web/Cine.Web.csproj
```

### 4) Ejecutar
```bash
dotnet run --project Cine.Web/Cine.Web.csproj
```

La app aplica migraciones y seeding automaticamente al iniciar.

## Ejecutar con Docker Compose
```bash
docker compose up --build
```

Servicios:
- App .NET (interno: `app:8080`)
- PostgreSQL con volumen persistente `db_data`
- Caddy reverse proxy (puertos `80` y `443`)
- Volumen persistente de imagenes subidas `uploads_data`

## Cambiar dominio en Caddyfile (EC2)
Editar `Caddyfile` y reemplazar `localhost` por tu dominio:
```caddy
midominio.com {
  encode gzip
  reverse_proxy app:8080
}
```

Despues:
```bash
docker compose up -d --build
```

## Pruebas API movil (curl)
### 1) Login cliente (OK)
```bash
curl -k -X POST https://localhost/api/mobile/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"cliente@demo.com\",\"password\":\"Cliente123!\"}"
```

### 2) Login admin (debe fallar con 403)
```bash
curl -k -X POST https://localhost/api/mobile/auth/login \
  -H "Content-Type: application/json" \
  -d "{\"email\":\"admin@demo.com\",\"password\":\"Admin123!\"}"
```

### 3) Consumir peliculas activas
```bash
curl -k https://localhost/api/mobile/movies \
  -H "Authorization: Bearer TU_TOKEN"
```

### 4) Detalle de pelicula activa
```bash
curl -k https://localhost/api/mobile/movies/1 \
  -H "Authorization: Bearer TU_TOKEN"
```

