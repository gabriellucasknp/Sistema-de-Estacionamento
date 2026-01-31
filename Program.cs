using SistemaEstacionamento.Models;
using Microsoft.Extensions.Configuration;

Console.OutputEncoding = System.Text.Encoding.UTF8;

// Carrega configurações do appsettings.json
var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
    .Build();

// Cria instância do estacionamento com configurações
var capacidade = configuration.GetValue<int>("Estacionamento:Capacidade", 20);
var tarifaPorHora = configuration.GetValue<decimal>("Estacionamento:TarifaPorHora", 5.0m);

Estacionamento estacionamento = new(capacidade, tarifaPorHora);

// Carrega dados salvos anteriormente
estacionamento.CarregarDados();

Console.WriteLine("✅ Sistema inicializado!");
Console.WriteLine($"📊 Capacidade: {capacidade} vagas | Tarifa: R$ {tarifaPorHora:F2}/hora\n");

while (true)
{
    Console.WriteLine("--- SISTEMA DE ESTACIONAMENTO ---");
    Console.WriteLine("1 - Registrar entrada 🚗");
    Console.WriteLine("2 - Registrar saída 🚙");
    Console.WriteLine("3 - Ver vagas disponíveis 📊");
    Console.WriteLine("4 - Listar veículos 📋");
    Console.WriteLine("0 - Sair 🚪");
    Console.Write("Escolha: ");

    string? opcao = Console.ReadLine();

    try
    {
        switch (opcao)
        {
            case "1":
                RegistrarEntrada(estacionamento);
                break;

            case "2":
                RegistrarSaida(estacionamento);
                break;

            case "3":
                ExibirVagas(estacionamento);
                break;

            case "4":
                ListarVeiculos(estacionamento);
                break;

            case "0":
                Console.WriteLine("\n💾 Salvando dados...");
                estacionamento.SalvarDados();
                Console.WriteLine("✅ Dados salvos com sucesso!");
                Console.WriteLine("👋 Encerrando sistema...");
                return;

            default:
                Console.WriteLine("❌ Opção inválida. Tente novamente.\n");
                break;
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"\n❌ Erro: {ex.Message}\n");
    }
}

// Métodos auxiliares para organização do código
static void RegistrarEntrada(Estacionamento estacionamento)
{
    Console.WriteLine("\n--- REGISTRAR ENTRADA ---");
    Console.Write("Placa: ");
    string? placaEntrada = Console.ReadLine();

    Console.Write("Modelo: ");
    string? modelo = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(placaEntrada))
    {
        Console.WriteLine("❌ Placa é obrigatória!\n");
        return;
    }

    if (string.IsNullOrWhiteSpace(modelo))
    {
        Console.WriteLine("❌ Modelo é obrigatório!\n");
        return;
    }

    if (estacionamento.VagasDisponiveis() <= 0)
    {
        Console.WriteLine("❌ Não há vagas disponíveis!\n");
        return;
    }

    try
    {
        estacionamento.RegistrarEntrada(new Veiculo
        {
            Placa = placaEntrada.ToUpper().Trim(),
            Modelo = modelo.Trim(),
            EntradaUtc = DateTime.UtcNow
        });

        Console.WriteLine("✅ Veículo registrado com sucesso!\n");
    }
    catch (InvalidOperationException ex)
    {
        Console.WriteLine($"❌ {ex.Message}\n");
    }
}

static void RegistrarSaida(Estacionamento estacionamento)
{
    Console.WriteLine("\n--- REGISTRAR SAÍDA ---");
    Console.Write("Placa: ");
    string? placaSaida = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(placaSaida))
    {
        Console.WriteLine("❌ Placa é obrigatória!\n");
        return;
    }

    var resultado = estacionamento.RegistrarSaida(placaSaida.ToUpper().Trim());

    if (resultado.sucesso)
    {
        Console.WriteLine("✅ Saída registrada com sucesso!");
        Console.WriteLine($"⏱️  Tempo: {resultado.horas}h {resultado.minutos}min");
        Console.WriteLine($"💰 Valor: R$ {resultado.valorPago:F2}\n");
    }
    else
    {
        Console.WriteLine("❌ Veículo não encontrado ou já teve saída registrada!\n");
    }
}

static void ExibirVagas(Estacionamento estacionamento)
{
    Console.WriteLine("\n--- STATUS DAS VAGAS ---");
    int vagasLivres = estacionamento.VagasDisponiveis();
    int vagasOcupadas = estacionamento.TotalVagas - vagasLivres;
    decimal taxaOcupacao = estacionamento.TotalVagas > 0 
        ? (decimal)vagasOcupadas / estacionamento.TotalVagas * 100 
        : 0;

    Console.WriteLine($"📌 Vagas disponíveis: {vagasLivres}/{estacionamento.TotalVagas}");
    Console.WriteLine($"🚗 Vagas ocupadas: {vagasOcupadas}");
    Console.WriteLine($"📊 Taxa de ocupação: {taxaOcupacao:F1}%\n");
}

static void ListarVeiculos(Estacionamento estacionamento)
{
    var veiculosEstacionados = estacionamento.Veiculos
        .Where(v => v.SaidaUtc == null)
        .OrderBy(v => v.EntradaUtc)
        .ToList();
    
    var veiculosHistorico = estacionamento.Veiculos
        .Where(v => v.SaidaUtc != null)
        .OrderByDescending(v => v.SaidaUtc)
        .Take(10)
        .ToList();

    Console.WriteLine("\n--- VEÍCULOS ---");

    if (veiculosEstacionados.Any())
    {
        Console.WriteLine("\n🚗 VEÍCULOS ESTACIONADOS:");
        foreach (var v in veiculosEstacionados)
        {
            var tempo = DateTime.UtcNow - v.EntradaUtc;
            var entradaLocal = v.EntradaUtc.ToLocalTime();
            Console.WriteLine($"  {v.Placa,-10} | {v.Modelo,-20} | Entrada: {entradaLocal:dd/MM/yyyy HH:mm} | Tempo: {tempo.Hours}h {tempo.Minutes}min");
        }
    }
    else
    {
        Console.WriteLine("\n✨ Nenhum veículo estacionado no momento.");
    }

    if (veiculosHistorico.Any())
    {
        Console.WriteLine("\n📋 HISTÓRICO (últimos 10):");
        foreach (var v in veiculosHistorico)
        {
            var saidaLocal = v.SaidaUtc?.ToLocalTime();
            var statusPago = v.Pago ? "✅" : "❌";
            Console.WriteLine($"  {v.Placa,-10} | {v.Modelo,-20} | Saída: {saidaLocal:dd/MM/yyyy HH:mm} | Valor: R$ {v.ValorPago:F2} {statusPago}");
        }
    }

    Console.WriteLine();
}
