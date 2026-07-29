# Graph Report - .  (2026-07-20)

## Corpus Check
- 269 files · ~134,904 words
- Verdict: corpus is large enough that graph structure adds value.

## Summary
- 1493 nodes · 2680 edges · 149 communities (124 shown, 25 thin omitted)
- Extraction: 95% EXTRACTED · 5% INFERRED · 0% AMBIGUOUS · INFERRED: 145 edges (avg confidence: 0.79)
- Token cost: 450,792 input · 0 output

## Community Hubs (Navigation)
- SDH.Application/Services
- SDH.Application/Services
- Entities/Parametro
- contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv
- js/pages
- Persistence/Cache
- SDH.Application/Services
- contactabilidad_inteligente.mcv/Controllers
- Persistence/Repositories
- contactabilidad_inteligente.mcv/Pages
- SDH.Domain/Enums
- contactabilidad_inteligente.mcv/Controllers
- SDH.Application/Services
- js/ds
- js/ds
- PlanesDeTrabajo/Release_1.1.0
- SDH.Application/Services
- contactabilidad_inteligente.mcv/Properties
- js/pages
- contactabilidad_inteligente.mcv/Controllers
- Entities/Operative
- DTOs/Auth
- contactabilidad_inteligente.mcv/Pages
- js/pages
- Entities/Operative
- js/pages
- contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv
- Publicacion_Pro/2026-054
- contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv
- Entities/Operative
- contactabilidad_inteligente.mcv/Mappers
- SDH.infrastructure/Migrations
- js/components
- logs_pyhton
- Entities/Seguridad
- SmartHubSecretTool
- js/components
- SDH.Application/Services
- contactabilidad_inteligente.mcv/Pages
- Persistence/Seeders
- Tecnica/DB
- Persistence/Services
- Persistence/Repositories
- Persistence/Queries
- SDH.Domain/Repositories
- Tecnica/Diseño
- Ports/Services
- Entities/Operative
- docs/Archi
- DTOs/Clients
- js/ds
- SmartHubSecretTool/SDH.SecretTool
- Entities/Operative
- Persistence/Data
- js/ds
- Entities/Operative
- SDH.Domain/Ports
- SDH.Domain/Extensions
- DataConfigurations/Parametro
- Persistence/Repositories
- Tecnica/DB
- docs/Archi
- contactabilidad_inteligente.mcv/Pages
- js/ds
- .github/ISSUES
- SDH.Application
- DTOs/Clients
- Persistence/Services
- DB_ODS/Scripts
- Models/Client
- contactabilidad_inteligente.mcv/Pages
- contactabilidad_inteligente.mcv/Properties
- contactabilidad_inteligente.mcv/Properties
- docs/Tecnica
- Tecnica/Scripts
- Persistence/Configuration
- SDH.infrastructure/Persistence
- contactabilidad_inteligente.mcv/Helpers
- Views/Shared
- contactabilidad_inteligente.mcv/Views
- js/ds
- lib/jquery
- docs/Tecnica
- contactabilidad_inteligente.mcv/Pages
- DB_ODS/Scripts
- SDH.SQL.DB
- SDH.SQL.DB
- contactabilidad_inteligente.mcv/Models
- Pages/Auth
- Components/FileUpload
- contactabilidad_inteligente.mcv/Pages
- contactabilidad_inteligente.mcv/Pages
- docs/assets
- Jobs
- SDH.SQL.DB
- SDH.SQL.DB
- 2026-053/MVP
- 2026-053/MVP
- 2026-053/Alcance_1
- 2026-053/MVP
- Publicacion_Pro/2026-054
- Publicacion_Pro/2026-054
- Tecnica/DB
- Tecnica/DB

## God Nodes (most connected - your core abstractions)
1. `Client` - 34 edges
2. `Users` - 34 edges
3. `SDH.Domain.Enums` - 31 edges
4. `SDH.Application.Ports.Services` - 30 edges
5. `ApplicationDbContext` - 22 edges
6. `CatalogGroup` - 21 edges
7. `DashboardModel` - 20 edges
8. `init()` - 18 edges
9. `ARCHITECTURE.md — Arquitectura Hexagonal` - 18 edges
10. `SDH.Application.Services` - 17 edges

