using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Domain.Entities;

/// <summary>
/// Representa uma agregação de dados de leituras (hora, dia, semana, mês)
/// </summary>
public class AgregacaoDados
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// ID do sensor
    /// </summary>
    public Guid SensorId { get; private set; }
    
    /// <summary>
    /// Device ID do sensor
    /// </summary>
    public string DeviceId { get; private set; }
    
    /// <summary>
    /// ID da propriedade
    /// </summary>
    public Guid PropriedadeId { get; private set; }
    
    /// <summary>
    /// ID do talhão (opcional)
    /// </summary>
    public Guid? TalhaoId { get; private set; }
    
    /// <summary>
    /// Tipo do sensor
    /// </summary>
    public TipoSensor TipoSensor { get; private set; }
    
    /// <summary>
    /// Tipo de agregação (horária, diária, etc)
    /// </summary>
    public TipoAgregacao TipoAgregacao { get; private set; }
    
    /// <summary>
    /// Período de inicio da agregação
    /// </summary>
    public DateTime PeriodoInicio { get; private set; }
    
    /// <summary>
    /// Período de fim da agregação
    /// </summary>
    public DateTime PeriodoFim { get; private set; }
    
    /// <summary>
    /// Quantidade de leituras agregadas
    /// </summary>
    public int TotalLeituras { get; private set; }
    
    /// <summary>
    /// Valor mínimo no período
    /// </summary>
    public decimal? ValorMinimo { get; private set; }
    
    /// <summary>
    /// Valor máximo no período
    /// </summary>
    public decimal? ValorMaximo { get; private set; }
    
    /// <summary>
    /// Valor médio no período
    /// </summary>
    public decimal? ValorMedio { get; private set; }
    
    /// <summary>
    /// Desvio padrão
    /// </summary>
    public decimal? DesvioPadrao { get; private set; }
    
    /// <summary>
    /// Unidade de medida
    /// </summary>
    public string Unidade { get; private set; }
    
    /// <summary>
    /// Leituras normais
    /// </summary>
    public int LeiturasNormais { get; private set; }
    
    /// <summary>
    /// Leituras suspeitas
    /// </summary>
    public int LeiturasSuspeitas { get; private set; }
    
    /// <summary>
    /// Leituras inválidas
    /// </summary>
    public int LeiturasInvalidas { get; private set; }
    
    public DateTime DataCriacao { get; private set; }
    
    // EF Core
    private AgregacaoDados() 
    { 
        DeviceId = string.Empty;
        Unidade = string.Empty;
    }
    
    public AgregacaoDados(
        Guid sensorId,
        string deviceId,
        Guid propriedadeId,
        TipoSensor tipoSensor,
        TipoAgregacao tipoAgregacao,
        DateTime periodoInicio,
        DateTime periodoFim,
        int totalLeituras,
        string unidade,
        Guid? talhaoId = null,
        decimal? valorMinimo = null,
        decimal? valorMaximo = null,
        decimal? valorMedio = null,
        decimal? desvioPadrao = null,
        int leiturasNormais = 0,
        int leiturasSuspeitas = 0,
        int leiturasInvalidas = 0)
    {
        ValidarInvariantes(deviceId, unidade, periodoInicio, periodoFim);
        
        Id = Guid.NewGuid();
        SensorId = sensorId;
        DeviceId = deviceId.ToUpperInvariant();
        PropriedadeId = propriedadeId;
        TalhaoId = talhaoId;
        TipoSensor = tipoSensor;
        TipoAgregacao = tipoAgregacao;
        PeriodoInicio = periodoInicio;
        PeriodoFim = periodoFim;
        TotalLeituras = totalLeituras;
        Unidade = unidade;
        ValorMinimo = valorMinimo;
        ValorMaximo = valorMaximo;
        ValorMedio = valorMedio;
        DesvioPadrao = desvioPadrao;
        LeiturasNormais = leiturasNormais;
        LeiturasSuspeitas = leiturasSuspeitas;
        LeiturasInvalidas = leiturasInvalidas;
        DataCriacao = DateTime.UtcNow;
    }
    
    // Validações de invariantes
    private static void ValidarInvariantes(
        string deviceId, 
        string unidade, 
        DateTime periodoInicio, 
        DateTime periodoFim)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID não pode ser vazio", nameof(deviceId));
        
        if (string.IsNullOrWhiteSpace(unidade))
            throw new ArgumentException("Unidade não pode ser vazia", nameof(unidade));
        
        if (periodoFim <= periodoInicio)
            throw new ArgumentException("Período fim deve ser posterior ao início");
    }
}
