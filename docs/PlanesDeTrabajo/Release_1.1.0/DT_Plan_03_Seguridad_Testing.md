# Plan de Trabajo — Deuda Técnica Items 7, 8 y 9
## Grupos de Seguridad, Pruebas Unitarias y Automatización Selenium
### Release 1.1.0 — Paquete E

**Fecha:** 05 de junio de 2026  
**Estado:** Planificado  
**Responsable:** Por asignar  
**Estimación:** 5-8 días de desarrollo + coordinación con equipo de directorio de identidad

---

## Objetivo

Homologar la convención de grupos de seguridad entre ambientes, establecer la base de pruebas unitarias del repositorio y crear el proyecto de automatización de interfaz con Selenium. Estos tres items son independientes entre sí y pueden distribuirse entre miembros del equipo.

---

## DT-07 — Renombrado de grupos de seguridad de `G_DSH_*` a `GS_DSH_*`

### Contexto

Los grupos del directorio de identidad corporativo (Active Directory) que controlan el acceso a la plataforma SDH tienen la convención `G_DSH_*` en los ambientes de certificación y desarrollo, mientras que en producción la convención es `GS_DSH_*`. La inconsistencia obliga a mantener configuraciones distintas por ambiente y genera confusión operativa.

### Investigación previa requerida

El nombre actual de los grupos en código no está en los archivos `.cs` del repositorio (la búsqueda de `G_DSH` solo devolvió resultados en documentación y archivos `.archimate`). Los grupos están referenciados en la configuración de la aplicación. Antes de planificar el cambio, localizar dónde están definidos:

1. Buscar en archivos de configuración:
   ```powershell
   Select-String -Path "D:\sources\DSHCore_BQA_Desa\CI_MVC\**\appsettings*.json" -Pattern "G_DSH" -Recurse
   Select-String -Path "D:\sources\DSHCore_BQA_Desa\CI_MVC\**\Program.cs" -Pattern "G_DSH" -Recurse
   Select-String -Path "D:\sources\DSHCore_BQA_Desa\CI_API\**\*.cs" -Pattern "GroupMappings\|LdapGroup\|G_DSH" -Recurse
   ```
2. Revisar `appsettings.json`, `appsettings.Development.json`, `appsettings.Staging.json` para encontrar la sección de configuración de grupos LDAP.
3. Revisar el commit `b58aece` ("Update GroupMappings for AD") en el log de git para entender qué configuración se estableció y en qué archivos.

### Archivos esperados con referencias a grupos AD

Basado en la arquitectura del sistema (tres proveedores de autenticación configurados en Program.cs), los grupos probablemente están en:

| Archivo | Ubicación probable |
|---------|-------------------|
| `appsettings.json` | Sección `LdapSettings.GroupMappings` o similar |
| `appsettings.Development.json` | Override de grupos para el ambiente de desarrollo |
| `appsettings.Staging.json` | Override de grupos para certificación |
| `Program.cs` | Configuración de políticas de autorización |

### Alcance del cambio

El renombrado se hace en dos planos:

**Plano 1 — Configuración de la aplicación (código/config):**

Actualizar los valores de configuración en `appsettings.Development.json` y `appsettings.Staging.json`:
- Reemplazar `G_DSH_ADMIN` → `GS_DSH_ADMIN`
- Reemplazar `G_DSH_SUPERVISOR` → `GS_DSH_SUPERVISOR`
- Reemplazar `G_DSH_AGENTE` → `GS_DSH_AGENTE`
- Reemplazar `G_DSH_CONSULTA` → `GS_DSH_CONSULTA`
- (Verificar que existen exactamente estos cuatro grupos o si hay más)

**Plano 2 — Directorio de identidad (operación fuera del repositorio):**

Coordinar con el equipo responsable del Active Directory en los ambientes de certificación y desarrollo para:
- Crear los nuevos grupos `GS_DSH_*` con los mismos miembros que los grupos `G_DSH_*` actuales, o
- Renombrar los grupos existentes directamente en AD.