## Surprising Connections (you probably didn't know these)
- `CatalogosEnum Range Definitions` --semantically_similar_to--> `Issue: Parametrizar Catálogos de Códigos de Error`  [INFERRED] [semantically similar]
  CI_API/SDH.Domain/Enums/CatalogosEnum.txt → CI_MVC/contactabilidad_inteligente.mcv/.github/ISSUES/parametrizar-catalogos-codigos-error.md
- `SOLUCION_ERROR_TABLAS_EXISTENTES.md — Usuarios de Prueba` --semantically_similar_to--> `Contactabilidad Inteligente (Smart Contactability) — Root README`  [INFERRED] [semantically similar]
  CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/SOLUCION_ERROR_TABLAS_EXISTENTES.md → README.md
- `CatalogosEnum Range Definitions` --semantically_similar_to--> `Pase 1.1.0 — Notas de Liberación`  [INFERRED] [semantically similar]
  CI_API/SDH.Domain/Enums/CatalogosEnum.txt → docs/Funcional/Versionamiento/pase_1.1.0.md
- `Contactabilidad Inteligente (Smart Contactability) — Root README` --references--> `Procedimientos Operativos — Plataforma de Gestión de Datos`  [AMBIGUOUS]
  README.md → docs/Funcional/IPGestionDeDatos.md
- `DashboardModel` --references--> `IClienteQueryService`  [EXTRACTED]
  CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/Pages/Dashboard.cshtml.cs → CI_API/SDH.Application/Ports/Services/IClienteQueryService.cs

## Import Cycles
- None detected.

## Hyperedges (group relationships)
- **Documentos que definen la Arquitectura Hexagonal del sistema SDH** — ci_mvc_contactabilidad_inteligente_mcv_contactabilidad_inteligente_mcv_architecture, ci_mvc_contactabilidad_inteligente_mcv_contactabilidad_inteligente_mcv_docs_ds_back, ci_mvc_contactabilidad_inteligente_mcv_contactabilidad_inteligente_mcv_readme, readme [INFERRED 0.85]
- **Artefactos que definen y ejecutan el alcance del Release 1.1.0** — docs_archi_release_pgd, docs_planesdetrabajo_release_1_1_0_dt_plan_01_esquema_dominio, docs_planesdetrabajo_release_1_1_0_dt_plan_02_estandarizacion_prefijos, docs_funcional_versionamiento_pase_1_1_0 [INFERRED 0.85]
- **Tema de parametrización y verificación de catálogos configurables** — ci_api_sdh_domain_enums_catalogosenum, ci_mvc_contactabilidad_inteligente_mcv_github_issues_parametrizar_catalogos_codigos_error, docs_funcional_versionamiento_pase_1_1_0 [INFERRED 0.65]
- **SmartHubSecretTool / Bm.Security.App.SecretTool across docs** — docs_publicacion_pro_2026_053_mvp_manual_implementacion_iis_smarthubsecrettool, docs_tecnica_runbook_smarthubsecrettool, docs_planesdetrabajo_release_1_1_0_dt_plan_04_cifrado_herramientas_dt11_bm_security_app_secrettool, docs_vulnerabilidad_20260528_informe_smarthubsecrettool_program_cs [INFERRED 0.85]
- **Reporte Diario de Actividad — design, plan and deployment docs** — docs_superpowers_specs_2026_07_06_reporte_actividad_diaria_contactos_design_reportes_schema, docs_superpowers_plans_2026_07_06_reporte_actividad_diaria_contactos_reportes_schema_ddl, docs_publicacion_pro_2026_054_alcance_2026_54_reportes_schema [EXTRACTED 1.00]
- **DB_ODS schema documentation set** — docs_tecnica_db_diccionario_datos_dsh_schema_operativo, docs_tecnica_db_entidad_relacion_dsh_erd_diagram, docs_tecnica_db_estrategia_migracion_db_ods_index [INFERRED 0.85]

