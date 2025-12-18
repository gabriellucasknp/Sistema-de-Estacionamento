using System.Text.Json;
using SistemaEstacionamento.Models;

namespace SistemaEstacionamento.Repositories
{
    public class EstacionamentoRepositorio
    {
        private readonly string _caminhoArquivo = "dados_estacionamento.json";

        // Salva todos os dados do estacionamento
        public void Salvar(Estacionamento estacionamento)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };

                string json = JsonSerializer.Serialize(estacionamento, options);
                File.WriteAllText(_caminhoArquivo, json);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao salvar dados: {ex.Message}");
            }
        }

        // Carrega dados salvos anteriormente
        public Estacionamento Carregar()
        {
            try
            {
                if (!File.Exists(_caminhoArquivo))
                    return new Estacionamento();

                string json = File.ReadAllText(_caminhoArquivo);

                var estacionamento = JsonSerializer.Deserialize<Estacionamento>(json);

                return estacionamento ?? new Estacionamento();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erro ao carregar dados: {ex.Message}");
                return new Estacionamento();
            }
        }
    }
}
