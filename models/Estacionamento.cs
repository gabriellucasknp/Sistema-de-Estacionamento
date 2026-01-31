namespace SistemaEstacionamento.Models;

public class Estacionamento
{
    // Lista de veículos registrados no estacionamento
    public List<Veiculo> Veiculos { get; private set; } = new();

    // Número total de vagas disponíveis
    public int TotalVagas { get; private set; }

    // Valor da hora de estacionamento
    private readonly decimal _precoHora;

    /// <summary>
    /// Construtor que permite configurar capacidade e preço
    /// </summary>
    public Estacionamento(int totalVagas = 20, decimal precoHora = 5.0m)
    {
        TotalVagas = totalVagas;
        _precoHora = precoHora;
    }

    /// <summary>
    /// Retorna o número de vagas livres no momento.
    /// </summary>
    public int VagasDisponiveis()
    {
        return TotalVagas - Veiculos.Count(v => v.SaidaUtc == null);
    }

    /// <summary>
    /// Registra a entrada de um novo veículo.
    /// </summary>
    public void RegistrarEntrada(Veiculo veiculo)
    {
        // Verifica se o veículo já está estacionado
        var veiculoExistente = Veiculos.FirstOrDefault(v => v.Placa == veiculo.Placa && v.SaidaUtc == null);
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
    /// Retorna tupla com status e informações da operação.
    /// </summary>
    public (bool sucesso, int horas, int minutos, decimal valorPago) RegistrarSaida(string placa)
    {
        var veiculo = Veiculos.FirstOrDefault(v => v.Placa == placa && v.SaidaUtc == null);

        if (veiculo == null)
            return (false, 0, 0, 0m);

        veiculo.SaidaUtc = DateTime.UtcNow;

        var tempo = veiculo.SaidaUtc.Value - veiculo.EntradaUtc;
        var horasTotal = (decimal)Math.Ceiling(tempo.TotalHours);
        
        veiculo.ValorPago = horasTotal * _precoHora;
        veiculo.Pago = true;

        SalvarDados();
        
        return (true, tempo.Hours, tempo.Minutes, veiculo.ValorPago);
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
