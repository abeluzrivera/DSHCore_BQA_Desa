namespace SDH.Infrastructure.Persistence.ReadModels.Parametro
{
    /// <summary>
    /// Modelo exclusivo de Infraestructura para mapear la vista Vw_Cat_Detalle_General.
    /// No tiene reglas de negocio, es solo un contenedor de datos SQL.
    /// </summary>
    public class VwCatalogoDetalle
    {
        public int IdItemCatalogo { get; set; }
        public int IdGrupoCatalogo { get; set; }
        public string NombreGrupo { get; set; } = string.Empty;
        public string CodigoValor { get; set; } = string.Empty;
        public string TextoVisual { get; set; } = string.Empty;
        public int OrdenVisual { get; set; }
        public bool EstaActivo { get; set; }
        public bool EsSistema { get; set; }
    }
}