// Models/Veiculo.cs
using System;

namespace SistemaEstacionamento. Models
{
    /// <summary>
    /// Entidade que representa um veículo no sistema de estacionamento. 
    /// </summary>
    public class Veiculo
    {
        // Identificador interno (poderia ser GUID ou autonumérico em banco)
        public Guid Id { get; set; } = Guid. NewGuid();

        // Placa única do veículo (chave de negócio)
        public string Placa { get; set; } = null!;

        // Modelo ou descrição do veículo
        public string Modelo { get; set; } = string.Empty;

        // Data/hora de entrada (UTC)
        public DateTime EntradaUtc { get; set; }

        // Data/hora de saída (UTC). Null enquanto estiver dentro do estacionamento. 
        public DateTime? SaidaUtc { get; set; }

        // Valor cobrado na saída
        public decimal ValorPago { get; set; }

        // Indica se já foi cobrado/pago
        public bool Pago { get; set; } = false;
    }
}
