namespace SDH.Domain.Entities.Operative
{
    public class CoreOfficializations
    {
        public long Id { get; private set; }

        public long ClientId { get; private set; }

        public string SentJsonPayload { get; private set; } = string.Empty;
        public string CoreResponseCode { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = string.Empty;

        // Constructor privado para EF Core
        private CoreOfficializations() { }

        /// <summary>
        /// Factory method para registrar una nueva oficialización.
        /// Al ser un registro histórico, no tiene métodos de actualización.
        /// </summary>
        public static CoreOfficializations Create(
            string tramaJsonEnviada,
            string respuestaCoreCodigo,
            string usuarioCreacion)
        {
            return new CoreOfficializations
            {
                SentJsonPayload = tramaJsonEnviada,
                CoreResponseCode = respuestaCoreCodigo,
                CreatedAt = DateTime.Now, // Siempre Now para registros de tiempo exactos
                CreatedBy = usuarioCreacion
            };
        }
    }
}
