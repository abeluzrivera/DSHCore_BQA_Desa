# Runbook — Contactabilidad Inteligente

**Versión:** 1.1  
**Fecha:** 2026-06-01  
**Responsable:** Pedro Rivera (pedro.rivera@bmachala.com | 0994059032)

---

## 1. Descripción del Sistema

**Nombre:** Contactabilidad Inteligente (Smart Contactability Management System)  
**Tecnología:** ASP.NET Core 10 — Razor Pages, SQL Server, LDAP/AD  
**Propósito:** Gestión de estrategias de contacto de clientes del banco.  
**Estado:** En certificación / pre-producción.

---

## 2. Requisitos del Servidor

| Componente | Versión mínima |
|---|---|
| Windows Server | 2019 o superior |
| IIS | 10.0 |
| .NET Runtime | 10.0 o superior |

---

## 3. Entornos

| Ambiente    | Dominio AD             | Base de datos | Notas                              |
|-------------|------------------------|---------------|------------------------------------|
| Desarrollo  | bancomachala.des       | DB_ODS_DEV    | Artefacto en carpeta Drive         |
| Staging/QA  | bancomachala.pre       | DB_ODS_DEV    | Artefacto en ruta local del equipo |
| Producción  | bmachala.com           | DB_ODS        | Pendiente definir IP del servidor  |

---

## 4. Proceso de Despliegue

El proceso es completamente manual y sigue esta cadena:

```
Desarrollador
  └─ Publica artefacto (Data Smart Hub.zip) en carpeta compartida de Google Drive
        └─ Equipo de Desarrollo toma el artefacto y levanta en su entorno (validación interna)
              └─ Arma el paquete y lo deposita en ruta compartida local para Certificación
                    └─ Equipo de Certificación (QA) lo levanta, valida y aprueba
                          └─ Equipo de Producción toma ese mismo artefacto y lo despliega en IIS
```

### 4.1 Publicar un artefacto (desarrollador)

1. Desde el directorio del proyecto MVC, ejecutar:
   ```bash
   dotnet publish -c Release -o ./publish
   ```
2. El CSS se compila automáticamente con `dotnet build`. Verificar que `wwwroot/css/` esté actualizado.
3. Comprimir la carpeta `publish/` como `Data Smart Hub.zip`.
4. Subir a la ruta acordada en Drive con nombre que incluya versión y fecha, por ejemplo: `DataSmartHub_v1.2_20260601.zip`.
5. Notificar al equipo receptor con descripción del cambio.

### 4.2 Instalar en IIS (equipo receptor)

1. Descomprimir `Data Smart Hub.zip` en:
   ```
   C:\Sitios\DataSmartHub\
   ```
2. Verificar la estructura resultante:
   ```
   C:\Sitios\DataSmartHub\
   ├── appsettings.json
   ├── bin\
   ├── wwwroot\
   └── *.dll
   ```
3. Configurar `appsettings.json` con los valores cifrados del entorno (ver sección 6).
4. Confirmar que las variables de entorno del sistema estén definidas (ver sección 5).
5. Reiniciar IIS:
   ```cmd
   iisreset
   ```
6. Navegar al sitio en el puerto `2125` y confirmar que la página de login responde.

---

## 5. Configuración del Servidor

### 5.1 Variables de entorno del sistema

Las variables deben definirse a nivel **sistema**, no de usuario. Nunca deben estar en archivos de configuración.

| Variable                | Descripción                                              |
|-------------------------|----------------------------------------------------------|
| `CONFIG_MASTER_KEY`     | Clave AES-256-CBC que descifra todos los valores `ENC:` en appsettings |
| `JwtSettings__SecretKey`| Clave de firma de tokens JWT (cadena aleatoria >= 32 caracteres) |
| `ASPNETCORE_ENVIRONMENT`| `Development`, `Staging`, o `Production`                 |

**Configurar en Windows Server:**

1. Panel de control > Sistema > Configuración avanzada del sistema > Variables de entorno.
2. En "Variables del sistema", agregar cada variable con "Nueva".
3. Ejecutar `iisreset` para que IIS las reconozca.

### 5.2 Herramienta SmartHubSecretTool

