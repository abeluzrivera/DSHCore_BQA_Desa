# Arquitectura Hexagonal - Contactabilidad Inteligente

## ?? Estructura del Proyecto

Este proyecto implementa **Arquitectura Hexagonal** (también conocida como Ports & Adapters) siguiendo principios de **Clean Code** y **Domain-Driven Design (DDD)**.

## ??? Capas de la Arquitectura

### 1. **Domain Layer** (Núcleo)
Contiene las entidades del dominio con lógica de negocio encapsulada.

```
Domain/
??? Entities/
?   ??? Seguridad/
?   ?   ??? Usuario.cs
?   ??? Operativo/
?   ?   ??? Cliente.cs
?   ?   ??? FinancieroCliente.cs
?   ?   ??? ContactoCliente.cs
?   ?   ??? OficializacionCore.cs
?   ??? Carga/
?   ?   ??? EquifaxImportacion.cs
?   ??? Parametro/
?       ??? GrupoCatalogo.cs
?       ??? ItemCatalogo.cs
```

**Características:**
- ? Propiedades privadas con setters privados
- ? Factory Methods para creación de entidades
- ? Encapsulación de lógica de negocio
- ? Navigation properties para relaciones

### 2. **Application Layer** (Puertos)
Define las interfaces (contratos) que deben implementar los adaptadores.

```
Application/
??? Ports/
    ??? Repositories/
        ??? IClienteRepository.cs
        ??? IUsuarioRepository.cs
        ??? ICatalogoRepository.cs
        ??? IUnitOfWork.cs
```

**Características:**
- ? Interfaces independientes de frameworks
- ? Contratos para operaciones CRUD
- ? Patrón Unit of Work para transacciones

### 3. **Infrastructure Layer** (Adaptadores)
Implementaciones concretas de los puertos usando EF Core.

```
Infrastructure/
??? Persistence/
    ??? Configurations/
    ?   ??? Seguridad/
    ?   ?   ??? UsuarioConfiguration.cs
    ?   ??? Operativo/
    ?   ?   ??? ClienteConfiguration.cs
    ?   ?   ??? FinancieroClienteConfiguration.cs
    ?   ?   ??? ContactoClienteConfiguration.cs
    ?   ?   ??? OficializacionCoreConfiguration.cs
    ?   ??? Carga/
    ?   ?   ??? EquifaxImportacionConfiguration.cs
    ?   ??? Parametro/
    ?       ??? GrupoCatalogoConfiguration.cs
    ?       ??? ItemCatalogoConfiguration.cs
    ??? Repositories/
    ?   ??? ClienteRepository.cs
    ?   ??? UsuarioRepository.cs
    ?   ??? CatalogoRepository.cs
    ?   ??? UnitOfWork.cs
    ??? DependencyInjection.cs
```

**Características:**
- ? EF Core Fluent API Configurations
- ? Mapeo de esquemas de base de datos
- ? Implementaciones de repositorios
- ? Unit of Work con manejo de transacciones

## ?? Modelo de Base de Datos

### Esquemas:
- **seguridad**: Gestión de usuarios y autenticación
- **operativo**: Clientes y operaciones principales
- **carga**: Datos de staging/importación
- **parametro**: Catálogos configurables

### Relaciones Principales:
- `Cliente` (1) ? (N) `FinancieroCliente`
- `Cliente` (1) ? (N) `ContactoCliente`
- `Cliente` (1) ? (N) `OficializacionCore`
- `GrupoCatalogo` (1) ? (N) `ItemCatalogo`

## ?? Uso de los Repositorios

### Ejemplo 1: Crear un Cliente

```csharp
public class ClienteService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<long> CrearClienteAsync(string identificacion, string nombre, string usuario)
    {
        // Verificar si existe
        if (await _unitOfWork.Clientes.ExisteAsync(identificacion))
        {
            throw new InvalidOperationException("El cliente ya existe");
        }

        // Crear usando factory method
        var cliente = Cliente.Crear(identificacion, nombre, usuario);
        
        // Agregar financiero
        var financiero = FinancieroCliente.Crear(0, "Activo", 1000.00m);
        cliente.AgregarFinanciero(financiero);

        // Guardar
        await _unitOfWork.Clientes.AgregarAsync(cliente);
        await _unitOfWork.SaveChangesAsync();

        return cliente.IdCliente;
    }
}
```

### Ejemplo 2: Consultar Cliente con Relaciones

```csharp
public async Task<Cliente?> ObtenerClienteCompletoAsync(long id)
{
    // El repositorio ya incluye todas las relaciones
    return await _unitOfWork.Clientes.ObtenerPorIdAsync(id);
}
```

### Ejemplo 3: Usar Transacciones

```csharp
public async Task ProcesarCargaMasivaAsync(List<EquifaxImportacion> datos)
{
    await _unitOfWork.BeginTransactionAsync();
    
    try
    {
        foreach (var dato in datos)
        {
            var cliente = Cliente.Crear(dato.Identificacion, dato.Nombres, "Sistema");
            await _unitOfWork.Clientes.AgregarAsync(cliente);
        }
        
        await _unitOfWork.CommitTransactionAsync();
    }
    catch
    {
        await _unitOfWork.RollbackTransactionAsync();
        throw;
    }
}
```

## ?? Configuración

### 1. Connection String
Configurar en `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=ContactabilidadInteligente;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 2. Generar Migración

```bash
dotnet ef migrations add InitialCreate --output-dir Data/Migrations
```

### 3. Aplicar Migración

```bash
dotnet ef database update
```

## ? Principios Implementados

### Clean Code:
- ? Nombres descriptivos y significativos
- ? Métodos pequeños y con responsabilidad única
- ? Comentarios XML para documentación
- ? Separación de concerns

### SOLID:
- ? **S**ingle Responsibility: Cada clase tiene una responsabilidad
- ? **O**pen/Closed: Extensible sin modificar código existente
- ? **L**iskov Substitution: Interfaces bien definidas
- ? **I**nterface Segregation: Interfaces específicas
- ? **D**ependency Inversion: Dependencias sobre abstracciones

### Domain-Driven Design:
- ? Entidades con comportamiento
- ? Factory Methods para creación
- ? Encapsulación de lógica de negocio
- ? Value Objects (colecciones de solo lectura)

### Hexagonal Architecture:
- ? Núcleo del dominio independiente
- ? Puertos (interfaces) bien definidos
- ? Adaptadores intercambiables
- ? Fácil testing y mantenimiento

## ?? Referencias

- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [EF Core Best Practices](https://docs.microsoft.com/en-us/ef/core/)
- [Clean Code by Robert C. Martin](https://www.amazon.com/Clean-Code-Handbook-Software-Craftsmanship/dp/0132350882)
