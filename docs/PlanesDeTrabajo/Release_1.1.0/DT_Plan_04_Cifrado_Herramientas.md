# Plan de Trabajo — Deuda Técnica Items 10 y 11
## Librería de Cifrado Centralizada y Renombrado de SmartHubSecretTool
### Release 1.1.0 — Paquete E

**Fecha:** 05 de junio de 2026  
**Estado:** Listo para implementar  
**Responsable:** Por asignar  
**Estimación:** 3-5 días de desarrollo + configuración inicial del feed en Azure Artifacts

---

## Decisiones tomadas

| Decisión | Valor |
|----------|-------|
| Nombre de la librería de cifrado | `Bm.Security.Lib.SecretTool` (package ID NuGet: `bm.security.lib.secrettool`) |
| Nombre de la herramienta de secrets | `Bm.Security.App.SecretTool` (package ID NuGet: `bm.security.app.secrettool`) |
| Mecanismo de distribución | Azure Artifacts (NuGet feed privado del banco en Azure DevOps) |
| Scope de la librería | Solo cifrado/descifrado AES-256-CBC para esta primera iteración |

---

## Objetivo

Extraer la lógica de cifrado AES-256-CBC a la librería `Bm.Security.Lib.SecretTool`, publicarla en el feed NuGet privado de Azure DevOps, y convertir `SDH.SecretTool` en la herramienta institucional `Bm.Security.App.SecretTool`. Ambos proyectos deben quedar consumibles por cualquier proyecto del banco sin dependencia del repositorio SDH.

---

## DT-10 — Librería `Bm.Security.Lib.SecretTool`

### Investigación previa

Antes de implementar, mapear dónde existe actualmente la lógica de cifrado en el repositorio:

```powershell
# Implementaciones AES en todo el repositorio
Select-String -Path "D:\sources\DSHCore_BQA_Desa\**\*.cs" `
  -Pattern "Aes\.|AesCbc|AesManaged|RijndaelManaged|CryptoStream|ICryptoTransform" -Recurse |
  Select-Object Path, LineNumber, Line

# Referencias a la clave maestra
Select-String -Path "D:\sources\DSHCore_BQA_Desa\**\*.cs" `
  -Pattern "CONFIG_MASTER_KEY|masterKey|MasterKey" -Recurse |
  Select-Object Path, LineNumber, Line

# Archivos del SecretTool actual
Get-ChildItem -Recurse "D:\sources\DSHCore_BQA_Desa\CI_Tools\SmartHubSecretTool\SDH.SecretTool" -Filter "*.cs" |
  Select-Object FullName
```

El resultado determina si la lógica de cifrado está solo en el SecretTool o también duplicada en `SDH.infrastructure` o el proyecto MVC.

### Estructura del nuevo proyecto

```
CI_Tools/
  Bm.Security.Lib.SecretTool/
    Bm.Security.Lib.SecretTool.csproj
    Abstractions/
      ISymmetricCryptoService.cs
    Services/
      AesCbcCryptoService.cs
    README.md
```

### Interfaz pública

```csharp
namespace Bm.Security.Lib.SecretTool
{
    public interface ISymmetricCryptoService
    {
        string Encrypt(string plainText, string key);
        string Decrypt(string cipherText, string key);
    }

    public class AesCbcCryptoService : ISymmetricCryptoService
    {
        public string Encrypt(string plainText, string key) { ... }
        public string Decrypt(string cipherText, string key) { ... }
    }
}
```

La implementación es la misma lógica AES-256-CBC que ya existe en `SDH.SecretTool`; solo se reubica. No agregar funcionalidades nuevas.

### Fase 1 — Creación del proyecto

```bash
# Desde la raíz del repositorio
dotnet new classlib -n Bm.Security.Lib.SecretTool \
  -o CI_Tools/Bm.Security.Lib.SecretTool \
  --framework net10.0
```

