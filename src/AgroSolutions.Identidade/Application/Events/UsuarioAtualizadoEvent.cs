namespace AgroSolutions.Identidade.Application.Events;

/// <summary>
/// Evento publicado quando um usuário é atualizado
/// </summary>
public record UsuarioAtualizadoEvent(
    Guid Id,
    string Email,
    string NomeCompleto,
    DateTime DataAtualizacao
);
