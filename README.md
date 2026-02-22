# Cine Stream (.NET MVC + API movil)

Aplicacion web de streaming con autenticacion por roles (`Admin`, `Cliente`) y API movil JWT restringida a usuarios `Cliente`.

## Stack
- ASP.NET Core MVC (.NET 10)
- ASP.NET Core Identity (cookies para web)
- EF Core + PostgreSQL (Npgsql)
- API movil JWT
- Docker Compose (app + postgres + caddy)

## Credenciales demo (solo para desarrollo)
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
  - `POST /api/mobile/auth/register` (registro de clientes).
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

## Ejecutar con Docker Compose (local)
```bash
docker compose up --build
```

Servicios:
- App .NET (interno: `app:8080`)
- PostgreSQL con volumen persistente `db_data`
- Caddy reverse proxy (puertos `80` y `443`)
- Volumen persistente de imagenes subidas `uploads_data`

## Produccion en AWS EC2 (Docker + Caddy + Hostinger)
Esta configuracion ya soporta dominio por variables de entorno (`.env`) y HTTPS automatico con Caddy.

### 1) Preparar EC2
Recomendado:
- Ubuntu 22.04/24.04 LTS
- Security Group abierto en:
  - `22` (SSH, idealmente solo tu IP)
  - `80` (HTTP)
  - `443` (HTTPS)

Opcional pero recomendado:
- Asignar Elastic IP a la instancia para que la IP publica no cambie.

### 2) Apuntar dominio en Hostinger (`caleiro.online`)
En el panel DNS de Hostinger crea/ajusta:
- Registro `A` para `@` -> `IP_PUBLICA_O_ELASTIC_IP_DE_EC2`
- (Opcional) Registro `A` para `www` -> `IP_PUBLICA_O_ELASTIC_IP_DE_EC2`

Espera propagacion DNS (puede tardar minutos u horas).

### 3) Instalar Docker y Docker Compose plugin en EC2 (Ubuntu)
```bash
sudo apt update
sudo apt install -y ca-certificates curl gnupg
sudo install -m 0755 -d /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg
sudo chmod a+r /etc/apt/keyrings/docker.gpg

echo \
  "deb [arch=$(dpkg --print-architecture) signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  $(. /etc/os-release && echo $VERSION_CODENAME) stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

sudo apt update
sudo apt install -y docker-ce docker-ce-cli containerd.io docker-buildx-plugin docker-compose-plugin
sudo usermod -aG docker $USER
```

Cierra y vuelve a abrir la sesion SSH (o ejecuta `newgrp docker`).

### 4) Subir el proyecto a EC2
Opciones comunes:
- `git clone` del repo en la instancia
- `scp` / SFTP desde tu PC

### 5) Configurar variables de produccion
En la raiz del proyecto:
```bash
cp .env.example .env
nano .env
```

Valores importantes (ajusta estos SI o SI):
- `DOMAIN=caleiro.online`
- `ACME_EMAIL=tu_correo_real@...`
- `POSTGRES_PASSWORD=...` (fuerte)
- `JWT_SECRET_KEY=...` (larga y aleatoria, minimo 32 chars)
- `SEED_DEMO_DATA=false`
- `BOOTSTRAP_ADMIN_EMAIL=admin@caleiro.online`
- `BOOTSTRAP_ADMIN_PASSWORD=...` (fuerte)

Notas:
- En produccion el seeding demo queda desactivado por defecto (`SEED_DEMO_DATA=false`).
- Se crea solo el admin bootstrap si defines `BOOTSTRAP_ADMIN_*`.

### 6) Levantar en produccion
```bash
docker compose up -d --build
```

Ver logs:
```bash
docker compose logs -f caddy
docker compose logs -f app
```

### 7) Verificar HTTPS
Una vez DNS apunte correctamente y puertos `80/443` esten abiertos, Caddy emitira certificados automaticamente.

Prueba:
- `https://caleiro.online`
- `https://caleiro.online/api/mobile/auth/login` (debe responder `405` si entras por navegador GET, lo cual confirma routing)

## Configuracion de dominio en Caddy
`Caddyfile` ya usa variables de entorno:
- `DOMAIN` (ej. `caleiro.online`)
- `ACME_EMAIL` (correo para Let's Encrypt)

No necesitas editar `Caddyfile` si configuras `.env` correctamente.

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

## Operacion basica en EC2
Actualizar despliegue:
```bash
git pull
docker compose up -d --build
```

Reiniciar servicios:
```bash
docker compose restart
```

Parar servicios:
```bash
docker compose down
```

## Riesgos/pendientes recomendados para produccion
- Cambiar/retirar cuentas demo definitivamente (`SEED_DEMO_DATA=false`).
- Hacer backup de volumen PostgreSQL (`db_data`) y uploads (`uploads_data`).
- Restringir SSH por IP en Security Group.
- Considerar CloudWatch/monitoring y logs persistentes.
