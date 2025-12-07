namespace SistemaEstacionamento.Models;

public class Estacionamento
{
    // Lista de veículos registrados no estacionamento
    public List<Veiculo> Veiculos { get; private set; } = new();

    // Número total de vagas disponíveis
    public int TotalVagas { get; private set; } = 20;

    // Valor da hora de estacionamento
    private const double PrecoHora = 5.0;

    /// <summary>
    /// Retorna o número de vagas livres no momento.
    /// </summary>
    public int VagasDisponiveis()
    {
        return TotalVagas - Veiculos.Count(v => v.Saida == null);
    }

    /// <summary>
    /// Registra a entrada de um novo veículo.
    /// </summary>
    public void RegistrarEntrada(Veiculo veiculo)
    {
        // Verifica se o veículo já está estacionado
        var veiculoExistente = Veiculos.FirstOrDefault(v => v.Placa == veiculo.Placa && v.Saida == null);
        if (veiculoExistente != null)
        {
            throw new InvalidOperationException($"Veículo com placa {veiculo.Placa} já está estacionado!");
        }

        Veiculos.Add(veiculo);
        SalvarDados();
    }

    /// <summary>
    /// Registra a saída de um veículo com base na placa.
    /// Calcula o valor a ser pago.
    /// </summary>
    public bool RegistrarSaida(string placa)
    {
        var veiculo = Veiculos.FirstOrDefault(v => v.Placa == placa && v.Saida == null);

        if (veiculo == null)
            return false; // Não encontrou veículo com essa placa

        veiculo.Saida = DateTime.Now;

        var horas = (veiculo.Saida.Value - veiculo.Entrada).TotalHours;
        veiculo.ValorPago = Math.Ceiling(horas) * PrecoHora;

        SalvarDados();
        return true;
    }

    /// <summary>
    /// Salva os dados dos veículos no arquivo CSV.
    /// </summary>
    public void SalvarDados()
    {
        Repositorio.Salvar(Veiculos);
    }

    /// <summary>
    /// Carrega os dados dos veículos do arquivo CSV.
    /// </summary>
    public void CarregarDados()
    {
        Veiculos = Repositorio.Carregar();
    }
}
