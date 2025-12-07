// DTOs/EntradaDto.cs
using System.ComponentModel.DataAnnotations;

namespace SistemaEstacionamento.DTOs
{
    /// <summary>
    /// DTO para registro de entrada.
    /// </summary>
    public class EntradaDto
    {
        [Required]
        [RegularExpression(@"^[A-Z0-9\-]+$", ErrorMessage = "Placa inválida. Use apenas letras, números e traço.")]
        public string Placa { get; set; } = null!;

        [Required]
        public string Modelo { get; set; } = null!;
    }
}
