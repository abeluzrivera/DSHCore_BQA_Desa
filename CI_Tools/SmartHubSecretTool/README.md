# SmartHub Secret Tool - Manual de Usuario

## Descripción

**SmartHub Secret Tool** es una herramienta de línea de comandos para cifrar valores sensibles (contraseñas, claves API, tokens, etc.) que se almacenan en archivos de configuración (`appsettings.json`). 

Utiliza cifrado AES-256-CBC con una clave maestra (`CONFIG_MASTER_KEY`) que se gestiona a través de variables de entorno del sistema.

---

## Requisitos Previos

- **.NET 10** (o superior) instalado en el sistema
- **Variable de entorno `CONFIG_MASTER_KEY`** configurada en Windows
  - Esta clave es la que descifra los valores en tiempo de ejecución de la aplicación

---

## Instalación

1. **Descargar el archivo**: Extrae el archivo `.zip` en la carpeta deseada
   ```
   SmartHubSecretTool/
   ├── SDH.SecretTool.dll
   ├── SDH.SecretTool.exe
   └── ... (otros archivos)
   ```

2. **Verificar .NET**: Abre PowerShell o CMD y ejecuta:
   ```cmd
   dotnet --version
   ```

---

## Uso

### Ejecutar la herramienta

**En Windows:**
```cmd
SDH.SecretTool.exe
```

**Con dotnet (desde la carpeta del proyecto):**
```cmd
dotnet SDH.SecretTool.dll
```

---

## Menú Principal

Al ejecutar la herramienta, verás:

```
====================================
 SmartHub Secret Tool
====================================
 CONFIG_MASTER_KEY: P*** [OK]
====================================
1. Cifrar cadena
2. Generar clave
0. Salir
```

### Opción 1: Cifrar Cadena

**Flujo:**
1. Selecciona la opción **1**
2. Ingresa el valor a cifrar (la entrada está enmascarada con `*` por seguridad)
3. La herramienta genera el valor cifrado con prefijo `ENC:`
4. Copia el resultado a tu `appsettings.json`

**Ejemplo:**

```
Seleccione una opcion: 1

Ingrese el valor a cifrar: Admin123!

Valor cifrado:
ENC:tQzYFt7xbs2Amlcb3cfXhgNTlhHbWxTt/NmtVkleJU8=
```

**Uso en appsettings.json:**
```json
{
  "SeedSettings": {
    "DefaultPassword": "ENC:tQzYFt7xbs2Amlcb3cfXhgNTlhHbWxTt/NmtVkleJU8="
  }
}
```

### Opción 2: Generar Clave

**Propósito:** Crear una clave segura para usar como `CONFIG_MASTER_KEY`

**Flujo:**
1. Selecciona la opción **2**
2. Ingresa la longitud deseada (default: 32 caracteres)
3. Elige si incluir símbolos (default: sí)
4. La herramienta genera una clave aleatoria

**Ejemplo:**

```
Seleccione una opcion: 2

Longitud de la clave [32]: 32
Incluir simbolos? [S/n]: s

Clave generada:
aB3$mK9pL@2xN5qR7vW8yZ0dF4gH6jT1
```

**Uso:** Configura esta clave como variable de entorno `CONFIG_MASTER_KEY`:

```powershell
# PowerShell (administrador)
[Environment]::SetEnvironmentVariable("CONFIG_MASTER_KEY", "aB3$mK9pL@2xN5qR7vW8yZ0dF4gH6jT1", "Machine")
```

```cmd
# CMD (administrador)
setx CONFIG_MASTER_KEY "aB3$mK9pL@2xN5qR7vW8yZ0dF4gH6jT1"
```

---

## Flujo Completo: Proteger una Contraseña

### 1️⃣ Generar la clave maestra (si no existe)

```cmd
SDH.SecretTool.exe
Seleccione una opcion: 2
Longitud de la clave [32]: ← Presiona ENTER
Incluir simbolos? [S/n]: ← Presiona ENTER
```

Copia la clave generada y configúrala en tu sistema:

```powershell
[Environment]::SetEnvironmentVariable("CONFIG_MASTER_KEY", "tu_clave_aqui", "Machine")
```

