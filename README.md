# 🚗 Sistema de Estacionamento

Sistema completo de gerenciamento de estacionamento desenvolvido em C# .NET 9.0.

## ✨ Funcionalidades

- ✅ Registrar entrada de veículos
- ✅ Registrar saída de veículos com cálculo automático de valor
- ✅ Visualizar vagas disponíveis
- ✅ Listar veículos estacionados e histórico
- ✅ Persistência de dados em CSV
- ✅ Validação de placas duplicadas
- ✅ Interface interativa no console

## 🛠️ Tecnologias

- C# .NET 9.0
- File System (persistência em CSV)

## 📋 Pré-requisitos

- .NET 9.0 SDK ou superior

## 🚀 Como executar

```bash
# Clone o repositório
git clone <seu-repositorio>

# Entre na pasta do projeto
cd Sistema_Estacionamento

# Execute o projeto
dotnet run
```

## 💡 Como usar

1. **Registrar entrada**: Digite a placa e modelo do veículo
2. **Registrar saída**: Digite a placa para calcular o valor e registrar saída
3. **Ver vagas**: Visualize quantas vagas estão disponíveis
4. **Listar veículos**: Veja os veículos estacionados e histórico
5. **Sair**: Salva os dados e encerra o sistema

## 💰 Tabela de preços

- **R$ 5,00** por hora (arredondado para cima)
- **20 vagas** disponíveis

## 📁 Estrutura do projeto

```
Sistema_Estacionamento/
├── models/
│   ├── Estacionamento.cs    # Lógica principal do estacionamento
│   ├── Veiculo.cs            # Modelo de dados do veículo
│   └── Repositorio.cs        # Persistência de dados
├── Program.cs                # Ponto de entrada da aplicação
├── dados.csv                 # Arquivo de dados (gerado automaticamente)
└── README.md                 # Documentação
```

## 🧪 Testes

O sistema foi otimizado para:
- ✅ Tratamento de null safety
- ✅ Validação de entrada de dados
- ✅ Persistência confiável
- ✅ Formatação consistente de datas e valores
- ✅ Encoding UTF-8 para caracteres especiais

## 📝 Licença

Este projeto está sob a licença MIT.
