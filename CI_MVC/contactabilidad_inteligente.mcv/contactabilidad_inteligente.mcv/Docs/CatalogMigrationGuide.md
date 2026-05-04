# 📋 Guía de Migración a Catálogos Centralizados

## 🎯 Objetivo
Eliminar hardcoding de catálogos en archivos JS y C# (PageModels) y centralizar en `ContactabilityLabelHelper` + `DS.catalogs`.

## ✅ Archivos Migrados
- ✅ `dashboard.js` - Eliminados `ERROR_CATALOG` y `CONTACT_LABELS`
- ✅ `Clientes.cshtml.cs` - Eliminados `ContactTypeRegistry` y `StateRegistry`
- ✅ `ContactabilityLabelHelper.cs` - Extendido con metadata de tipos y estados

## 📂 Archivos Pendientes de Revisión
Los siguientes archivos pueden contener catálogos hardcodeados:
- `ds-search.js` (posiblemente tenga iconos o labels hardcodeados)
- Otros PageModels que mapeen contactos/direcciones

## 🔧 Cómo Migrar

### ANTES (Hardcodeado en C#):
```csharp
private static readonly Dictionary<string, ContactTypeMetadata> ContactTypeRegistry = new()
{
    ["CEL"] = new("smartphone", "phone", "Celular", 0),
    // ...
};

var metadata = ContactTypeRegistry.GetValueOrDefault(contacto.TipoMedioContacto);
```

### DESPUÉS (Centralizado):
```csharp
using contactabilidad_inteligente.mcv.Helpers;

var metadata = ContactabilityLabelHelper.ContactTypes.GetValueOrDefault(contacto.TipoMedioContacto);
```

### ANTES (Hardcodeado en JS):
```javascript
const icons = {
    'EMAIL': 'mail', 'CEL': 'smartphone'
};
const icon = icons[type] || 'contact_page';
```

### DESPUÉS (Centralizado):
```javascript
const icon = DS.catalogs.getContactIcon(type); // Carga desde backend
```

## 📊 API del Módulo DS.catalogs (EXTENDIDA)

### Backend Endpoints (CatalogosController)

#### `GET /api/catalogos/error-codes`
Retorna errores de contactabilidad agrupados por categoría.
```json
{
  "email": [{"code": "ERR_MAIL_FMT", "label": "Formato no válido"}],
  "phone": [{"code": "ERR_TEL_FMT", "label": "Formato no válido"}],
  "address": [{"code": "ERR_DIR_SHORT", "label": "Demasiado corta"}]
}
```

#### `GET /api/catalogos/contact-types` ✨ NUEVO
Retorna tipos de contacto con metadata completa.
```json
[
  {
    "code": "CEL",
    "icon": "smartphone",
    "iconType": "phone",
    "label": "Celular",
    "order": 0,
    "shouldTruncate": false
  }
]
```

#### `GET /api/catalogos/contact-states` ✨ NUEVO
Retorna estados de contactabilidad con clases Tailwind.
```json
[
  {
    "code": "Verificado",
    "bgClass": "tw-bg-verified-bg",
    "borderClass": "tw-border-verified-border",
    "textClass": "tw-text-verified-text",
    "state": "verified"
  }
]
```

### Frontend API (DS.catalogs)

#### Métodos Existentes:
- `DS.catalogs.getErrorsForType(contactType)` - Array de errores
- `DS.catalogs.getContactLabel(type)` - Label amigable
- `DS.catalogs.getErrorLabel(errorCode)` - Label de error

#### Métodos Nuevos ✨:

```javascript
// Obtener metadata completa de un tipo de contacto
const metadata = DS.catalogs.getContactMetadata('EMAIL');
// {code, icon, iconType, label, order, shouldTruncate}

// Obtener solo el ícono
const icon = DS.catalogs.getContactIcon('CEL'); // 'smartphone'

// Obtener tipo de ícono (phone|email|location|link)
const iconType = DS.catalogs.getContactIconType('CEL'); // 'phone'

// Verificar si debe truncarse
const truncate = DS.catalogs.shouldTruncate('EMAIL'); // true

// Obtener metadata de estado
const stateMetadata = DS.catalogs.getStateMetadata('Verificado');
// {code, bgClass, borderClass, textClass, state}

// Obtener solo las clases CSS de un estado
const classes = DS.catalogs.getStateClasses('Verificado');
// {bg: 'tw-bg-verified-bg', border: '...', text: '...', state: 'verified'}
```

## 🔍 Cómo Buscar Código a Migrar

### En JavaScript:
```bash
grep -r "EMAIL.*mail\|CEL.*smartphone" wwwroot/js/
grep -r "const.*icons.*=.*{" wwwroot/js/
grep -r "CONTACT_LABELS\|ERROR_CATALOG" wwwroot/js/
```

### En C# PageModels:
```bash
grep -r "ContactTypeRegistry\|StateRegistry" Pages/
grep -r "Dictionary.*ContactType.*Icon" Pages/
grep -r "Dictionary.*State.*Bg" Pages/
```

## ⚠️ Notas Importantes

1. **Backend Centralizado:** `ContactabilityLabelHelper` es la fuente única de verdad
2. **Enums de Dominio:** Usa `EnumContactabilityType.Email.GetValueCatalog()` para códigos
3. **Timing:** `DS.catalogs.init()` se ejecuta automáticamente al cargar la página
4. **Fallback:** Si los catálogos no cargan, devuelven valores por defecto seguros
5. **Cache:** Los endpoints tienen cache de 1 hora (datos estáticos)

## 🧪 Testing

### Backend:
```bash
curl http://localhost:5000/api/catalogos/contact-types
curl http://localhost:5000/api/catalogos/contact-states
curl http://localhost:5000/api/catalogos/error-codes
```

### Frontend:
```javascript
// En consola del navegador:
console.log(DS.catalogs.isLoaded); // true
console.log(DS.catalogs.contactTypes); // Array completo
console.log(DS.catalogs.getContactMetadata('EMAIL')); // Metadata específica
```

## 📚 Referencias
- Backend Helper: `ContactabilityLabelHelper.cs` (✨ Extendido)
- Backend Enum: `EnumErrorCodeContact.cs` + `EnumContactabilityType.cs`
- API Endpoint: `CatalogosController.cs` (endpoints agregados)
- Frontend Module: `ds-catalogs.js` (✨ API extendida)
