using AgroSolutions.IngestaoDados.Domain.Enums;

namespace AgroSolutions.IngestaoDados.Domain.Entities;

/// <summary>
/// Representa uma leitura capturada por um sensor IoT
/// </summary>
public class LeituraSensor
{
    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    
    /// <summary>
    /// Valor da leitura
    /// </summary>
    public decimal Valor { get; private set; }
    
    /// <summary>
    /// Unidade de medida (°C, %, mm, m/s, hPa, etc.)
    /// </summary>
    public string Unidade { get; private set; }
    
    /// <summary>
    /// Timestamp da leitura no sensor (pode ser diferente do recebimento)
    /// </summary>
    public DateTime TimestampLeitura { get; private set; }
    
    /// <summary>
    /// Timestamp de recebimento no servidor
    /// </summary>
    public DateTime TimestampRecebimento { get; private set; }
    
    /// <summary>
    /// Qualidade/confiabilidade da leitura
    /// </summary>
    public QualidadeLeitura Qualidade { get; private set; }
    
    /// <summary>
    /// Nível de bateria do sensor (0-100%)
    /// </summary>
    public int? NivelBateria { get; private set; }
    
    /// <summary>
    /// Intensidade do sinal (RSSI para sensores wireless)
    /// </summary>
    public int? IntensidadeSinal { get; private set; }
    
    /// <summary>
    /// Dados brutos/adicionais em JSON
    /// </summary>
    public string? DadosAdicionais { get; private set; }
    
    /// <summary>
    /// Observações sobre a leitura
    /// </summary>
    public string? Observacoes { get; private set; }
    
    // Relacionamentos
    public Sensor Sensor { get; private set; } = null!;
    
    // EF Core
    private LeituraSensor() 
    { 
        Unidade = string.Empty;
    }
    
    public LeituraSensor(
        Guid sensorId,
        decimal valor,
        string unidade,
        DateTime timestampLeitura,
        QualidadeLeitura qualidade = QualidadeLeitura.Normal,
        int? nivelBateria = null,
        int? intensidadeSinal = null,
        string? dadosAdicionais = null,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(unidade))
            throw new ArgumentException("Unidade é obrigatória", nameof(unidade));
        
        if (timestampLeitura > DateTime.UtcNow.AddMinutes(5))
            throw new ArgumentException("Timestamp da leitura não pode ser futuro", nameof(timestampLeitura));
        
        ValidarNivelBateria(nivelBateria);
        
        Id = Guid.NewGuid();
        SensorId = sensorId;
        Valor = valor;
        Unidade = unidade.Trim();
        TimestampLeitura = timestampLeitura;
        TimestampRecebimento = DateTime.UtcNow;
        Qualidade = qualidade;
        NivelBateria = nivelBateria;
        IntensidadeSinal = intensidadeSinal;
        DadosAdicionais = dadosAdicionais;
        Observacoes = observacoes;
    }
    
    public void MarcarComoSuspeita(string motivo)
    {
        Qualidade = QualidadeLeitura.Suspeita;
        Observacoes = motivo;
    }
    
    public void MarcarComoInvalida(string motivo)
    {
        Qualidade = QualidadeLeitura.Invalida;
        Observacoes = motivo;
    }
    
    public void MarcarComoCalibrada(string motivo)
    {
        Qualidade = QualidadeLeitura.Calibrada;
        Observacoes = motivo;
    }
    
    public bool BateriaBaixa() => NivelBateria.HasValue && NivelBateria.Value < 20;
    
    public bool SinalFraco() => IntensidadeSinal.HasValue && IntensidadeSinal.Value < -80; // RSSI em dBm
    
    public TimeSpan LatenciaRecebimento() => TimestampRecebimento - TimestampLeitura;
    
    private static void ValidarNivelBateria(int? nivel)
    {
        if (nivel.HasValue && (nivel.Value < 0 || nivel.Value > 100))
            throw new ArgumentException("Nível de bateria deve estar entre 0 e 100", nameof(nivel));
    }
}