Ubicación en servidor: `C:\Herramientas\SmartHubSecretTool\`

Ejecutar: `SDH.SecretTool.exe`

| Operación | Cuándo usarla |
|---|---|
| Generar clave de cifrado | Crear `CONFIG_MASTER_KEY` o `JwtSettings__SecretKey` |
| Cifrar cadena | Cifrar cadenas de conexión, contraseñas LDAP, DefaultPassword |

> La `CONFIG_MASTER_KEY` nunca debe almacenarse en archivos del sistema ni en el repositorio. Custodiarla en el gestor de contraseñas corporativo.

### 5.3 Application Pool en IIS

| Campo | Valor |
|---|---|
| Nombre | `SmartHubPool` |
| Versión de .NET CLR | Sin código administrado |
| Modo de canalización | Integrado |
| Identidad | `ApplicationPoolIdentity` (predeterminada) |

**Permisos sobre la carpeta del sitio:**

Agregar `IIS AppPool\SmartHubPool` con permisos de "Lectura y ejecución" sobre `C:\Sitios\DataSmartHub\`.

### 5.4 Sitio web en IIS

| Campo | Valor |
|---|---|
| Nombre del sitio | `Data Smart Hub` |
| Grupo de aplicaciones | `SmartHubPool` |
| Ruta física | `C:\Sitios\DataSmartHub` |
| Puerto | `2125` |

---

## 6. Configuración de appsettings.json

Archivo ubicado en `C:\Sitios\DataSmartHub\appsettings.json`.

### 6.1 Cadena de conexión

Construir en texto plano y cifrar con SmartHubSecretTool antes de pegar:

```
Server=<IP_SERVIDOR_SQL>;Database=<NOMBRE_BD>;User ID=usr_smarthub;Password=<CONTRASENA>;Persist Security Info=False;TrustServerCertificate=True
```

```json
"ConnectionStrings": {
  "DefaultConnection": "ENC:xxxxxxxxxxxxxxxxxxxxxxxx"
}
```

Bases de datos por entorno: `DB_ODS` (producción) / `DB_ODS_DEV` (staging y desarrollo).

### 6.2 Proveedor de autenticación

```json
"AuthSettings": {
  "Provider": "DB"
}
```

### 6.3 Contraseña inicial de usuarios

```json
"SeedSettings": {
  "DefaultPassword": "ENC:xxxxxxxxxxxxxxxxxxxxxxxx"
}
```

### 6.4 LDAP

```json
"LdapSettings": {
  "Host": "10.107.41.155",
  "Port": 389,
  "UseSSL": false,
  "BaseDn": "DC=dominio,DC=local",
  "BindDn": "usuario@dominio.local",
  "BindPassword": "ENC:xxxxxxxxxxxxxxxxxxxxxxxx",
  "UserSearchBase": "DC=dominio,DC=local",
  "UserSearchFilter": "(&(objectClass=user)(sAMAccountName={0}))"
}
```

> `BindPassword` debe cifrarse con SmartHubSecretTool. Los campos `Host`, `BaseDn`, `BindDn` y `BindPassword` cambian por entorno.

---

## 7. Base de Datos

La aplicación usa autenticación SQL Server con usuario local. No requiere usuario de dominio.

```sql
-- Crear login
CREATE LOGIN usr_smarthub WITH PASSWORD = '<contrasena_segura>';

-- Asociar a la base de datos
USE DB_ODS;
CREATE USER usr_smarthub FOR LOGIN usr_smarthub;

