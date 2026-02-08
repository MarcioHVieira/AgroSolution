namespace AgroSolutions.Propriedades.Application.Events;

/// <summary>
/// Evento publicado quando um talhão é atualizado
/// Inclui informações do proprietário para Event-Driven Architecture
/// </summary>
public record TalhaoAtualizadoEvent(
    Guid TalhaoId,
    Guid PropriedadeId,
    string Nome,
    decimal AreaHectares,
    string Cultura,
    string Status,
    DateTime DataAtualizacao,
    Guid ProprietarioId,
    string EmailProprietario,
    string NomeProprietario
);
