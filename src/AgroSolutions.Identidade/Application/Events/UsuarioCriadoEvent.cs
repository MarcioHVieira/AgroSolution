namespace AgroSolutions.Identidade.Application.Events;

/// <summary>
/// Evento publicado quando um usuário é criado
/// </summary>
public record UsuarioCriadoEvent(
    Guid Id,
    string Email,
    string NomeCompleto,
    DateTime DataCriacao
);
