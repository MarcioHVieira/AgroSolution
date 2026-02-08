namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface do serviço de auditoria
/// </summary>
public interface IAuditoriaService
{
    Task RegistrarAsync(
        string acao,
        string entidade,
        Guid? entidadeId = null,
        object? dadosAntigos = null,
        object? dadosNovos = null,
        bool sucesso = true,
        string? mensagemErro = null);
}