Este paso es operativo y no puede hacerse desde el repositorio. Requiere una ventana coordinada para no interrumpir accesos.

### Sub-tarea: Coordinación con el equipo de seguridad

Según la formulación del DT-07, se debe "conversar con seguridad para armar una arquitectura de implementación centralizada". Los puntos a definir en esa reunión son:

1. ¿Se crean grupos nuevos o se renombran los existentes?
2. ¿El proceso de sincronización de miembros entre `G_DSH_*` y `GS_DSH_*` se hace manualmente o con un script?
3. ¿Cuándo se activa la nueva configuración en la aplicación — antes o después del cambio en AD?
4. ¿Existe un plan de rollback si el cambio impide el acceso a usuarios?
5. ¿Se centraliza la gestión de grupos en una herramienta corporativa (Identity Governance)?

### Pasos de implementación

1. Completar la investigación previa para localizar los archivos de configuración.
2. Agendar reunión con el equipo de seguridad (sub-tarea).
3. Acordar la estrategia de transición (ventana, orden de cambios).
4. Actualizar los archivos de configuración de la aplicación con los nuevos nombres de grupo.
5. Coordinar el cambio en Active Directory con el equipo de directorio.
6. Aplicar la nueva configuración en el ambiente de certificación.
7. Verificar que todos los perfiles (ADMIN, SUPERVISOR, AGENTE, CONSULTA) pueden autenticarse correctamente.
8. Repetir para el ambiente de desarrollo.

### Estrategia de rollback

En caso de fallo, la aplicación debe poder revertir al nombre de grupo antiguo en minutos. Para ello:
- Mantener `appsettings.Development.json` y `appsettings.Staging.json` en git con los valores viejos como rama de respaldo.
- Considerar usar la sección de `appsettings` con soporte de recarga en caliente (si está configurado) para que el cambio no requiera reinicio del servicio IIS.

### Criterios de aceptación

- Todos los usuarios de los ambientes de certificación y desarrollo pueden autenticarse con sus credenciales habituales después del cambio.
- Los cuatro perfiles (ADMIN, SUPERVISOR, AGENTE, CONSULTA) funcionan correctamente.
- No existe ninguna referencia a `G_DSH_*` en los archivos de configuración activos.
- El equipo de seguridad ha validado la arquitectura de implementación centralizada.

---

## DT-08 — Creación del proyecto de pruebas unitarias

### Contexto

La solución no tiene ningún proyecto de pruebas. Esto implica que cualquier cambio de refactoring (como los de DT-01 a DT-06) no tiene validación automatizada que proteja contra regresiones. El proyecto de pruebas es la base sobre la que se construirá la confianza en los cambios del release.

### Decisiones de diseño previas

Antes de crear el proyecto, definir:

| Decisión | Opciones | Recomendación |
|----------|---------|---------------|
| Framework de testing | xUnit, NUnit, MSTest | xUnit (estándar de .NET moderno) |
| Framework de mocking | Moq, NSubstitute, FakeItEasy | Moq (más adoptado en .NET) |
| Cobertura de código | Coverlet | Coverlet integrado con xUnit |
| Estructura | Un proyecto para todo, o separado por capa | Un proyecto por capa (Domain, Application) |

### Estructura propuesta

```
CI_Tests/
  SDH.Domain.Tests/           (o DSH.Domain.Tests si se aplica DT-06)
    SDH.Domain.Tests.csproj
    Entities/
      Operative/
        ClientFinancialTests.cs
        CustomerAddressesTests.cs
        CustomerContactsTests.cs
        ClientTests.cs
  SDH.Application.Tests/      (o DSH.Application.Tests)
    SDH.Application.Tests.csproj
    Services/
      CustomerCommandServiceTests.cs
```

