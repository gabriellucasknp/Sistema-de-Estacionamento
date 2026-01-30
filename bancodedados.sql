
-- ============================================
-- BANCO DE DADOS - SISTEMA DE ESTACIONAMENTO
-- ============================================

-- Criação do banco de dados
CREATE DATABASE IF NOT EXISTS SistemaEstacionamento
CHARACTER SET utf8mb4
COLLATE utf8mb4_unicode_ci;

USE SistemaEstacionamento;

-- ============================================
-- TABELA: Veiculos
-- Armazena os dados de todos os veículos que utilizaram o estacionamento
-- ============================================
CREATE TABLE IF NOT EXISTS Veiculos (
    Id CHAR(36) PRIMARY KEY DEFAULT (UUID()),
    Placa VARCHAR(20) NOT NULL,
    Modelo VARCHAR(100) NOT NULL,
    EntradaUtc DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    SaidaUtc DATETIME NULL,
    ValorPago DECIMAL(10, 2) DEFAULT 0.00,
    Pago BOOLEAN DEFAULT FALSE,
    
    -- Índices para melhorar performance
    INDEX idx_placa (Placa),
    INDEX idx_entrada (EntradaUtc),
    INDEX idx_saida (SaidaUtc),
    INDEX idx_veiculos_ativos (Placa, SaidaUtc)
) ENGINE=InnoDB;

