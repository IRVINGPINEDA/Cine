# Despliegue en AWS EC2 (Docker + Caddy + Hostinger)

Guia para subir la aplicacion web (`Cine.Web`) a una instancia EC2 usando Docker Compose, PostgreSQL y Caddy con HTTPS automatico.

Dominio objetivo: `caleiro.online`

## 1. Requisitos previos

- Cuenta AWS activa
- Dominio en Hostinger (ya comprado)
- Acceso SSH a la instancia EC2
- Proyecto subido a Git (o archivo zip para copiarlo)

## 2. Crear y configurar la instancia EC2

### 2.1 Crear instancia

Recomendado:
- AMI: `Ubuntu Server 22.04 LTS` o `24.04 LTS`
- Tipo: `t3.small` (minimo razonable) o `t3.micro` (si vas muy justo)
- Disco: `20 GB` o mas

### 2.2 Key pair

- Crea o selecciona una llave `.pem`
- Guardala en un lugar seguro

### 2.3 Security Group (MUY IMPORTANTE)

Abre estos puertos:
- `22` (SSH) -> idealmente solo tu IP
- `80` (HTTP) -> `0.0.0.0/0`
- `443` (HTTPS) -> `0.0.0.0/0`

### 2.4 Elastic IP (recomendado)

Para evitar que cambie la IP publica:
- Asigna una `Elastic IP` a la instancia
- Usa esa IP en Hostinger DNS

## 3. Configurar DNS en Hostinger (`caleiro.online`)

En el panel DNS de Hostinger:

### 3.1 Registro principal

- Tipo: `A`
- Host/Nombre: `@`
- Valor: `IP_PUBLICA_O_ELASTIC_IP_DE_EC2`

### 3.2 (Opcional) Subdominio www

- Tipo: `A`
- Host/Nombre: `www`
- Valor: `IP_PUBLICA_O_ELASTIC_IP_DE_EC2`

### 3.3 Esperar propagacion

- Puede tardar desde minutos hasta varias horas
- Puedes verificar con:

```bash
nslookup caleiro.online
```

## 4. Conectarte por SSH a EC2

En tu PC:

```bash
chmod 400 tu-llave.pem
ssh -i tu-llave.pem ubuntu@IP_PUBLICA_EC2
```

## 5. Instalar Docker y Docker Compose (Ubuntu)

Ejecuta en EC2:

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

Aplica el grupo Docker:

```bash
newgrp docker
```

Verifica:

```bash
docker --version
docker compose version
```

## 6. Subir el proyecto a EC2

### Opcion A: Git (recomendado)

```bash
git clone TU_REPO.git
cd Cine
```

### Opcion B: SCP (desde tu PC)

```bash
scp -i tu-llave.pem -r /ruta/a/Cine ubuntu@IP_PUBLICA_EC2:~/
ssh -i tu-llave.pem ubuntu@IP_PUBLICA_EC2
cd ~/Cine
```

## 7. Configurar variables de produccion (`.env`)

En la raiz del proyecto:

```bash
cp .env.example .env
nano .env
```

Configura (obligatorio):

```env
ASPNETCORE_ENVIRONMENT=Production

DOMAIN=caleiro.online
ACME_EMAIL=tu-correo-real@dominio.com

POSTGRES_DB=cine_db
POSTGRES_USER=postgres
POSTGRES_PASSWORD=CAMBIA_ESTA_CLAVE_POSTGRES

JWT_ISSUER=Cine.Web
JWT_AUDIENCE=Cine.Mobile
JWT_SECRET_KEY=CAMBIA_ESTA_CLAVE_LARGA_Y_ALEATORIA_MIN_32
JWT_EXPIRES_MINUTES=120

SEED_DEMO_DATA=false

BOOTSTRAP_ADMIN_EMAIL=admin@caleiro.online
BOOTSTRAP_ADMIN_PASSWORD=CAMBIA_ESTA_CLAVE_ADMIN
```