La separación por capa tiene la ventaja de que el proyecto de pruebas de Domain no necesita las dependencias de infraestructura (EF Core, LDAP). El proyecto de pruebas de Application sí necesitará Moq para mockear repositorios.

### Pasos de implementación

#### Crear el proyecto de Domain Tests

```bash
# Desde la raíz del repositorio
dotnet new xunit -n SDH.Domain.Tests -o CI_Tests/SDH.Domain.Tests
cd CI_Tests/SDH.Domain.Tests
dotnet add reference ../../../CI_API/SDH.Domain/SDH.Domain.csproj
dotnet add package Moq
```

#### Agregar a la solución

```bash
# Desde la carpeta de la solución
dotnet sln contactabilidad_inteligente.mcv.sln add \
  ../../../../../../CI_Tests/SDH.Domain.Tests/SDH.Domain.Tests.csproj
```

#### Crear el proyecto de Application Tests

```bash
dotnet new xunit -n SDH.Application.Tests -o CI_Tests/SDH.Application.Tests
cd CI_Tests/SDH.Application.Tests
dotnet add reference ../../../CI_API/SDH.Domain/SDH.Domain.csproj
dotnet add reference ../../../CI_API/SDH.Application/SDH.Application.csproj
dotnet add package Moq
```

#### Agregar a la solución

```bash
dotnet sln contactabilidad_inteligente.mcv.sln add \
  ../../../../../../CI_Tests/SDH.Application.Tests/SDH.Application.Tests.csproj
```

### Pruebas iniciales a implementar

El objetivo del release es tener pruebas que cubran los items de deuda técnica corregidos y los comportamientos críticos del dominio. No se busca cobertura total; se busca una red de seguridad funcional.

#### SDH.Domain.Tests — Pruebas de entidades

**ClientFinancialTests.cs** (cubre DT-01):
- `Create_DebeAsignarAccountingType_ConValorCorrecto` — verifica que `AccountingType` se asigna al crear.
- `UpdateType_DebeActualizarAccountingType` — verifica que `UpdateType()` modifica la propiedad.
- `Create_NoDebeAsignarClientId_PropiedadNoExiste` — verifica que la propiedad renombrada ya no existe (compilación).

**CustomerAddressesTests.cs** (cubre DT-02):
- `Create_DebeAsignarSourceDesdeParametro` — verifica que `Source` toma el valor del parámetro.
- `Create_NoDebeHardcodearSystemUser` — verifica que al pasar un origen distinto, no se sobrescribe con `SystemUser`.
- `Create_DebeAsignarEstadoPendienteAlCrear` — verifica el estado de verificación inicial.
- `Verify_DebeActualizarEstadoAVerificado` — verifica el flujo de verificación.
- `UnVerify_DebeActualizarEstadoAlError` — verifica el rechazo.

**ClientTests.cs** (comportamientos críticos del agregado):
- `AgregarDireccion_NoDebePermitirMasDeUnaActiva` (si existe esa regla de dominio).
- `AgregarContacto_DebeRegistrarseConEstadoPendiente`.

#### SDH.Application.Tests — Pruebas de servicios

**CustomerCommandServiceTests.cs**:
- `RegistrarDireccion_DebeLlamarACreate_ConSourceCorrecto` — usando Moq para el repositorio.
- `VerificarContacto_DebeLlamarAUnitOfWork_Commit` — verifica que siempre se hace commit a través de IUnitOfWork.

### Configuración de Coverlet (cobertura)

En cada `.csproj` de pruebas:
```xml
<ItemGroup>
  <PackageReference Include="coverlet.collector" Version="6.*" />
</ItemGroup>
```

Comando para ejecutar con cobertura:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Criterios de aceptación

- Los dos proyectos de pruebas existen en la solución y compilan.
- Las pruebas listadas arriba están implementadas y pasan.
- `dotnet test` desde la raíz ejecuta todas las pruebas sin fallos.
- La cobertura de las entidades del dominio afectadas por DT-01 y DT-02 es de al menos 80%.

