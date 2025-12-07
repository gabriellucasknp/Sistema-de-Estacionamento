namespace SistemaEstacionamento.Models;

public class Veiculo
{
    public required string Placa { get; set; }
    public required string Modelo { get; set; }
    public DateTime Entrada { get; set; }
    public DateTime? Saida { get; set; }
    public double ValorPago { get; set; }
}
