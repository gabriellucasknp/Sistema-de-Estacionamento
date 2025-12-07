// Options/EstacionamentoOptions.cs
namespace SistemaEstacionamento.Options
{
    /// <summary>
    /// Configurações do estacionamento (capacidade, tarifa horária).
    /// </summary>
    public class EstacionamentoOptions
    {
        // Número máximo de vagas simultâneas
        public int Capacidade { get; set; } = 50;

        // Tarifa por hora (valor monetário)
        public decimal TarifaPorHora { get; set; } = 5.00m;
    }
}