Editar el `.csproj` generado para incluir los metadatos de publicación:

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
    <PackageId>bm.security.lib.secrettool</PackageId>
    <Version>1.0.0</Version>
    <Authors>Equipo SDH — Banco Bolivariano</Authors>
    <Description>Librería de cifrado simétrico AES-256-CBC para proyectos del banco.</Description>
    <RepositoryUrl>https://dev.azure.com/[organizacion]/[proyecto]/_git/DSHCore</RepositoryUrl>
    <IsPackable>true</IsPackable>
  </PropertyGroup>
</Project>
```

No agregar dependencias de NuGet. La librería solo usa `System.Security.Cryptography`, disponible en el SDK base de .NET.

### Fase 2 — Extracción de la lógica de cifrado

1. Leer el código de cifrado actual en `SDH.SecretTool`.
2. Copiar la implementación AES-256-CBC a `AesCbcCryptoService.cs` bajo el namespace `Bm.Security.Lib.SecretTool`.
3. Crear `ISymmetricCryptoService.cs` con la interfaz.
4. Verificar que la clase implementa la interfaz.
5. Hacer una prueba manual de round-trip antes de continuar:
   ```csharp
   var svc = new AesCbcCryptoService();
   var key = "clave-de-prueba-32-caracteres!!!";
   var cipher = svc.Encrypt("valor-de-prueba", key);
   var plain = svc.Decrypt(cipher, key);
   // plain debe ser igual a "valor-de-prueba"
   ```
6. Cifrar un valor que ya esté en `appsettings.json` de desarrollo y verificar que la librería produce el mismo resultado que el SecretTool original. Esta prueba de compatibilidad es crítica antes de eliminar el código antiguo.

### Fase 3 — Integración en `SDH.SecretTool` (como referencia de proyecto, previo a la publicación)

Mientras el feed NuGet no está configurado, la integración se hace como `ProjectReference`:

```bash
cd CI_Tools/SmartHubSecretTool/SDH.SecretTool
dotnet add reference ../../Bm.Security.Lib.SecretTool/Bm.Security.Lib.SecretTool.csproj
```

Actualizar el código del SecretTool para usar `Bm.Security.Lib.SecretTool.AesCbcCryptoService` en lugar de su propia implementación. Eliminar el código de cifrado local. Compilar y verificar que funciona.

### Fase 4 — Integración en la solución principal

```bash
cd CI_API/SDH.infrastructure
dotnet add reference ../../../CI_Tools/Bm.Security.Lib.SecretTool/Bm.Security.Lib.SecretTool.csproj
```

Registrar el servicio en `Program.cs`:

```csharp
using Bm.Security.Lib.SecretTool;

// En la sección de servicios:
builder.Services.AddSingleton<ISymmetricCryptoService, AesCbcCryptoService>();
```

Reemplazar cualquier implementación local de cifrado en `SDH.infrastructure` o en el proyecto MVC por el uso de `ISymmetricCryptoService`. Eliminar código de cifrado duplicado.

### Fase 5 — Configuración del feed Azure Artifacts y publicación

#### Prerequisito: tener acceso al portal de Azure DevOps del banco

1. En Azure DevOps, ir a **Artifacts** y crear un feed (si no existe uno):
   - Nombre sugerido: `bm-packages` o el nombre que use el banco para feeds internos.
   - Visibilidad: solo miembros del proyecto (privado).

2. Obtener la URL del feed. Tiene la forma:
   ```
   https://pkgs.dev.azure.com/[organizacion]/_packaging/[nombre-feed]/nuget/v3/index.json
   ```

3. Crear o actualizar el archivo `nuget.config` en la raíz del repositorio para incluir el feed:
   ```xml
   <?xml version="1.0" encoding="utf-8"?>
   <configuration>
     <packageSources>
       <add key="nuget.org" value="https://api.nuget.org/v3/index.json" />
       <add key="bm-packages" value="https://pkgs.dev.azure.com/[organizacion]/_packaging/[nombre-feed]/nuget/v3/index.json" />
     </packageSources>
   </configuration>
   ```

4. Autenticar con el feed. En un equipo de desarrolladores locales, usar el Credential Provider de Azure Artifacts:
   ```powershell
   # Instalar el credential provider (una vez por máquina)
   iex "& { $(irm https://aka.ms/install-artifacts-credprovider.ps1) }"

   # Autenticar
   dotnet restore --interactive
   ```

5. Empaquetar y publicar la librería:
   ```bash
   # Desde la carpeta de la librería
   cd CI_Tools/Bm.Security.Lib.SecretTool

   # Generar el paquete
   dotnet pack -c Release -o ./nupkg

   # Publicar al feed de Azure Artifacts
   dotnet nuget push ./nupkg/bm.security.lib.secrettool.1.0.0.nupkg \
     --source "bm-packages" \
     --api-key az
   ```

6. Verificar que el paquete aparece en el feed de Azure Artifacts en el portal.

#### Configurar el pipeline CI para publicación automática

En el pipeline de CI/CD (si existe), agregar un step de publicación que se ejecute al hacer merge a main:

```yaml
# Azure DevOps pipeline
- task: DotNetCoreCLI@2
  displayName: 'Pack Bm.Security.Lib.SecretTool'
  inputs:
    command: 'pack'
    packagesToPack: 'CI_Tools/Bm.Security.Lib.SecretTool/Bm.Security.Lib.SecretTool.csproj'
    versioningScheme: 'off'

