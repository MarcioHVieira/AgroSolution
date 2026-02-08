using AgroSolutions.IngestaoDados.Domain.Enums;

namespace AgroSolutions.IngestaoDados.Domain.Entities;

/// <summary>
/// Representa um sensor IoT instalado em uma propriedade rural
/// </summary>
public class Sensor
{
    public Guid Id { get; private set; }
    
    /// <summary>
    /// ID da propriedade onde o sensor está instalado
    /// </summary>
    public Guid PropriedadeId { get; private set; }
    
    /// <summary>
    /// ID do talhão específico (opcional)
    /// </summary>
    public Guid? TalhaoId { get; private set; }
    
    /// <summary>
    /// Identificador único do dispositivo físico
    /// </summary>
    public string DeviceId { get; private set; }
    
    /// <summary>
    /// Nome/descrição do sensor
    /// </summary>
    public string Nome { get; private set; }
    
    /// <summary>
    /// Tipo de sensor
    /// </summary>
    public TipoSensor Tipo { get; private set; }
    
    /// <summary>
    /// Fabricante do sensor
    /// </summary>
    public string? Fabricante { get; private set; }
    
    /// <summary>
    /// Modelo do sensor
    /// </summary>
    public string? Modelo { get; private set; }
    
    /// <summary>
    /// Localização geográfica do sensor
    /// </summary>
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    
    /// <summary>
    /// Altitude em metros
    /// </summary>
    public decimal? Altitude { get; private set; }
    
    /// <summary>
    /// Intervalo de leitura em minutos
    /// </summary>
    public int IntervaloLeituraMinutos { get; private set; }
    
    /// <summary>
    /// Status operacional do sensor
    /// </summary>
    public StatusSensor Status { get; private set; }
    
    /// <summary>
    /// Data da última leitura recebida
    /// </summary>
    public DateTime? UltimaLeitura { get; private set; }
    
    /// <summary>
    /// Data da última calibração
    /// </summary>
    public DateTime? UltimaCalibracao { get; private set; }
    
    /// <summary>
    /// Observações sobre o sensor
    /// </summary>
    public string? Observacoes { get; private set; }
    
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    
    // Relacionamentos
    public ICollection<LeituraSensor> Leituras { get; private set; } = new List<LeituraSensor>();
    
    // EF Core
    private Sensor() 
    { 
        DeviceId = string.Empty;
        Nome = string.Empty;
    }
    
    public Sensor(
        Guid propriedadeId,
        string deviceId,
        string nome,
        TipoSensor tipo,
        int intervaloLeituraMinutos = 15,
        Guid? talhaoId = null,
        string? fabricante = null,
        string? modelo = null,
        decimal? latitude = null,
        decimal? longitude = null,
        decimal? altitude = null,
        string? observacoes = null)
    {
        ValidarInvariantes(deviceId, nome, intervaloLeituraMinutos);
        
        Id = Guid.NewGuid();
        PropriedadeId = propriedadeId;
        TalhaoId = talhaoId;
        DeviceId = deviceId.Trim().ToUpperInvariant();
        Nome = nome.Trim();
        Tipo = tipo;
        Fabricante = fabricante?.Trim();
        Modelo = modelo?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        IntervaloLeituraMinutos = intervaloLeituraMinutos;
        Status = StatusSensor.Ativo;
        Observacoes = observacoes;
        DataCadastro = DateTime.UtcNow;
    }
    
    public void Atualizar(
        string nome,
        int intervaloLeituraMinutos,
        Guid? talhaoId = null,
        string? fabricante = null,
        string? modelo = null,
        decimal? latitude = null,
        decimal? longitude = null,
        decimal? altitude = null,
        string? observacoes = null)
    {
        ValidarInvariantesAtualizacao(nome, intervaloLeituraMinutos);
        
        Nome = nome.Trim();
        TalhaoId = talhaoId;
        Fabricante = fabricante?.Trim();
        Modelo = modelo?.Trim();
        Latitude = latitude;
        Longitude = longitude;
        Altitude = altitude;
        IntervaloLeituraMinutos = intervaloLeituraMinutos;
        Observacoes = observacoes;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void AtualizarStatus(StatusSensor novoStatus)
    {
        Status = novoStatus;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void RegistrarLeitura()
    {
        UltimaLeitura = DateTime.UtcNow;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public void RegistrarCalibracao()
    {
        UltimaCalibracao = DateTime.UtcNow;
        Status = StatusSensor.Ativo;
        DataAtualizacao = DateTime.UtcNow;
    }
    
    public bool EstaAtivo() => Status == StatusSensor.Ativo;
    
    public bool PrecisaCalibracao()
    {
        if (UltimaCalibracao == null) return true;
        
        // Calibração recomendada a cada 90 dias
        return (DateTime.UtcNow - UltimaCalibracao.Value).TotalDays > 90;
    }

    private static void ValidarInvariantes(string deviceId, string nome, int intervaloLeituraMinutos)
    {
        if (string.IsNullOrWhiteSpace(deviceId))
            throw new ArgumentException("Device ID não pode ser vazio", nameof(deviceId));
        
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do sensor não pode ser vazio", nameof(nome));
        
        if (intervaloLeituraMinutos <= 0)
            throw new ArgumentException("Intervalo de leitura deve ser maior que zero", nameof(intervaloLeituraMinutos));
    }

    private static void ValidarInvariantesAtualizacao(string nome, int intervaloLeituraMinutos)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do sensor não pode ser vazio", nameof(nome));
        
        if (intervaloLeituraMinutos <= 0)
            throw new ArgumentException("Intervalo de leitura deve ser maior que zero", nameof(intervaloLeituraMinutos));
    }
}