## Communities (149 total, 25 thin omitted)

### Community 0 - "SDH.Application/Services"
Cohesion: 0.08
Nodes (30): CatalogDetail, CatalogDiscrepancyDto, CancellationToken, IReadOnlyList, List, Task, ICatalogQueryService, CancellationToken (+22 more)

### Community 1 - "SDH.Application/Services"
Cohesion: 0.08
Nodes (27): Result, CreateUsuarioCommand, UpdateUsuarioCommand, UserDetailDto, UserListItemDto, UserStatusToggleDto, CancellationToken, IReadOnlyList (+19 more)

### Community 2 - "Entities/Parametro"
Cohesion: 0.07
Nodes (28): DateTime, IReadOnlyCollection, List, CatalogGroup, DateTime, CatalogItem, CancellationToken, Task (+20 more)

### Community 3 - "contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv"
Cohesion: 0.05
Nodes (43): net10.0, EPPlus (8.5.3), System.Security.Cryptography.Xml (10.0.6), Microsoft.NET.Sdk, SDH.Domain, net10.0, Microsoft.NET.Sdk, net10.0 (+35 more)

### Community 4 - "js/pages"
Cohesion: 0.11
Nodes (44): _apiContactError(), _apiSearchClientsByIds(), _apiVerify(), _applyVerificationState(), _asideIsEmpty(), _bindClientePerfilLinks(), _bindClientRowSelection(), _bindContactActions() (+36 more)

### Community 5 - "Persistence/Cache"
Cohesion: 0.10
Nodes (21): CustomerFinancialDto, CustomerSearchResultDto, DateTime, List, CustomerCacheModel, CancellationToken, IReadOnlyList, Task (+13 more)

### Community 6 - "SDH.Application/Services"
Cohesion: 0.12
Nodes (20): DateTime, FileUploadInfo, FileUploadStatus, SectionLightGreenFormAreaViewModel, IFormFile, ILogger, List, long (+12 more)

### Community 7 - "contactabilidad_inteligente.mcv/Controllers"
Cohesion: 0.13
Nodes (17): Authorize, AddAddressRequest, AddContactRequest, CustomerListItemDto, SaveGpsRequest, ICurrentUserService, EnumContactabilityType, ClientController (+9 more)

### Community 8 - "Persistence/Repositories"
Cohesion: 0.09
Nodes (18): CancellationToken, Task, IUnitOfWork, CancellationToken, Task, bool, CancellationToken, ILogger (+10 more)

### Community 9 - "contactabilidad_inteligente.mcv/Pages"
Cohesion: 0.07
Nodes (23): LoginModel, IActionResult, ILogger, Task, SecurityConfig, bool, Dictionary, int (+15 more)

### Community 10 - "SDH.Domain/Enums"
Cohesion: 0.08
Nodes (18): Attribute, MappedCatalog, string, CatalogGroups, EnumCustomerStatus, EnumFileStatus, EnumIdentificationType, EnumSystemRole (+10 more)

### Community 11 - "contactabilidad_inteligente.mcv/Controllers"
Cohesion: 0.11
Nodes (21): AuthController, HttpPost, IActionResult, Task, CreateUsuarioRequest, UpdateUsuarioRequest, UsersController, CancellationToken (+13 more)

### Community 12 - "SDH.Application/Services"
Cohesion: 0.15
Nodes (16): IFormFile, List, Task, FileExtractionResult, IFileExtractionService, IFormFile, ILogger, List (+8 more)

### Community 13 - "js/ds"
Cohesion: 0.09
Nodes (7): _apply(), init(), off(), on(), once(), reapply(), _updateCounts()

### Community 14 - "js/ds"
Cohesion: 0.18
Nodes (26): _bindFilterPills(), _bindKeyboard(), _bindListSearch(), _bindOutsideClick(), _bindSearchBar(), _buildArticleHTML(), _buildContactIconHTML(), _buildContactIconsHTML() (+18 more)