- task: NuGetCommand@2
  displayName: 'Push to Azure Artifacts'
  inputs:
    command: 'push'
    packagesToPush: '$(Build.ArtifactStagingDirectory)/*.nupkg'
    nuGetFeedType: 'internal'
    publishVstsFeed: '[nombre-feed]'
```

### Fase 6 — Migrar de ProjectReference a PackageReference

Una vez publicado el paquete en Azure Artifacts:

1. En `SDH.infrastructure.csproj`, reemplazar la `ProjectReference` por:
   ```xml
   <PackageReference Include="bm.security.lib.secrettool" Version="1.0.0" />
   ```
2. En `Bm.Security.App.SecretTool.csproj` (DT-11), ídem.
3. Ejecutar `dotnet restore` para confirmar que el paquete se resuelve desde el feed.
4. Compilar y verificar que todo funciona.

### Consideraciones de seguridad

- La librería no almacena ni hardcodea ninguna clave. La clave siempre se pasa como parámetro en tiempo de ejecución.
- No incluir en los tests ninguna clave real del ambiente de producción.
- El README de la librería debe aclarar explícitamente que la gestión de la clave maestra (`CONFIG_MASTER_KEY`) es responsabilidad del consumidor.
- El paquete NuGet no debe contener archivos de configuración ni secrets.

### Criterios de aceptación — DT-10

- El proyecto `Bm.Security.Lib.SecretTool` existe, compila, y no tiene dependencias NuGet externas.
- `SDH.SecretTool` (o su sucesor `Bm.Security.App.SecretTool`) usa la librería y no contiene código propio de cifrado.
- `SDH.infrastructure` usa la librería y no contiene código propio de cifrado.
- Una prueba de round-trip confirma que los valores cifrados en `appsettings.json` de producción se descifran correctamente con la nueva librería.
- El paquete `bm.security.lib.secrettool` v1.0.0 está publicado en el feed de Azure Artifacts.
- El `nuget.config` del repositorio referencia el feed del banco.

---

## DT-11 — `Bm.Security.App.SecretTool`

### Contexto

La herramienta `SDH.SecretTool` se renombra a `Bm.Security.App.SecretTool`. Al mismo tiempo, se integra con la librería `Bm.Security.Lib.SecretTool` del DT-10 para eliminar la lógica de cifrado duplicada.

### Estructura post-renombrado

```
CI_Tools/
  Bm.Security.App.SecretTool/
    Bm.Security.App.SecretTool.csproj
    Program.cs
    Commands/
      EncryptCommand.cs
      DecryptCommand.cs
    README.md
