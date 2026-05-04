namespace SDH.Domain.Entities.Operative
{
    public class ClientFinancial
    {
        public long Id { get; private set; }

        public long IdCliente { get; private set; }

        public string ClientId { get; private set; } = string.Empty;
        public decimal AccountingAmount { get; private set; }

        // Constructor privado para EF Core
        private ClientFinancial() { }

        /// <summary>
        /// Factory method para registrar información financiera.
        /// </summary>
        public static ClientFinancial Create(string tipoContabilidad, decimal montoContable)
        {
            return new ClientFinancial
            {
                ClientId = tipoContabilidad,
                AccountingAmount = montoContable
            };
        }

        public void UpdateAmount(decimal nuevoMonto)
        {
            AccountingAmount = nuevoMonto;
        }

        public void UpdateType(string nuevoTipo)
        {
            ClientId = nuevoTipo;
        }
    }
}