### Community 15 - "PlanesDeTrabajo/Release_1.1.0"
Cohesion: 0.10
Nodes (26): DT-07 Renombrado de grupos G_DSH_* a GS_DSH_*, DT-08 Creación del proyecto de pruebas unitarias, DT-09 Pruebas automatizadas con Selenium, Page Object Model pattern for Selenium tests, SDH.Application.Tests project, SDH.Domain.Tests project, SDH.UI.Tests Selenium project, AesCbcCryptoService implementation (+18 more)

### Community 16 - "SDH.Application/Services"
Cohesion: 0.15
Nodes (11): SDH.Application.Common, SDH.infrastructure.Persistence.Queries, SDH.infrastructure.Persistence.Ports.Services, SDH.Domain.Ports, SDH.Infrastructure.Persistence.Queries, SDH.Domain.Entities.Seguridad, SDH.Domain.Repositories, SDH.Infrastructure.Persistence.Repositories (+3 more)

### Community 17 - "contactabilidad_inteligente.mcv/Properties"
Cohesion: 0.08
Nodes (24): commandName, environmentVariables, launchBrowser, launchUrl, publishAllPorts, useSSL, ASPNETCORE_ENVIRONMENT, ASPNETCORE_HTTP_PORTS (+16 more)

### Community 18 - "js/pages"
Cohesion: 0.20
Nodes (24): _getErrorLabel(), _initAddDireccionForm(), _initAddEmailForm(), _initAddPhoneForm(), _initAddressMapPanel(), _initAll(), _initContactFilter(), _initEditToggle() (+16 more)

### Community 19 - "contactabilidad_inteligente.mcv/Controllers"
Cohesion: 0.15
Nodes (10): AgregarItemRequest, CrearGrupoRequest, ErrorCodeDto, SDH.Domain.Extensions, contactabilidad_inteligente.mcv.Helpers, contactabilidad_inteligente.mcv.Controllers, SDH.Application.Services, contactabilidad_inteligente.mcv.Models.Client (+2 more)

### Community 20 - "Entities/Operative"
Cohesion: 0.15
Nodes (10): CustomerAddressConfiguration.cs (Índice y columna LOPDP), FinancieroClienteConfiguration.cs, Scripts de Migración SQL README, Migración DT_CorreccionesEsquema_R110, SDH.infrastructure.Persistence.DataConfigurations.Operativo, SDH.Domain.Entities.Parametro, SDH.Domain.Entities.Operative, SDH.infrastructure.Persistence.DataConfigurations.Parametro (+2 more)

### Community 21 - "DTOs/Auth"
Cohesion: 0.13
Nodes (12): LdapAuthResult, CancellationToken, Task, ILdapAuthenticationService, Dictionary, string, AuthSettings, LdapSettings (+4 more)

### Community 22 - "contactabilidad_inteligente.mcv/Pages"
Cohesion: 0.18
Nodes (10): DashboardModel, SearchClientsByIdsRequest, CancellationToken, Dictionary, IActionResult, IFormFile, ILogger, IWebHostEnvironment (+2 more)

### Community 23 - "js/pages"
Cohesion: 0.23
Nodes (17): addToUploadHistory(), changeStep(), downloadReport(), formatFileSize(), getRelativeTime(), handleDrop(), handleFileSelect(), initializeButtons() (+9 more)

### Community 24 - "Entities/Operative"
Cohesion: 0.16
Nodes (7): DateTime, Dictionary, IReadOnlyCollection, List, Client, EntityTypeBuilder, ClienteConfiguration

### Community 25 - "js/pages"
Cohesion: 0.16
Nodes (6): _closeModal(), _initCreateUserForm(), _initEditUserForm(), _resetBtn(), _setLoading(), _val()

### Community 26 - "contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv"
Cohesion: 0.12
Nodes (16): autoprefixer, description, devDependencies, autoprefixer, eslint, postcss, tailwindcss, name (+8 more)

### Community 27 - "Publicacion_Pro/2026-054"
Cohesion: 0.12
Nodes (17): SerilogSettings config section added, Sitio IIS Data Smart Hub, JOB_Baseline_Carga_Inicial (pase), JOB_Reporte_Actividad_Diaria (pase), Schema reportes (pase 2026-054), SP_Baseline_Carga_Inicial (pase), SP_Reporte_Actividad_Diaria (pase), Task 1 — Schema y Tablas DDL reportes (+9 more)