-- ============================================
-- TABELA: ConfiguracaoEstacionamento
-- Armazena as configurações globais do estacionamento
-- ============================================
CREATE TABLE IF NOT EXISTS ConfiguracaoEstacionamento (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    TotalVagas INT NOT NULL DEFAULT 20,
    PrecoHora DECIMAL(10, 2) NOT NULL DEFAULT 5.00,
    PrecoInicial DECIMAL(10, 2) DEFAULT 0.00,
    DataAtualizacao DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB;

-- Insere configuração padrão
INSERT INTO ConfiguracaoEstacionamento (TotalVagas, PrecoHora, PrecoInicial)
VALUES (20, 5.00, 0.00);

-- ============================================
-- TABELA: HistoricoPrecos
-- Mantém histórico de alterações nos preços
-- ============================================
CREATE TABLE IF NOT EXISTS HistoricoPrecos (
    Id INT PRIMARY KEY AUTO_INCREMENT,
    PrecoHoraAnterior DECIMAL(10, 2),
    PrecoHoraNovo DECIMAL(10, 2),
    DataAlteracao DATETIME DEFAULT CURRENT_TIMESTAMP,
    Usuario VARCHAR(100),
    
    INDEX idx_data_alteracao (DataAlteracao)
) ENGINE=InnoDB;

-- ============================================
-- VIEWS ÚTEIS
-- ============================================

-- VIEW: Veículos atualmente estacionados
CREATE OR REPLACE VIEW VeiculosEstacionados AS
SELECT 
    Id,
    Placa,
    Modelo,
    EntradaUtc,
    TIMESTAMPDIFF(MINUTE, EntradaUtc, NOW()) AS MinutosEstacionado,
    CEIL(TIMESTAMPDIFF(MINUTE, EntradaUtc, NOW()) / 60.0) AS HorasCobradas,
    CEIL(TIMESTAMPDIFF(MINUTE, EntradaUtc, NOW()) / 60.0) * 
        (SELECT PrecoHora FROM ConfiguracaoEstacionamento LIMIT 1) AS ValorEstimado
FROM Veiculos
WHERE SaidaUtc IS NULL
ORDER BY EntradaUtc;

-- VIEW: Relatório de veículos que já saíram
CREATE OR REPLACE VIEW HistoricoSaidas AS
SELECT 
    Id,
    Placa,
    Modelo,
    EntradaUtc,
    SaidaUtc,
    TIMESTAMPDIFF(MINUTE, EntradaUtc, SaidaUtc) AS MinutosEstacionado,
    ValorPago,
    Pago
FROM Veiculos
WHERE SaidaUtc IS NOT NULL
ORDER BY SaidaUtc DESC;

-- VIEW: Estatísticas do dia
CREATE OR REPLACE VIEW EstatisticasDia AS
SELECT 
    DATE(EntradaUtc) AS Data,
    COUNT(*) AS TotalVeiculos,
    COUNT(CASE WHEN SaidaUtc IS NOT NULL THEN 1 END) AS VeiculosSairam,
    COUNT(CASE WHEN SaidaUtc IS NULL THEN 1 END) AS VeiculosEstacionados,
    COALESCE(SUM(CASE WHEN Pago = TRUE THEN ValorPago END), 0) AS ReceitaTotal,
    COALESCE(AVG(CASE WHEN SaidaUtc IS NOT NULL THEN 
        TIMESTAMPDIFF(MINUTE, EntradaUtc, SaidaUtc) END), 0) AS MediaMinutosEstacionado
FROM Veiculos
GROUP BY DATE(EntradaUtc)
ORDER BY Data DESC;

-- ============================================
-- STORED PROCEDURES
-- ============================================

-- Procedure: Registrar entrada de veículo
DELIMITER $$

CREATE PROCEDURE IF NOT EXISTS sp_RegistrarEntrada(
    IN p_Placa VARCHAR(20),
    IN p_Modelo VARCHAR(100)
)
BEGIN
    DECLARE v_VeiculoExistente INT;
    DECLARE v_VagasDisponiveis INT;
    
    -- Verifica se o veículo já está estacionado
    SELECT COUNT(*) INTO v_VeiculoExistente
    FROM Veiculos
    WHERE Placa = p_Placa AND SaidaUtc IS NULL;
    
    IF v_VeiculoExistente > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Veículo já está estacionado';
    END IF;
    
    -- Verifica se há vagas disponíveis
    SELECT (c.TotalVagas - COUNT(v.Id)) INTO v_VagasDisponiveis
    FROM ConfiguracaoEstacionamento c
    LEFT JOIN Veiculos v ON v.SaidaUtc IS NULL
    GROUP BY c.TotalVagas;
    
    IF v_VagasDisponiveis <= 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Não há vagas disponíveis';
    END IF;
    
    -- Registra a entrada
    INSERT INTO Veiculos (Placa, Modelo, EntradaUtc, Pago)
    VALUES (p_Placa, p_Modelo, NOW(), FALSE);
    
    SELECT 'Entrada registrada com sucesso!' AS Mensagem, LAST_INSERT_ID() AS VeiculoId;
END$$

-- Procedure: Registrar saída de veículo
CREATE PROCEDURE IF NOT EXISTS sp_RegistrarSaida(
    IN p_Placa VARCHAR(20)
)
BEGIN
    DECLARE v_VeiculoId CHAR(36);
    DECLARE v_EntradaUtc DATETIME;
    DECLARE v_PrecoHora DECIMAL(10, 2);
    DECLARE v_HorasCobradas DECIMAL(10, 2);
    DECLARE v_ValorTotal DECIMAL(10, 2);
    
    -- Busca o veículo estacionado
    SELECT Id, EntradaUtc INTO v_VeiculoId, v_EntradaUtc
    FROM Veiculos
    WHERE Placa = p_Placa AND SaidaUtc IS NULL
    LIMIT 1;
    
    IF v_VeiculoId IS NULL THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = 'Veículo não encontrado ou já saiu';
    END IF;
    
    -- Busca o preço por hora
    SELECT PrecoHora INTO v_PrecoHora
    FROM ConfiguracaoEstacionamento
    LIMIT 1;
    
    -- Calcula o valor a pagar
    SET v_HorasCobradas = CEIL(TIMESTAMPDIFF(MINUTE, v_EntradaUtc, NOW()) / 60.0);
    SET v_ValorTotal = v_HorasCobradas * v_PrecoHora;
    
    -- Atualiza o registro
    UPDATE Veiculos
    SET SaidaUtc = NOW(),
        ValorPago = v_ValorTotal,
        Pago = TRUE
    WHERE Id = v_VeiculoId;
    
    -- Retorna informações
    SELECT 
        'Saída registrada com sucesso!' AS Mensagem,
        v_VeiculoId AS VeiculoId,
        p_Placa AS Placa,
        v_HorasCobradas AS HorasCobradas,
        v_ValorTotal AS ValorTotal;
END$$

-- Procedure: Consultar vagas disponíveis
CREATE PROCEDURE IF NOT EXISTS sp_ConsultarVagasDisponiveis()
BEGIN
    SELECT 
        c.TotalVagas,
        COUNT(v.Id) AS VagasOcupadas,
        (c.TotalVagas - COUNT(v.Id)) AS VagasDisponiveis
    FROM ConfiguracaoEstacionamento c
    LEFT JOIN Veiculos v ON v.SaidaUtc IS NULL
    GROUP BY c.TotalVagas;
END$$

-- Procedure: Relatório de receita por período
CREATE PROCEDURE IF NOT EXISTS sp_RelatorioReceita(
    IN p_DataInicio DATE,
    IN p_DataFim DATE
)
BEGIN
    SELECT 
        DATE(EntradaUtc) AS Data,
        COUNT(*) AS TotalVeiculos,
        SUM(ValorPago) AS ReceitaTotal,
        AVG(ValorPago) AS ValorMedio,
        MIN(ValorPago) AS ValorMinimo,
        MAX(ValorPago) AS ValorMaximo
    FROM Veiculos
    WHERE DATE(EntradaUtc) BETWEEN p_DataInicio AND p_DataFim
        AND Pago = TRUE
    GROUP BY DATE(EntradaUtc)
    ORDER BY Data;
END$$

DELIMITER ;

-- ============================================
-- TRIGGERS
-- ============================================

-- Trigger: Registrar histórico de alteração de preços
DELIMITER $$

CREATE TRIGGER IF NOT EXISTS trg_HistoricoPrecos
BEFORE UPDATE ON ConfiguracaoEstacionamento
FOR EACH ROW
BEGIN
    IF OLD.PrecoHora <> NEW.PrecoHora THEN
        INSERT INTO HistoricoPrecos (PrecoHoraAnterior, PrecoHoraNovo)
        VALUES (OLD.PrecoHora, NEW.PrecoHora);
    END IF;
END$$

DELIMITER ;

-- ============================================
-- DADOS DE EXEMPLO (OPCIONAL)
-- ============================================

-- Exemplos de entradas
INSERT INTO Veiculos (Placa, Modelo, EntradaUtc, SaidaUtc, ValorPago, Pago) VALUES
('ABC-1234', 'Fiat Uno', DATE_SUB(NOW(), INTERVAL 3 HOUR), DATE_SUB(NOW(), INTERVAL 1 HOUR), 15.00, TRUE),
('XYZ-9876', 'Honda Civic', DATE_SUB(NOW(), INTERVAL 5 HOUR), DATE_SUB(NOW(), INTERVAL 2 HOUR), 15.00, TRUE),
('DEF-5678', 'Toyota Corolla', DATE_SUB(NOW(), INTERVAL 2 HOUR), NULL, 0.00, FALSE),
('GHI-4321', 'Chevrolet Onix', DATE_SUB(NOW(), INTERVAL 1 HOUR), NULL, 0.00, FALSE);

-- ============================================
-- CONSULTAS ÚTEIS
-- ============================================

-- Listar todos os veículos estacionados
-- SELECT * FROM VeiculosEstacionados;

-- Listar histórico de saídas
-- SELECT * FROM HistoricoSaidas;

-- Consultar estatísticas do dia
-- SELECT * FROM EstatisticasDia;

-- Registrar entrada de um veículo
-- CALL sp_RegistrarEntrada('ABC-1234', 'Fiat Uno');

-- Registrar saída de um veículo
-- CALL sp_RegistrarSaida('ABC-1234');

-- Consultar vagas disponíveis
-- CALL sp_ConsultarVagasDisponiveis();

-- Relatório de receita
-- CALL sp_RelatorioReceita('2026-01-01', '2026-01-31');
