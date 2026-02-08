namespace AgroSolutions.Notificacoes.Domain.Entities;

/// <summary>
/// Read Model de Propriedade sincronizado via eventos do microserviço Propriedades
/// </summary>
public class PropriedadeInfo
{
    public Guid Id { get; set; }
    public Guid ProprietarioId { get; set; }
    public string EmailProprietario { get; set; } = string.Empty;
    public string NomeProprietario { get; set; } = string.Empty;
    public DateTime DataSincronizacao { get; set; }
}