### 2️⃣ Cifrar la contraseña

```cmd
SDH.SecretTool.exe
Seleccione una opcion: 1
Ingrese el valor a cifrar: Admin123!
```

Resultado:
```
ENC:tQzYFt7xbs2Amlcb3cfXhgNTlhHbWxTt/NmtVkleJU8=
```

### 3️⃣ Agregar a appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=..."
  },
  "AuthSettings": {
    "Provider": "DB"
  },
  "SeedSettings": {
    "DefaultPassword": "ENC:tQzYFt7xbs2Amlcb3cfXhgNTlhHbWxTt/NmtVkleJU8="
  }
}
```

### 4️⃣ Usar en código

En `UsuarioSeeder.cs`:

```csharp
string encryptedPassword = configuration["SeedSettings:DefaultPassword"];
string plainPassword = ConfigCrypto.DecryptFromEnvironment(encryptedPassword);
string hash = BCrypt.Net.BCrypt.HashPassword(plainPassword);
```

---

## Validación de Configuración

Al abrir la herramienta, verás en el menú si la variable `CONFIG_MASTER_KEY` está correcta:

✅ **Correcta:**
```
CONFIG_MASTER_KEY: P*** [OK]
```

❌ **No configurada o incorrecta:**
```
CONFIG_MASTER_KEY: [NO CONFIGURADA]
```

Si ves `[NO CONFIGURADA]`:
1. Verifica que la variable de entorno está definida
2. Reinicia PowerShell/CMD después de configurarla
3. Asegúrate de ejecutar como administrador si es necesario

---

## Seguridad

### ✅ Buenas prácticas

- **NO commits valores sin cifrar** al repositorio
- **Usa la herramienta** para cifrar antes de guardar en `appsettings.json`
- **Protege la clave maestra** (`CONFIG_MASTER_KEY`):
  - Configúrala como variable de entorno de máquina en producción
  - No la compartas por email o chat
  - Usa gestores de secretos (Azure Key Vault, HashiCorp Vault) en prod

### ⚠️ Qué NO hacer

- ❌ Poner la clave maestra en `appsettings.json`
- ❌ Usar `appsettings.Development.json` para secretos
- ❌ Commitear valores sin cifrar
- ❌ Compartir la clave maestra por canales no seguros

---

## Solución de Problemas

### Error: "CONFIG_MASTER_KEY: [NO CONFIGURADA]"

**Causa:** La variable de entorno no está definida

**Solución:**

1. Abre **PowerShell como administrador**
2. Ejecuta:
   ```powershell
   [Environment]::SetEnvironmentVariable("CONFIG_MASTER_KEY", "tu_clave_aqui", "Machine")
   ```
3. **Cierra completamente PowerShell** (todas las ventanas)
4. **Abre una nueva ventana de PowerShell** y ejecuta la herramienta nuevamente

---

### Error: "No se pudo cifrar el valor"

**Causa:** Problema con la clave maestra

**Solución:**
1. Verifica que `CONFIG_MASTER_KEY` está correctamente configurada
2. Regenera una clave nueva con la opción **2**
3. Actualiza la variable de entorno

---

### Valor cifrado diferente cada vez

**Esto es normal.** AES-256-CBC con un IV aleatorio genera resultados diferentes. Los valores son válidos mientras `CONFIG_MASTER_KEY` sea el mismo.

---

## Ejemplos de Uso Típico

### Cifrar contraseña de base de datos

```
Ingrese el valor a cifrar: Server=prod.example.com;Database=SmartHub;User Id=admin;Password=MyPassword123!

Valor cifrado:
ENC:aBc123DeF4gHiJkLmNoPqRsTuVwXyZaBcDeFgHiJkLmNoPqRsT
```

### Cifrar token API

```
Ingrese el valor a cifrar: sk_live_51234567890abcdefghijklmnop

Valor cifrado:
ENC:9xYzAbCdEfGhIjKlMnOpQrStUvWxYzAbCdEfGhIjKlMnOpQr
```

---

## Contacto y Soporte

Para problemas o sugerencias, contacta al equipo de desarrollo.

---

## Versión

- **Versión:** 1.0
- **Última actualización:** Mayo 2026