---

## DT-09 — Pruebas automatizadas con Selenium

### Contexto

Las pruebas unitarias (DT-08) validan la lógica de negocio aislada. Las pruebas de Selenium validan los flujos de usuario completos en el navegador, incluyendo navegación, formularios, y comportamientos de la UI que no son verificables desde el backend.

### Tecnología recomendada

| Componente | Opción |
|-----------|--------|
| Driver de navegador | Selenium WebDriver |
| Framework de pruebas | xUnit (mismo que el proyecto de unit tests) |
| Gestión de drivers | `WebDriverManager.Net` (evita instalar chromedriver manualmente) |
| Patrón de diseño | Page Object Model (POM) |
| Navegador principal | Chrome (headless para CI) |

### Estructura propuesta

```
CI_Tests/
  SDH.UI.Tests/               (o DSH.UI.Tests si se aplica DT-06)
    SDH.UI.Tests.csproj
    Pages/
      LoginPage.cs
      DashboardPage.cs
      ClientDetailPage.cs
    Tests/
      AuthTests.cs
      DashboardTests.cs
      ContactVerificationTests.cs
      BulkUploadTests.cs
    Fixtures/
      WebDriverFixture.cs      (ciclo de vida del driver)
      AppFixture.cs            (URL base, credenciales de prueba)
    appsettings.test.json      (URL del ambiente de prueba, credenciales)
```

### Pasos de implementación

#### Crear el proyecto

```bash
dotnet new xunit -n SDH.UI.Tests -o CI_Tests/SDH.UI.Tests
cd CI_Tests/SDH.UI.Tests
dotnet add package Selenium.WebDriver
dotnet add package Selenium.Support
dotnet add package WebDriverManager
dotnet add package Microsoft.Extensions.Configuration.Json
```

#### Agregar a la solución

```bash
dotnet sln contactabilidad_inteligente.mcv.sln add \
  ../../../../../../CI_Tests/SDH.UI.Tests/SDH.UI.Tests.csproj
```

#### Configurar el fixture del WebDriver

`WebDriverFixture.cs` debe:
- Inicializar el driver Chrome en modo headless para CI.
- Exponer la URL base leída desde `appsettings.test.json`.
- Implementar `IDisposable` para cerrar el browser al terminar.

```csharp
// Estructura esperada de WebDriverFixture
public class WebDriverFixture : IDisposable
{
    public IWebDriver Driver { get; private set; }
    public string BaseUrl { get; private set; }

    public WebDriverFixture()
    {
        // Inicialización del driver
        // Lectura de configuración
    }

    public void Dispose() => Driver.Quit();
}
```

#### Implementar el patrón Page Object Model

Cada página de la aplicación tiene una clase Page que encapsula los selectores y las acciones. Ejemplo:

```csharp
// Pages/LoginPage.cs
public class LoginPage
{
    private readonly IWebDriver _driver;

    public LoginPage(IWebDriver driver) => _driver = driver;

    public void Login(string username, string password)
    {
        _driver.FindElement(By.Id("username")).SendKeys(username);
        _driver.FindElement(By.Id("password")).SendKeys(password);
        _driver.FindElement(By.Id("btn-login")).Click();
    }

    public bool IsLogged() =>
        _driver.Url.Contains("/dashboard");
}
```

#### Flujos críticos a cubrir (primera iteración)

| Test | Descripción | Página |
|------|-------------|--------|
| `Login_CredencialesValidas_RedirectADashboard` | Autenticación exitosa | Login → Dashboard |
| `Login_CredencialesInvalidas_MuestraMensajeError` | Rechazo de credenciales inválidas | Login |
| `Dashboard_BusquedaCliente_MuestraResultados` | Búsqueda con 3+ caracteres retorna resultados | Dashboard |
| `FichaCliente_AbrirDesdeResultadoBusqueda` | Navegar a la ficha completa desde un resultado | Dashboard → ClientDetail |
| `FichaCliente_VerificarContacto_RegistraResultado` | Aprobar un contacto y confirmar cambio de estado | ClientDetail |
| `FichaCliente_RechazarContacto_RequiereMotivoYRegistra` | Rechazar un contacto con motivo seleccionado | ClientDetail |
| `CargaMasiva_SubirArchivoExcel_MuestraResultados` | Subir un Excel y ver resultados reales | BulkUpload |