### Community 28 - "contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv"
Cohesion: 0.17
Nodes (16): ICatalogoRepository (Port), IClienteRepository (Port), IUnitOfWork (Port), Cliente.cs (Domain Entity), ContactoCliente.cs (Domain Entity), FinancieroCliente.cs (Domain Entity), OficializacionCore.cs (Domain Entity), GrupoCatalogo.cs (Domain Entity) (+8 more)

### Community 29 - "Entities/Operative"
Cohesion: 0.13
Nodes (8): DateTime, IAuditableEntity, DateTime, IVerificableEntity, DateTime, CustomerAddresses, EntityTypeBuilder, CustomerAddressConfiguration

### Community 30 - "contactabilidad_inteligente.mcv/Mappers"
Cohesion: 0.28
Nodes (3): EnumContactabilityStatusExtensions, ClientViewModelMapper, List

### Community 31 - "SDH.infrastructure/Migrations"
Cohesion: 0.13
Nodes (9): ModelBuilder, InitCreate, InitCreate, ModelBuilder, ApplicationDbContextModelSnapshot, SDH.infrastructure.Migrations, Migration, MigrationBuilder (+1 more)

### Community 33 - "logs_pyhton"
Cohesion: 0.28
Nodes (15): build_summary_rows(), close_session(), main(), normalize_username(), OpenSession, parse_timestamp(), process_file(), datetime (+7 more)

### Community 34 - "Entities/Seguridad"
Cohesion: 0.15
Nodes (5): DateTime, Users, EntityTypeBuilder, UsuarioConfiguration, SDH.infrastructure.Persistence.DataConfigurations.Seguridad

### Community 35 - "SmartHubSecretTool"
Cohesion: 0.17
Nodes (15): DESIGN_SYSTEM.md (redirect stub), DS-Back.md — Senior Backend .NET Architect Prompt, SOLUCION_ERROR_TABLAS_EXISTENTES.md — Usuarios de Prueba, SmartHub Secret Tool — Manual de Usuario, UsuarioSeeder.cs, AES-256-CBC Encryption Scheme, CONFIG_MASTER_KEY (variable de entorno), ConfigCrypto.DecryptFromEnvironment (+7 more)

### Community 37 - "SDH.Application/Services"
Cohesion: 0.27
Nodes (8): ClaimsPrincipal, Users, LoginResultDto, ClaimsPrincipal, ITokenGenerator, CancellationToken, Task, AutenticacionService

### Community 38 - "contactabilidad_inteligente.mcv/Pages"
Cohesion: 0.19
Nodes (8): CustomerContactDto, ClientePerfilViewModel, DireccionPerfilViewModel, ClientesModel, IActionResult, IReadOnlyCollection, List, Task

### Community 39 - "Persistence/Seeders"
Cohesion: 0.21
Nodes (6): UsuarioSeeder, SDH.infrastructure.Persistence.Repositories, SDH.infrastructure.Persistence.Data, SDH.infrastructure.Persistence.Services, SDH.infrastructure.Persistence, SDH.infrastructure.Persistence.Seeders

### Community 40 - "Tecnica/DB"
Cohesion: 0.19
Nodes (14): Esquema parametro, Esquema seguridad, parametro.Tbl_Cat_Grupo, parametro.Tbl_Cat_Item, operativo.Tbl_Contacto_Cliente, operativo.Tbl_Direccion_Cliente, operativo.Tbl_Financiero_cliente, operativo.Tbl_Maest_Cliente (+6 more)

### Community 41 - "Persistence/Services"
Cohesion: 0.19
Nodes (6): IConfigDecryptionService, ClaimsPrincipal, IConfiguration, List, TokenGenerator, Claim

### Community 42 - "Persistence/Repositories"
Cohesion: 0.37
Nodes (4): CancellationToken, IEnumerable, Task, UserRepository

