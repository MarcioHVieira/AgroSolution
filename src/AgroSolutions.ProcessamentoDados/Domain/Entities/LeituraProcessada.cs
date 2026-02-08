using AgroSolutions.ProcessamentoDados.Domain.Enums;

namespace AgroSolutions.ProcessamentoDados.Domain.Entities;

/// <summary>
/// Representa uma leitura de sensor processada e armazenada
/// </summary>
public class LeituraProcessada
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// ID da leitura original (do IngestaoDados)
    /// </summary>
    public Guid LeituraOrigemId { get; private set; }
    
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
    /// Valor medido
    /// </summary>
    public decimal Valor { get; private set; }
    
    /// <summary>
    /// Unidade de medida
    /// </summary>
    public string Unidade { get; private set; }
    
    /// <summary>
    /// Timestamp da leitura no sensor
    /// </summary>
    public DateTime TimestampLeitura { get; private set; }
    
    /// <summary>
    /// Timestamp de recebimento no IngestaoDados
    /// </summary>
    public DateTime TimestampRecebimento { get; private set; }
    
    /// <summary>
    /// Timestamp de processamento
    /// </summary>
    public DateTime TimestampProcessamento { get; private set; }
    
    /// <summary>
    /// Qualidade da leitura
    /// </summary>
    public QualidadeLeitura Qualidade { get; private set; }
    
    /// <summary>
    /// Nível de bateria do sensor (%)
    /// </summary>
    public int? NivelBateria { get; private set; }
    
    /// <summary>
    /// Intensidade do sinal (RSSI em dBm)
    /// </summary>
    public int? IntensidadeSinal { get; private set; }
    
    /// <summary>
    /// Status do processamento
    /// </summary>
    public StatusProcessamento Status { get; private set; }
    
    /// <summary>
    /// Dados adicionais em formato JSON
    /// </summary>
    public string? DadosAdicionais { get; private set; }
    
    /// <summary>
    /// Mensagem de erro (se houver falha)
    /// </summary>
    public string? MensagemErro { get; private set; }
    
    public DateTime DataCriacao { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    
    // EF Core
    private LeituraProcessada() 
    { 
        DeviceId = string.Empty;
        Unidade = string.Empty;
    }
    
    public LeituraProcessada(
        Guid leituraOrigemId,
        Guid sensorId,
        string deviceId,
        Guid propriedadeId,
        TipoSensor tipoSensor,
        decimal valor,
        string unidade,
        DateTime timestampLeitura,
        DateTime timestampRecebimento,
        QualidadeLeitura qualidade,
        Guid? talhaoId = null,
        int? nivelBateria = null,
        int? intensidadeSinal = null,
        string? dadosAdicionais = null)
    {
        ValidarInvariantes(deviceId, unidade);
        
        Id = Guid.NewGuid();
        LeituraOrigemId = leituraOrigemId;
        SensorId = sensorId;
        DeviceId = deviceId.ToUpperInvariant();
        PropriedadeId = propriedadeId;
        TalhaoId = talhaoId;
        TipoSensor = tipoSensor;
        Valor = valor;
        Unidade = unidade;
        TimestampLeitura = timestampLeitura;
        TimestampRecebimento = timestampRecebimento;
        TimestampProcessamento = DateTime.UtcNow;
        Qualidade = qualidade;
        NivelBateria = nivelBateria;
        IntensidadeSinal = intensidadeSinal;
        Status = StatusProcessamento.Processado;
        DadosAdicionais = dadosAdicionais;
        DataCriacao = DateTime.UtcNow;
    }
    
    public void MarcarComoFalha(string mensagemErro)
    {
        Status = StatusProcessamento.Falha;
        MensagemErro = mensagemErro;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void Reprocessar()
    {
        Status = StatusProcessamento.Reprocessando;
        MensagemErro = null;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void MarcarComoProcessado()
    {
        Status = StatusProcessamento.Processado;
        MensagemErro = null;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    // Validações de invariantes
    private static void ValidarInvariantes(string deviceId, string unidade)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID não pode ser vazio", nameof(deviceId));
        
        if (string.IsNullOrWhiteSpace(unidade))
            throw new ArgumentException("Unidade não pode ser vazia", nameof(unidade));
    }
}