-- Permisos mínimos
ALTER ROLE db_datareader ADD MEMBER usr_smarthub;
ALTER ROLE db_datawriter ADD MEMBER usr_smarthub;
```

Para staging, repetir sobre `DB_ODS_DEV`.

---

## 8. Dependencias

| Dependencia | Tipo | Criticidad | Contacto si falla |
|---|---|---|---|
| SQL Server | Base de datos | Alta | Administrador de plataforma |
| LDAP / AD (host: 10.107.41.155) | Autenticación | Alta | Administrador de plataforma |
| Servicios externos (Equifax, etc.) | API | Media | Administrador de plataforma |

> Si cualquier dependencia falla, contactar al administrador de la plataforma antes de escalar.

---

## 9. Operación Diaria

### Verificar que el sistema está activo

1. Navegar al sitio en el puerto `2125`.
2. Confirmar que la página de login carga sin errores.
3. Hacer login con usuario de prueba de cada rol (AGENTE, SUPERVISOR, ADMIN).
4. Verificar que los catálogos cargan sin errores en el dashboard.

### Reinicio del servicio

Preferir recycle del pool antes que iisreset completo:

```
IIS Manager > Grupos de aplicaciones > SmartHubPool > Reciclar
```

Si el recycle no resuelve:

```cmd
iisreset
```

> `iisreset` afecta todos los sitios del servidor. Usarlo solo si el recycle del pool falla.

### Logs de aplicación

- **Ruta:** `C:\Sitios\DataSmartHub\Logs\`
- **Formato:** un archivo por día.
- **Retención:** 30 días. Archivos anteriores se eliminan automáticamente.

---

## 10. Respuesta a Incidentes

### Niveles de severidad propuestos

| Nivel | Condición | Tiempo de respuesta |
|---|---|---|
| P1 | Sistema inaccesible o login fallando al 100% | 1 hora |
| P2 | Funcionalidad crítica degradada | 4 horas |
| P3 | Error funcional no bloqueante | 1 día hábil |

> SLA formal pendiente de aprobación.

### Flujo de respuesta P1

```
1. Verificar que IIS está corriendo → IIS Manager o services.msc
2. Revisar logs del día en C:\Sitios\DataSmartHub\Logs\
3. Verificar variables de entorno CONFIG_MASTER_KEY y JwtSettings__SecretKey
4. Verificar conectividad a SQL Server (ver abajo)
5. Verificar conectividad a LDAP (ver abajo)
6. Si no se resuelve → recycle de SmartHubPool
7. Si persiste → iisreset
8. Si persiste → escalar al administrador de plataforma
```

### Verificar conectividad SQL Server

```cmd
sqlcmd -S <IP_SERVIDOR_SQL> -Q "SELECT 1"
```

### Verificar conectividad LDAP

```cmd
nltest /dsgetdc:bmachala.com
```

Para staging:

```cmd
nltest /dsgetdc:bancomachala.pre
```

---

## 11. Rollback

1. Antes de cada deploy, conservar una copia del artefacto anterior con nombre versionado.
2. Detener el sitio `Data Smart Hub` en IIS Manager.
3. Reemplazar el contenido de `C:\Sitios\DataSmartHub\` con el artefacto anterior.
4. Iniciar el sitio.
5. Verificar login y dashboard.

> Convención de nombrado recomendada: `DataSmartHub_v1.2_20260601.zip`

---

## 12. Checklist de Verificación Post-Deploy

- [ ] Sitio responde en el puerto `2125`
- [ ] Variables de entorno `CONFIG_MASTER_KEY` y `JwtSettings__SecretKey` están definidas en el sistema
- [ ] IIS fue reiniciado tras configurar las variables de entorno
- [ ] Login con usuario local de base de datos funciona
- [ ] Los catálogos cargan sin errores en el dashboard
- [ ] Usuario `usr_smarthub` tiene permisos `db_datareader` y `db_datawriter`
- [ ] Cadena de conexión y `DefaultPassword` están cifrados con formato `ENC:...`

---

## 13. Monitoreo

**Estado actual:** sin monitoreo activo.  
**Estado planificado:** implementar alertas y dashboard.

Verificación manual hasta que exista monitoreo:

- Revisar `C:\Sitios\DataSmartHub\Logs\` diariamente en staging y producción.
- Ante cualquier reporte de error, revisar el log del día.

---

## 14. Contactos

| Rol | Nombre | Email | Teléfono |
|---|---|---|---|
| Contacto técnico principal | Pedro Rivera | pedro.rivera@bmachala.com | - |
| Administrador de plataforma | Aldo Saldana | aldo.saldana@bmachala.com | - |
| Equipo de producción | Mesa de Ayuda | — | — |

---

## 15. Pendientes

- [ ] Definir IP y ruta del servidor de producción.
- [ ] Registrar IP/nombre del servidor SQL Server de producción.
- [ ] Completar tabla de contactos (admin de plataforma, equipo de producción).
- [ ] Aprobar SLA de disponibilidad.
- [ ] Implementar monitoreo activo.