### Community 43 - "Persistence/Queries"
Cohesion: 0.35
Nodes (6): CustomerAddressDto, CancellationToken, IReadOnlyList, List, Task, ClienteQueryService

### Community 44 - "SDH.Domain/Repositories"
Cohesion: 0.39
Nodes (4): CancellationToken, IEnumerable, Task, IUserRepository

### Community 45 - "Tecnica/Diseño"
Cohesion: 0.17
Nodes (12): DS.api — wrapper HTTP fetch con CSRF, .ds-btn — componente de botones CSS, ds-core.js — window.DS namespace, _DsClientList.cshtml partial, _DsClientSearch.cshtml partial, DS.form — validación y submit AJAX, DS.modal — gestión de stack de modales, DS.notify — notificaciones toast/confirm (+4 more)

### Community 46 - "Ports/Services"
Cohesion: 0.38
Nodes (5): CancellationToken, IReadOnlyList, List, Task, IClienteQueryService

### Community 47 - "Entities/Operative"
Cohesion: 0.25
Nodes (5): DateTime, CustomerContacts, EnumContactabilityStatus, EntityTypeBuilder, ContactoClienteConfiguration

### Community 49 - "docs/Archi"
Cohesion: 0.25
Nodes (11): Área de Seguridad Bancaria, Id_Cliente_Canonico (Columna, Autorreferencia), LOPDP — Ley Orgánica de Protección de Datos Personales, Mesa de Ayuda Institucional, Release 1.0.0 — MVP, Release 1.2.0 — Motor Slider, Release 2.0.0 — Evolución Arquitectural, Slider — Motor de Deduplicación de Clientes (Release 1.2.0) (+3 more)

### Community 50 - "DTOs/Clients"
Cohesion: 0.29
Nodes (4): SDH.infrastructure.Persistence.Cache, SDH.Domain.Enums.LogicaNegocio, SDH.Application.Models, SDH.Application.DTOs.Clients

### Community 51 - "js/ds"
Cohesion: 0.33
Nodes (7): clearError(), _isValidEmail(), serialize(), setError(), setLoading(), submitAjax(), validate()

### Community 52 - "SmartHubSecretTool/SDH.SecretTool"
Cohesion: 0.22
Nodes (3): string, AesEncryptor, SDH.SecretTool

### Community 53 - "Entities/Operative"
Cohesion: 0.25
Nodes (4): ClientFinancial, EntityTypeBuilder, FinancieroClienteConfiguration, IEntityTypeConfiguration

### Community 54 - "Persistence/Data"
Cohesion: 0.22
Nodes (7): ModelBuilder, ApplicationDbContext, ILogger, Task, ViewSeeder, DbContext, DbSet

### Community 55 - "js/ds"
Cohesion: 0.42
Nodes (7): add(), getIds(), has(), _load(), remove(), _save(), setAll()

### Community 56 - "Entities/Operative"
Cohesion: 0.29
Nodes (4): DateTime, CoreOfficializations, EntityTypeBuilder, OficializacionCoreConfiguration

### Community 58 - "SDH.Domain/Extensions"
Cohesion: 0.32
Nodes (4): Dictionary, EnumCache, EnumExtensions, Enum

### Community 59 - "DataConfigurations/Parametro"
Cohesion: 0.32
Nodes (5): EntityTypeBuilder, VwCatalogoDetalleConfiguration, VwCatalogoDetalle, SDH.Infrastructure.Persistence.ReadModels.Parametro, SDH.Infrastructure.Persistence.DataConfigurations.Parametro

### Community 60 - "Persistence/Repositories"
Cohesion: 0.54
Nodes (3): CancellationToken, Task, CustomerRepository

### Community 61 - "Tecnica/DB"
Cohesion: 0.32
Nodes (8): DB_ODS_1.0.0.sql script de creación de estructura, Capa 2 — Migraciones EF Core (dotnet ef migrations script), Job_gration.sql — carga masiva Equifax, usr_ods login mínimo privilegio, Tipo A — Migración de esquema EF Core, Tipo B — Recarga/enriquecimiento periódico de datos, Tipo C — Hotfix de esquema DDL manual, Estrategia de Migración DB_ODS — documento índice