```

La carpeta `SmartHubSecretTool` desaparece. La carpeta `Bm.Security.Lib.SecretTool` (DT-10) es hermana de `Bm.Security.App.SecretTool` bajo `CI_Tools/`.

### Archivos afectados

| Archivo / Carpeta actual | Estado tras el cambio |
|--------------------------|----------------------|
| `CI_Tools/SmartHubSecretTool/SDH.SecretTool/` | Se mueve y renombra a `CI_Tools/Bm.Security.App.SecretTool/` |
| `CI_Tools/SmartHubSecretTool/SDH.SecretTool/SDH.SecretTool.csproj` | Se renombra a `Bm.Security.App.SecretTool.csproj` |
| Todos los `.cs` del proyecto | Namespaces de `SDH.SecretTool.*` → `Bm.Security.App.SecretTool` |
| `CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv.sln` | Actualizar referencia al proyecto |
| `CLAUDE.md` | Actualizar sección Secret Management |
| `CI_Tools/SmartHubSecretTool/README.md` | Mover a `CI_Tools/Bm.Security.App.SecretTool/README.md` y actualizar |
| `CI_Tools/SmartHubSecretTool/Documentacion_DataSmartSecretTool.pdf` | Evaluar si mover o dejar como archivo histórico |

### Pasos de implementación

1. Completar DT-10 Fase 1 y Fase 2 antes de iniciar este ítem.

2. Crear la nueva carpeta y mover los archivos:
   ```powershell
   # Mover el proyecto al nuevo nombre
   Move-Item "D:\sources\DSHCore_BQA_Desa\CI_Tools\SmartHubSecretTool\SDH.SecretTool" `
             "D:\sources\DSHCore_BQA_Desa\CI_Tools\Bm.Security.App.SecretTool"

   # Renombrar el .csproj
   Rename-Item "D:\sources\DSHCore_BQA_Desa\CI_Tools\Bm.Security.App.SecretTool\SDH.SecretTool.csproj" `
               "Bm.Security.App.SecretTool.csproj"
   ```

3. Actualizar los namespaces en todos los archivos `.cs` del proyecto:
   ```powershell
   Get-ChildItem -Recurse -Filter "*.cs" `
     "D:\sources\DSHCore_BQA_Desa\CI_Tools\Bm.Security.App.SecretTool" |
     ForEach-Object {
       (Get-Content $_.FullName) `
         -replace 'namespace SDH\.SecretTool', 'namespace Bm.Security.App.SecretTool' `
         -replace 'using SDH\.SecretTool', 'using Bm.Security.App.SecretTool' |
       Set-Content $_.FullName
     }
   ```

4. Editar el `.csproj` para actualizar el `AssemblyName`, `RootNamespace` y agregar la referencia a la librería:
   ```xml
   <Project Sdk="Microsoft.NET.Sdk">
     <PropertyGroup>
       <OutputType>Exe</OutputType>
       <TargetFramework>net10.0</TargetFramework>
       <AssemblyName>Bm.Security.App.SecretTool</AssemblyName>
       <RootNamespace>Bm.Security.App.SecretTool</RootNamespace>
       <PackageId>bm.security.app.secrettool</PackageId>
       <Version>1.0.0</Version>
       <Authors>Equipo SDH — Banco Bolivariano</Authors>
       <Description>Herramienta CLI para gestión de secrets cifrados en appsettings.json.</Description>
     </PropertyGroup>
     <ItemGroup>
       <ProjectReference Include="..\Bm.Security.Lib.SecretTool\Bm.Security.Lib.SecretTool.csproj" />
     </ItemGroup>
   </Project>
   ```
   (Después de publicar la librería en Azure Artifacts, reemplazar la `ProjectReference` por `PackageReference`.)

5. Actualizar el `.sln`:
   - Abrir `contactabilidad_inteligente.mcv.sln` en un editor de texto.
   - Localizar la entrada del proyecto `SDH.SecretTool` y actualizar nombre y ruta:
     ```
     Project("{...}") = "Bm.Security.App.SecretTool",
       "..\..\..\..\CI_Tools\Bm.Security.App.SecretTool\Bm.Security.App.SecretTool.csproj",
       "{MISMO-GUID}"
     ```

