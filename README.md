# Contactabilidad Inteligente (Smart Contactability)

Sistema de gestión de contactabilidad inteligente desarrollado con **.NET 9**, siguiendo principios de **Arquitectura Hexagonal**, **Domain-Driven Design (DDD)** y **Clean Code**.

## 🚀 Tecnologías y Herramientas

- **Framework:** .NET 9 (C# 13)
- **UI:** Razor Pages con Tailwind CSS
- **Persistencia:** EF Core 9 (SQL Server)
- **Estilos:** Tailwind CSS (Pipeline de compilación vía npm)
- **Patrones:** Repository, Unit of Work, Result Pattern, Factory Methods.

## 🏗️ Estructura del Proyecto

El proyecto se divide en dos grandes bloques:

- **`CI_API/`**: Núcleo del negocio y lógica de persistencia.
  - `SDH.Domain`: Entidades de dominio, lógica de negocio y contratos.
  - `SDH.Application`: Puertos (interfaces) y servicios de aplicación (casos de uso).
  - `SDH.infrastructure`: Implementación de adaptadores (EF Core, Repositorios, Servicios externos).
- **`CI_MVC/`**: Capa de presentación (UI).
  - `contactabilidad_inteligente.mcv`: Aplicación Razor Pages principal.

## 🛠️ Configuración Inicial

### Requisitos
- .NET 9 SDK
- SQL Server (o LocalDB)
- Node.js & npm (para la compilación de Tailwind CSS)

### Pasos de Instalación
1. **Configurar Base de Datos:**
   Ajusta la cadena de conexión en `CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=TU_SERVIDOR;Database=ContactabilidadInteligente;Trusted_Connection=true;..."
   }
   ```

2. **Instalar Dependencias de UI:**
   ```bash
   cd CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv
   npm install
   ```

3. **Aplicar Migraciones:**
   ```bash
   dotnet ef database update -p ../../../CI_API/SDH.infrastructure -s .
   ```

## 💻 Comandos de Desarrollo

Los siguientes comandos deben ejecutarse dentro del directorio de la aplicación MVC (`CI_MVC/contactabilidad_inteligente.mcv/contactabilidad_inteligente.mcv`):

### Compilar Tailwind CSS
```bash
npm run build:css
```

### Ejecutar la Aplicación
```bash
dotnet run
```

### Gestión de Migraciones
Para añadir una nueva migración:
```bash
dotnet ef migrations add <NombreMigracion> -p ../../../CI_API/SDH.infrastructure -s . --output-dir Persistence/Migrations
```

## 📜 Estándares de Implementación

### Dominios
- Las entidades del dominio deben tener **setters privados**.
- Utilizar **Factory Methods** (ej. `public static Cliente Crear(...)`) para instanciar entidades.
- Las validaciones de negocio residen en las entidades o servicios de dominio.

### Persistencia
- **Data Annotations:** Para mapeos simples (tablas, claves, longitudes).
- **Fluent API:** Exclusivamente para configuraciones complejas (índices compuestos, valores por defecto de BD, comportamientos de borrado).

### Servicios
- Usar el **Result Pattern** para retornos de servicios, evitando el uso de excepciones para control de flujo de negocio.
- Toda operación de base de datos debe pasar por el **Unit of Work**.

---
**Nota:** Para instrucciones detalladas sobre el comportamiento de la IA en este repositorio, consulte el archivo `GEMINI.md`.