### Community 62 - "docs/Archi"
Cohesion: 0.29
Nodes (7): CatalogosEnum Range Definitions, CLAUDE.md (referenced target), Release 1.1.0 — Compliance, DT y Operativas SDH, DSH_Arq_emp.archimate (referenced target), Pase 1.1.0 — Notas de Liberación, DT Plan 02 — Estandarización del Nombre de la Plataforma (SDH), Manual_implementacion_IIS.md (referenced target)

### Community 63 - "contactabilidad_inteligente.mcv/Pages"
Cohesion: 0.29
Nodes (6): contactabilidad_inteligente.mcv.Helpers, contactabilidad_inteligente.mcv.Pages, SDH.Domain.Enums, SDH.Domain.Extensions, System.Text.Json, DashboardModel

### Community 64 - "js/ds"
Cohesion: 0.48
Nodes (5): buildFooterBtns(), close(), closeAll(), create(), focusFirst()

### Community 66 - ".github/ISSUES"
Cohesion: 0.40
Nodes (6): Issue: Parametrizar Catálogos de Códigos de Error, Pages/Clientes.cshtml, Pages/Dashboard.cshtml.cs, Tbl_Catalogo_Codigos_Error (Propuesta de Tabla), ErrorLabels Dictionary (hardcoded), ICatalogoService (Propuesto)

### Community 67 - "SDH.Application"
Cohesion: 0.40
Nodes (3): IServiceCollection, AppServiceInjection, SDH.Application

### Community 68 - "DTOs/Clients"
Cohesion: 0.40
Nodes (4): ContactVerificationStateDto, CustomerDto, FinancialItemDto, UpdateAddressCommand

### Community 70 - "DB_ODS/Scripts"
Cohesion: 0.50
Nodes (4): dbo].[__EFMigrationsHistory, operativo].[Tbl_Maest_Cliente, operativo].[vw_Clientes_Contactos_Detalle, parametro].[Tbl_Cat_Item

### Community 71 - "Models/Client"
Cohesion: 0.40
Nodes (3): ClientViewModel, List, ContactViewModel

### Community 72 - "contactabilidad_inteligente.mcv/Pages"
Cohesion: 0.40
Nodes (4): contactabilidad_inteligente.mcv.Helpers, SDH.Domain.Enums, System.Text.Json, contactabilidad_inteligente.mcv.Pages.ClientesModel

### Community 73 - "contactabilidad_inteligente.mcv/Properties"
Cohesion: 0.40
Nodes (4): dependencies, mssql1, connectionId, type

### Community 74 - "contactabilidad_inteligente.mcv/Properties"
Cohesion: 0.40
Nodes (4): dependencies, mssql1, connectionId, type

### Community 75 - "docs/Tecnica"
Cohesion: 0.60
Nodes (5): IIS Manager - Agregar sitio web, Agregar sitio web (Add Website action), IIS Manager (Internet Information Services), Sitios node (Sites), Server WT501PRO047 (BANCOMACH)

### Community 76 - "Tecnica/Scripts"
Cohesion: 0.60
Nodes (4): dbo].[__EFMigrationsHistory, parametro].[Tbl_Cat_Grupo, parametro].[Tbl_Cat_Item, parametro].[Vw_Cat_Detalle_General]
WITH SCHEMABINDING

### Community 77 - "Persistence/Configuration"
Cohesion: 0.50
Nodes (3): string, DatabaseOptions, SDH.infrastructure.Persistence.Configuration

### Community 78 - "SDH.infrastructure/Persistence"
Cohesion: 0.50
Nodes (3): IConfiguration, IServiceCollection, DependencyInjection

### Community 79 - "contactabilidad_inteligente.mcv/Helpers"
Cohesion: 0.50
Nodes (4): ContactabilityLabelHelper, ContactTypeMetadata, StateMetadata, Dictionary

### Community 80 - "Views/Shared"
Cohesion: 0.50
Nodes (3): IdentityUser, SignInManager<IdentityUser>, UserManager<IdentityUser>