No se busca cobertura exhaustiva en la primera iteración. El objetivo es tener una suite que detecte regresiones en los flujos principales del MVP.

#### Credenciales y datos de prueba

- Las credenciales para pruebas deben residir en `appsettings.test.json` (no commiteado) o en variables de entorno.
- Los datos de prueba (clientes, contactos) deben existir en la base de datos del ambiente de prueba antes de ejecutar los tests.
- Los tests no deben crear ni modificar datos permanentes; deben usar datos de prueba pre-existentes o limpiar tras cada ejecución.

### Consideraciones sobre el ambiente

- Las pruebas de Selenium necesitan una instancia corriendo de la aplicación. En CI, esto implica levantar la app antes de ejecutar los tests (o apuntar a un ambiente estable).
- Definir si los tests corren contra el ambiente de desarrollo local, certificación, o un ambiente de prueba dedicado.
- Para ejecución en CI sin monitor, Chrome debe ejecutarse en modo headless:
  ```csharp
  var options = new ChromeOptions();
  options.AddArgument("--headless");
  options.AddArgument("--no-sandbox");
  options.AddArgument("--disable-dev-shm-usage");
  ```

### Integración con el pipeline CI/CD

Agregar en el pipeline (cuando esté disponible) un step de UI tests que:
1. Levante la aplicación en modo test.
2. Ejecute `dotnet test CI_Tests/SDH.UI.Tests/`.
3. Genere un reporte de resultados.

Este step debe ejecutarse después de los tests unitarios y solo si la aplicación levanta correctamente.

### Criterios de aceptación

- El proyecto `SDH.UI.Tests` existe en la solución y compila.
- Los 7 tests listados arriba están implementados.
- Los tests pasan cuando se ejecutan contra el ambiente de desarrollo con datos de prueba preparados.
- Los tests usan Page Object Model y no tienen selectores duplicados entre clases.
- El `WebDriverFixture` cierra el browser correctamente al terminar, sin procesos `chromedriver` huérfanos.
- Existe un archivo `appsettings.test.json.example` commiteado con la estructura de configuración (sin valores reales).

---

## Orden de ejecución recomendado

```
DT-07 (coordinación AD) ← iniciar primero por el lead time de gestiones con seguridad
DT-08 (unit tests)      ← iniciar en paralelo con DT-07
DT-09 (Selenium)        ← iniciar después de DT-08 (misma persona puede continuar)
```

DT-07 tiene dependencia externa (equipo de AD), por lo que debe iniciarse antes aunque su implementación en código sea corta.

---

## Riesgos

| Riesgo | Probabilidad | Impacto | Mitigación |
|--------|-------------|---------|------------|
| El equipo de AD tarda más de lo esperado en hacer el cambio de grupos | Alta | Medio | Dejar la aplicación funcional con los grupos viejos hasta que AD confirme el cambio; el código de configuración se puede actualizar de forma independiente |
| Los tests de Selenium son frágiles por selectores que cambian con cada release de UI | Media | Medio | Usar atributos `data-testid` en los elementos de la UI para que los selectores no dependan de clases CSS o estructura HTML |
| No existe un ambiente estable para ejecutar Selenium en CI | Media | Alto | Definir la estrategia de ambiente antes de implementar; considerar un ambiente de staging estable como destino de los tests |
| El proyecto de pruebas unitarias queda sin mantenimiento tras el release | Media | Alto | Incluir en la definición de "Done" de cada historia futura que requiera agregar o actualizar tests |
