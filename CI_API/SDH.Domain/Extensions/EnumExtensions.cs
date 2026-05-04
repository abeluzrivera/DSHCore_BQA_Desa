using SDH.Domain.Attributes;
using System.Reflection;

namespace SDH.Domain.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Obtiene el valor string del catálogo mapeado en el Enum.
        /// Ejemplo: EnumErrorCodeContact.EmailFormato.GetValueCatalog() -> "ERR_MAIL_FMT"
        /// </summary>
        public static string GetValueCatalog(this Enum value)
        {
            FieldInfo? field = value.GetType().GetField(value.ToString());

            if (field == null) return value.ToString();

            var attribute = (MappedCatalog?)Attribute.GetCustomAttribute(field, typeof(MappedCatalog));

            return attribute?.Value ?? value.ToString();
        }

        /// <summary>
        /// Convierte un string del catálogo (BD) a su respectivo Enum basado en el atributo [MappedCatalog].
        /// </summary>
        public static TEnum? GetEnumFromStringCatalog<TEnum>(this string valorCatalogo) where TEnum : struct, Enum
        {
            // Usamos la clase anidada estática para cachear el resultado por cada tipo de Enum.
            // Esto asegura que la reflexión solo se ejecute UNA vez por ciclo de vida de la aplicación.
            return EnumCache<TEnum>.GetEnum(valorCatalogo);
        }

        public static TEnum? GetEnumFromIntCatalog<TEnum>(this int valorCatalogo) where TEnum : struct, Enum
        {
            // Usamos la clase anidada estática para cachear el resultado por cada tipo de Enum.
            // Esto asegura que la reflexión solo se ejecute UNA vez por ciclo de vida de la aplicación.
            return EnumCache<TEnum>.GetEnum(valorCatalogo);
        }


        // Clase auxiliar genérica para cachear la reflexión
        private static class EnumCache<TEnum> where TEnum : struct, Enum
        {
            // Índices de búsqueda separados para máximo rendimiento O(1)
            private static readonly Dictionary<string, TEnum> _mapaStringAEnum = new();
            private static readonly Dictionary<int, TEnum> _mapaIntAEnum = new();

            static EnumCache()
            {
                var tipoEnum = typeof(TEnum);
                var campos = tipoEnum.GetFields(BindingFlags.Public | BindingFlags.Static);

                foreach (var campo in campos)
                {
                    // Obtenemos el valor real del Enum (Ej: EnumContactabilityStatus.Verified)
                    var enumValue = (TEnum)campo.GetValue(null)!;

                    // 1. Poblamos el caché numérico (Int -> Enum)
                    int valorInt = Convert.ToInt32(enumValue);
                    _mapaIntAEnum[valorInt] = enumValue;

                    // 2. Poblamos el caché de texto (String -> Enum) si tiene el atributo
                    var atributo = campo.GetCustomAttribute<MappedCatalog>();
                    if (atributo != null)
                    {
                        // Key = "VERIF", Value = Verified
                        _mapaStringAEnum[atributo.Value] = enumValue;
                    }
                }
            }

            public static TEnum? GetEnum(string valorCatalogo)
            {
                if (string.IsNullOrWhiteSpace(valorCatalogo)) return null;

                return _mapaStringAEnum.TryGetValue(valorCatalogo, out var resultado)
                    ? resultado
                    : null;
            }

            public static TEnum? GetEnum(int valorCatalogo)
            {
                // Regla de Negocio: Si en tu base de datos NUNCA existe un catálogo con ID 0, esto es correcto.
                // Si algún día creas un enum con valor 0 (ej. Unknown = 0), debes quitar esta validación.
                if (valorCatalogo == 0) return null;

                return _mapaIntAEnum.TryGetValue(valorCatalogo, out var resultado)
                    ? resultado
                    : null;
            }
        }
    }


}