## 8. Desplegar contenedores (produccion)

En la raiz del proyecto:

```bash
docker compose up -d --build
```

Ver estado:

```bash
docker compose ps
```

## 9. Revisar logs (primer arranque)

### 9.1 Logs de la aplicacion

```bash
docker compose logs -f app
```

Debes ver:
- migraciones aplicadas
- app escuchando en `:8080`
- sin errores de DB

### 9.2 Logs de Caddy (HTTPS)

```bash
docker compose logs -f caddy
```

Debes ver:
- resolucion de dominio correcta
- emision de certificado Let's Encrypt

## 10. Verificar que quedo en linea

Pruebas:

- `http://caleiro.online` (debe redirigir o responder)
- `https://caleiro.online`
- `https://caleiro.online/api/mobile/auth/login` (si abres por navegador GET puede responder `405`, eso es normal)

Prueba por consola:

```bash
curl -I https://caleiro.online
```

## 11. Primer acceso admin (bootstrap)

Se crea automaticamente solo si definiste:
- `BOOTSTRAP_ADMIN_EMAIL`
- `BOOTSTRAP_ADMIN_PASSWORD`

Usa ese usuario para entrar al panel admin.

Importante:
- Mantener `SEED_DEMO_DATA=false` en produccion para no crear cuentas demo conocidas.

## 12. Operacion diaria (actualizar / reiniciar)

### Actualizar codigo

```bash
git pull
docker compose up -d --build
```

### Reiniciar servicios

```bash
docker compose restart
```

### Ver contenedores

```bash
docker compose ps
```

### Ver logs en tiempo real

```bash
docker compose logs -f app
docker compose logs -f caddy
docker compose logs -f db
```

### Detener

```bash
docker compose down
```

## 13. Backups (recomendado)

### 13.1 PostgreSQL (dump)

```bash
docker compose exec db pg_dump -U postgres -d cine_db > backup_cine_db.sql
```

### 13.2 Imagenes subidas (uploads)

El volumen es `uploads_data`. Puedes respaldarlo con:

```bash
docker run --rm -v cine_uploads_data:/data -v $(pwd):/backup alpine tar czf /backup/uploads_backup.tar.gz -C /data .
```

Nota:
- El nombre real del volumen puede variar segun la carpeta/proyecto (`docker volume ls` para verificar).

## 14. Troubleshooting (errores comunes)

### 14.1 No carga HTTPS / certificado no se emite

Revisa:
- DNS apunta a la IP correcta
- puertos `80` y `443` abiertos en Security Group
- Caddy logs:

```bash
docker compose logs -f caddy
```

### 14.2 Timeout al abrir la web

Revisa:
- `docker compose ps`
- `docker compose logs -f app`
- `docker compose logs -f caddy`

### 14.3 Error de conexion a DB

Revisa:
- `POSTGRES_PASSWORD` en `.env`
- que `db` este saludable:

```bash
docker compose ps
docker compose logs -f db
```

### 14.4 Cambie `.env` y no se refleja

Debes recrear:

```bash
docker compose up -d --build
```

## 15. Recomendaciones de seguridad (produccion)

- Mantener `SEED_DEMO_DATA=false`
- Cambiar todas las claves por valores fuertes
- Restringir SSH (`22`) a tu IP
- Usar Elastic IP
- Actualizar sistema periodicamente:

```bash
sudo apt update && sudo apt upgrade -y
```

- No subir `.env` al repositorio

## 16. Checklist rapido (orden correcto)

1. Crear EC2 + Security Group (22/80/443)
2. Asignar Elastic IP
3. Apuntar DNS en Hostinger (`A` -> IP EC2)
4. Instalar Docker + Compose
5. Subir proyecto
6. Configurar `.env` (`DOMAIN=caleiro.online`, claves seguras)
7. `docker compose up -d --build`
8. Revisar logs `app` + `caddy`
9. Probar `https://caleiro.online`

