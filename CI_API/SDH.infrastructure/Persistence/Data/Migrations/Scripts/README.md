# Scripts de Migración SQL

Esta carpeta contiene los scripts SQL generados para aplicar en los entornos de Desarrollo y Producción.

## Convención de Nombres

Los scripts deben seguir el siguiente formato:
- `Prod_<DescripcionCambio>_YYYYMMDD_HHmmss.sql` - Scripts para producción
- `Dev_<DescripcionCambio>_YYYYMMDD_HHmmss.sql` - Scripts para desarrollo (opcional)

## Ejemplo

```
Prod_AgregarTablaCampania_20250124_153000.sql
Prod_ModificarColumnaCliente_20250125_090000.sql
```

## ?? Importante

- Todos los scripts deben ser **idempotentes** (pueden ejecutarse múltiples veces)
- Deben incluir validaciones (IF NOT EXISTS, etc.)
- Deben ser revisados antes de enviar a producción
- Mantener este directorio sincronizado con Git

## Generar Script

Para generar un nuevo script:

```powershell
dotnet ef migrations script --context ApplicationDbContext --output ".\Data\Migrations\Scripts\Prod_NombreDescriptivo_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql" --idempotent
```
