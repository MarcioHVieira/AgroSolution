namespace AgroSolutions.Propriedades.Application.Events;

/// <summary>
/// Evento publicado quando uma propriedade é criada
/// Inclui dados do proprietário para Event-Driven Architecture
/// </summary>
public record PropriedadeCriadaEvent(
    Guid PropriedadeId,
    string Nome,
    string? Endereco,
    decimal? AreaTotal,
    Guid ProprietarioId,
    DateTime DataCriacao,
    string EmailProprietario,
    string NomeProprietario
);
