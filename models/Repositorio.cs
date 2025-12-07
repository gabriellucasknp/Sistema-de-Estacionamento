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
            sw.WriteLine("Placa,Modelo,Entrada,Saida,ValorPago"); // Cabeçalho
            
            foreach (var v in veiculos)
            {
                string saida = v.Saida?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture) ?? "";
                sw.WriteLine($"{v.Placa},{v.Modelo},{v.Entrada:yyyy-MM-dd HH:mm:ss},{saida},{v.ValorPago.ToString(CultureInfo.InvariantCulture)}");
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
                if (primeiraLinha && linha.StartsWith("Placa,"))
                {
                    primeiraLinha = false;
                    continue;
                }

                if (string.IsNullOrWhiteSpace(linha))
                    continue;

                var partes = linha.Split(',');
                if (partes.Length >= 5)
                {
                    lista.Add(new Veiculo
                    {
                        Placa = partes[0],
                        Modelo = partes[1],
                        Entrada = DateTime.Parse(partes[2], CultureInfo.InvariantCulture),
                        Saida = string.IsNullOrEmpty(partes[3]) ? null : DateTime.Parse(partes[3], CultureInfo.InvariantCulture),
                        ValorPago = double.Parse(partes[4], CultureInfo.InvariantCulture)
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
