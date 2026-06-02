using SDH.Domain.Attributes;

namespace SDH.Domain.Enums
{
    /// <summary>
    /// Códigos canónicos para el catálogo TIPO_CONTABILIDAD.
    /// Clasificación contable estándar para análisis financiero.
    /// </summary>
    [MappedCatalog(CatalogGroups.AccountingType)]
    public static class TiposContabilidad
    {
        public const string Activo = "ACT";
        public const string Pasivo = "PAS";
        public const string Patrimonio = "PAT";
        public const string ActivoFijo = "ACT_F";
        public const string CuentasOrden = "ORD";
        public const string Ingresos = "ING";
        public const string Gastos = "GAS";

        /// <summary>
        /// Determina si el tipo pertenece al balance general (Ecuación contable).
        /// </summary>
        public static bool EsBalance(string? codigo) =>
            codigo == Activo || codigo == Pasivo || codigo == Patrimonio;
    }
}