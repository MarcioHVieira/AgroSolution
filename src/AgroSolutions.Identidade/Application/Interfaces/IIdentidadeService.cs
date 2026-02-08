using AgroSolutions.Identidade.Application.DTOs;

namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface para serviços de autenticação e autorização
/// </summary>
public interface IIdentidadeService
{
    Task<RegistroResponseDto> RegistrarUsuarioAsync(RegistrarUsuarioDto dto, CancellationToken cancellationToken = default);
    Task ValidarCodigoAsync(ValidarCodigoDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> LoginAsync(LoginDto dto, CancellationToken cancellationToken = default);
    Task<TokenResponseDto> RefreshTokenAsync(RefreshTokenDto dto, CancellationToken cancellationToken = default);
    Task RevogarTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task ReenviarCodigoValidacaoAsync(string email, CancellationToken cancellationToken = default);
    Task EsqueciSenhaAsync(EsqueciSenhaDto dto, CancellationToken cancellationToken = default);
    Task RedefinirSenhaAsync(RedefinirSenhaDto dto, CancellationToken cancellationToken = default);
    Task AlterarSenhaAsync(Guid usuarioId, AlterarSenhaDto dto, CancellationToken cancellationToken = default);
    Task<object> ExportarDadosUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    Task ExcluirContaAsync(Guid usuarioId, string senha, CancellationToken cancellationToken = default);
}