### Community 81 - "contactabilidad_inteligente.mcv/Views"
Cohesion: 0.50
Nodes (3): SDH.Domain.Enums, SDH.Domain.Extensions, contactabilidad_inteligente.mcv.Models

### Community 83 - "lib/jquery"
Cohesion: 0.50
Nodes (4): jquery LICENSE, jquery-validation LICENSE, jquery-validation-unobtrusive LICENSE, MIT License

### Community 84 - "docs/Tecnica"
Cohesion: 0.67
Nodes (4): IIS Modificar Grupo de Aplicaciones Dialog, DataSmartHub IIS Application Pool, Managed Pipeline Mode: Integrated, .NET CLR Version: No Managed Code

### Community 94 - "docs/assets"
Cohesion: 1.00
Nodes (3): 63 Años Aniversario Campaign, Banco de Machala, Banco de Machala Logo

## Ambiguous Edges - Review These
- `Contactabilidad Inteligente (Smart Contactability) — Root README` → `Procedimientos Operativos — Plataforma de Gestión de Datos`  [AMBIGUOUS]
  README.md · relation: references

## Knowledge Gaps
- **188 isolated node(s):** `FinancialItemDto`, `UpdateAddressCommand`, `net10.0`, `EPPlus (8.5.3)`, `Microsoft.AspNetCore.Http.Features (5.0.17)` (+183 more)
  These have ≤1 connection - possible missing edges or undocumented components.
- **25 thin communities (<3 nodes) omitted from report** — run `graphify query` to explore isolated nodes.

## Suggested Questions
_Questions this graph is uniquely positioned to answer:_

- **What is the exact relationship between `Contactabilidad Inteligente (Smart Contactability) — Root README` and `Procedimientos Operativos — Plataforma de Gestión de Datos`?**
  _Edge tagged AMBIGUOUS (relation: references) - confidence is low._
- **Why does `SDH.Application.Ports.Services` connect `SDH.Application/Services` to `SDH.Application`, `Persistence/Services`, `contactabilidad_inteligente.mcv/Controllers`, `Persistence/Repositories`, `Persistence/Services`, `Persistence/Seeders`, `contactabilidad_inteligente.mcv/Controllers`, `SDH.Application/Services`, `contactabilidad_inteligente.mcv/Pages`, `DTOs/Clients`, `contactabilidad_inteligente.mcv/Controllers`, `Entities/Operative`, `DTOs/Auth`?**
  _High betweenness centrality (0.081) - this node is a cross-community bridge._
- **Why does `ApplicationDbContext` connect `Persistence/Data` to `Entities/Parametro`, `Entities/Seguridad`, `contactabilidad_inteligente.mcv/Controllers`, `Persistence/Repositories`, `Persistence/Repositories`, `Persistence/Queries`, `Entities/Operative`, `Entities/Operative`, `Entities/Operative`, `Entities/Operative`, `Entities/Operative`, `DataConfigurations/Parametro`, `Persistence/Repositories`, `Entities/Operative`?**
  _High betweenness centrality (0.067) - this node is a cross-community bridge._
- **Why does `SDH.Domain.Enums` connect `SDH.Domain/Enums` to `Entities/Parametro`, `contactabilidad_inteligente.mcv/Controllers`, `Persistence/Seeders`, `SDH.Application/Services`, `DTOs/Clients`, `contactabilidad_inteligente.mcv/Controllers`, `Entities/Operative`?**
  _High betweenness centrality (0.059) - this node is a cross-community bridge._
- **What connects `FinancialItemDto`, `UpdateAddressCommand`, `net10.0` to the rest of the system?**
  _188 weakly-connected nodes found - possible documentation gaps or missing edges._
- **Should `SDH.Application/Services` be split into smaller, more focused modules?**
  _Cohesion score 0.07704918032786885 - nodes in this community are weakly interconnected._
- **Should `SDH.Application/Services` be split into smaller, more focused modules?**
  _Cohesion score 0.08311688311688312 - nodes in this community are weakly interconnected._