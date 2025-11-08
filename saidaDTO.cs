// DTOs/SaidaDto.cs
using System.ComponentModel.DataAnnotations;

namespace SistemaEstacionamento.DTOs
{
    /// <summary>
    /// DTO para registrar saída de veículo.
    /// </summary>
    public class SaidaDto
    {
        [Required]
        public string Placa { get; set; } = null!;
    }
}
