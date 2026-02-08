namespace AgroSolutions.IngestaoDados.Domain.Enums;

/// <summary>
/// Tipos de sensores IoT suportados
/// </summary>
public enum TipoSensor
{
    /// <summary>
    /// Sensor de temperatura do ar
    /// </summary>
    Temperatura = 1,
    
    /// <summary>
    /// Sensor de umidade do ar
    /// </summary>
    UmidadeAr = 2,
    
    /// <summary>
    /// Sensor de umidade do solo
    /// </summary>
    UmidadeSolo = 3,
    
    /// <summary>
    /// Pluviômetro - medidor de precipitação
    /// </summary>
    Precipitacao = 4,
    
    /// <summary>
    /// Sensor de velocidade do vento
    /// </summary>
    VelocidadeVento = 5,
    
    /// <summary>
    /// Sensor de direção do vento
    /// </summary>
    DirecaoVento = 6,
    
    /// <summary>
    /// Sensor de pressão atmosférica
    /// </summary>
    PressaoAtmosferica = 7,
    
    /// <summary>
    /// Sensor de pH do solo
    /// </summary>
    PHSolo = 8,
    
    /// <summary>
    /// Sensor de condutividade elétrica do solo
    /// </summary>
    CondutividadeSolo = 9,
    
    /// <summary>
    /// Sensor de radiação solar
    /// </summary>
    RadiacaoSolar = 10
}

/// <summary>
/// Status operacional do sensor
/// </summary>
public enum StatusSensor
{
    /// <summary>
    /// Sensor ativo e operacional
    /// </summary>
    Ativo = 1,
    
    /// <summary>
    /// Sensor temporariamente inativo
    /// </summary>
    Inativo = 2,
    
    /// <summary>
    /// Sensor em manutenção
    /// </summary>
    EmManutencao = 3,
    
    /// <summary>
    /// Sensor com defeito
    /// </summary>
    Defeituoso = 4,
    
    /// <summary>
    /// Sensor aguardando calibração
    /// </summary>
    AguardandoCalibracao = 5
}

/// <summary>
/// Status de qualidade da leitura do sensor
/// </summary>
public enum QualidadeLeitura
{
    /// <summary>
    /// Leitura dentro dos parâmetros esperados
    /// </summary>
    Normal = 1,
    
    /// <summary>
    /// Leitura suspeita (fora dos padrões habituais)
    /// </summary>
    Suspeita = 2,
    
    /// <summary>
    /// Leitura inválida (erro de sensor)
    /// </summary>
    Invalida = 3,
    
    /// <summary>
    /// Leitura calibrada/ajustada
    /// </summary>
    Calibrada = 4
}

