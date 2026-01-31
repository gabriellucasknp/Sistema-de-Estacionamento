using System.Globalization;

namespace SistemaEstacionamento.Models;

public static class Repositorio
{
    private const string Caminho = "dados.csv";

    /// <summary>
    /// Salva a lista de veículos no arquivo CSV.
    /// </summary>
    public static void Salvar(List<Veiculo> veiculos)
    {
        try
        {
            using var sw = new StreamWriter(Caminho, false, System.Text.Encoding.UTF8);
            sw.WriteLine("Id,Placa,Modelo,EntradaUtc,SaidaUtc,ValorPago,Pago");
            
            foreach (var v in veiculos)
            {
                string saida = v.SaidaUtc?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "";
                sw.WriteLine($"{v.Id},{v.Placa},{v.Modelo},{v.EntradaUtc:yyyy-MM-dd HH:mm:ss},{saida},{v.ValorPago.ToString(CultureInfo.InvariantCulture)},{v.Pago}");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao salvar dados: {ex.Message}");
        }
    }

    /// <summary>
    /// Carrega a lista de veículos do arquivo CSV.
    /// </summary>
    public static List<Veiculo> Carregar()
    {
        var lista = new List<Veiculo>();
        
        if (!File.Exists(Caminho))
            return lista;

        try
        {
            var linhas = File.ReadAllLines(Caminho, System.Text.Encoding.UTF8);
            
            // Pula o cabeçalho se existir
            bool primeiraLinha = true;
            foreach (var linha in linhas)
            {
                if (primeiraLinha && linha.StartsWith("Id,"))
                {
                    primeiraLinha = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                var partes = linha.Split(',');
                if (partes.Length >= 7)
                {
                    lista.Add(new Veiculo
                    {
                        Id = Guid.Parse(partes[0]),
                        Placa = partes[1],
                        Modelo = partes[2],
                        EntradaUtc = DateTime.Parse(partes[3], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                        SaidaUtc = string.IsNullOrEmpty(partes[4]) ? null : DateTime.Parse(partes[4], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                        ValorPago = decimal.Parse(partes[5], CultureInfo.InvariantCulture),
                        Pago = bool.Parse(partes[6])
                    });
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Erro ao carregar dados: {ex.Message}");
        }

        return lista;
    }
}