6. Actualizar `CLAUDE.md`, sección **Secret Management**:
   - Reemplazar `CI_Tools/SmartHubSecretTool/` por `CI_Tools/Bm.Security.App.SecretTool/`.
   - Actualizar el nombre de la herramienta en los ejemplos de uso.

7. Compilar y verificar:
   ```bash
   dotnet build CI_Tools/Bm.Security.App.SecretTool/

   dotnet publish CI_Tools/Bm.Security.App.SecretTool/ \
     -c Release -r win-x64 --self-contained true \
     -o CI_Tools/Bm.Security.App.SecretTool/publish
   ```

8. Probar que la herramienta cifra y descifra correctamente:
   ```powershell
   $exe = "D:\sources\DSHCore_BQA_Desa\CI_Tools\Bm.Security.App.SecretTool\publish\Bm.Security.App.SecretTool.exe"
   # Adaptar los argumentos a la CLI actual de la herramienta (leer README original)
   & $exe encrypt "valor-de-prueba"
   ```

9. Verificar compatibilidad con datos de producción: descifrar un valor del `appsettings.json` real con la herramienta renombrada y confirmar que el resultado es correcto.

### Separación del repositorio (alcance futuro)

En este release, la herramienta permanece en el repositorio SDH pero con el nuevo nombre y desacoplada de la lógica de cifrado. La extracción a un repositorio propio es un paso independiente que se planifica en Release 2.0.0, una vez que el feed de Azure Artifacts esté operativo y el equipo de banco confirme la estrategia de repositorios.

### Criterios de aceptación — DT-11

- El proyecto se llama `Bm.Security.App.SecretTool` en todo el repositorio (carpeta, .csproj, namespaces, .sln).
- No existe ninguna referencia a `SDH.SecretTool` en archivos activos del repositorio.
- La herramienta compila, se publica como ejecutable `win-x64`, y puede cifrar/descifrar valores.
- Los valores ya cifrados en `appsettings.json` de producción se descifran correctamente con la herramienta renombrada.
- `CLAUDE.md` refleja el nuevo nombre y ruta de la herramienta.
- El README de la herramienta está actualizado con el nuevo nombre y los comandos de uso.

---

## Orden de ejecución

```
DT-10 Fase 1 — Crear proyecto Bm.Security.Lib.SecretTool
DT-10 Fase 2 — Extraer lógica AES + prueba de round-trip
DT-10 Fase 3 — Integrar en SDH.infrastructure (ProjectReference)
DT-11        — Renombrar SDH.SecretTool → Bm.Security.App.SecretTool
             — Integrar con Bm.Security.Lib.SecretTool
DT-10 Fase 5 — Configurar Azure Artifacts + publicar paquete
DT-10 Fase 6 — Migrar de ProjectReference a PackageReference en ambos consumidores
```

DT-10 Fase 5 y 6 pueden ejecutarse en paralelo con otros items del Paquete E porque solo cambia el mecanismo de referencia, no la funcionalidad.

---

## Riesgos

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| La lógica de cifrado en la solución principal usa parámetros distintos (IV, padding) que los del SecretTool | Media | Alto | Verificar round-trip entre SecretTool y solución principal con un valor real antes de eliminar cualquier implementación |
| Los valores ya cifrados en appsettings.json son incompatibles con la nueva librería | Baja | Muy Alto | Prueba de round-trip obligatoria en Fase 2 antes de avanzar |
| El feed Azure Artifacts no está configurado en el banco y el proceso tarda | Media | Medio | Operar con ProjectReference hasta que el feed esté listo; no bloquear el desarrollo |
| El credential provider de Azure Artifacts requiere configuración adicional en las máquinas del equipo | Media | Bajo | Documentar el proceso de setup y coordinarlo con el equipo antes del sprint |
| La carpeta `SmartHubSecretTool` tiene documentación en PDF que no debe perderse | Baja | Bajo | Mover el PDF a la nueva carpeta o a `docs/` antes de eliminar la carpeta original |
