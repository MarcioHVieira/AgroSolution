namespace AgroSolutions.Analise.Domain.Entities;

/// <summary>
/// Read Model local de informações de Talhão sincronizado via eventos
/// </summary>
public class TalhaoInfo
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public Guid PropriedadeId { get; set; }
    public Guid ProprietarioId { get; set; }
    public string EmailProprietario { get; set; } = string.Empty;
    public string NomeProprietario { get; set; } = string.Empty;
    public DateTime DataSincronizacao { get; set; }
}
