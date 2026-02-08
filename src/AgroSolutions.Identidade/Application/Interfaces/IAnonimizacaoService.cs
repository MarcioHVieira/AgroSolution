namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface do serviço de anonimização (LGPD)
/// </summary>
public interface IAnonimizacaoService
{
    Task AnonimizarDadosUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task<int> ProcessarExclusoesAutomaticasAsync(CancellationToken cancellationToken = default);
}
