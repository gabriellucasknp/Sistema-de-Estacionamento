// DTOs/VeiculoViewDto.cs
using System;

namespace SistemaEstacionamento.DTOs
{
    /// <summary>
    /// DTO usado para retornar informações de veículos pela API.
    /// </summary>
    public class VeiculoViewDto
    {
        public string Placa { get; init; } = null!;
        public string Modelo { get; init; } = string.Empty;
        public DateTime EntradaUtc { get; init; }
        public DateTime? SaidaUtc { get; init; }
        public decimal ValorPago { get; init; }
        public bool Pago { get; init; }
    }
}
