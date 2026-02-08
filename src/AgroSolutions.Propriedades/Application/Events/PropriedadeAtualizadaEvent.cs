namespace AgroSolutions.Propriedades.Application.Events;

/// <summary>
/// Evento publicado quando uma propriedade é atualizada
/// Inclui dados do proprietário para Event-Driven Architecture
/// </summary>
public record PropriedadeAtualizadaEvent(
    Guid PropriedadeId,
    string Nome,
    Guid ProprietarioId,
    string EmailProprietario,
    string NomeProprietario,
    DateTime DataAtualizacao
);
