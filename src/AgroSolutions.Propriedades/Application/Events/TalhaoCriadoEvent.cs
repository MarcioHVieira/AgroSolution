namespace AgroSolutions.Propriedades.Application.Events;

/// <summary>
/// Evento publicado quando um talhão é criado
/// Inclui informações do proprietário para Event-Driven Architecture
/// </summary>
public record TalhaoCriadoEvent(
    Guid TalhaoId,
    Guid PropriedadeId,
    string Nome,
    decimal AreaHectares,
    string Cultura,
    string Status,
    DateTime DataCriacao,
    Guid ProprietarioId,
    string EmailProprietario,
    string NomeProprietario
);
