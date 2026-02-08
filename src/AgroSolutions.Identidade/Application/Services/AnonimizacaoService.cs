using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Domain.Interfaces;

namespace AgroSolutions.Identidade.Application.Services;

/// <summary>
/// Serviço de anonimização de dados para conformidade LGPD
/// </summary>
public class AnonimizacaoService : IAnonimizacaoService
{
    private readonly IUsuarioRepository _usuarioRepository;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILogger<AnonimizacaoService> _logger;

    public AnonimizacaoService(
        IUsuarioRepository usuarioRepository,
        IAuditoriaService auditoriaService,
        ILogger<AnonimizacaoService> logger)
    {
        _usuarioRepository = usuarioRepository;
        _auditoriaService = auditoriaService;
        _logger = logger;
    }

    public async Task AnonimizarDadosUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        _logger.LogWarning("Iniciando anonimização de dados do usuário {UsuarioId}", usuarioId);

        var usuario = await _usuarioRepository.ObterPorIdAsync(usuarioId, cancellationToken);
        if (usuario == null)
        {
            _logger.LogWarning("Usuário {UsuarioId} não encontrado para anonimização", usuarioId);
            return;
        }

        // Guardar dados para auditoria
        var dadosAntigos = new
        {
            usuario.NomeCompleto,
            usuario.Email,
            usuario.Telefone,
            usuario.Cpf
        };

        // Anonimizar dados pessoais
        usuario.Anonimizar();
        await _usuarioRepository.AtualizarAsync(usuario, cancellationToken);

        // Registrar auditoria da anonimização
        await _auditoriaService.RegistrarAsync(
            acao: "ANONIMIZACAO_DADOS",
            entidade: "Usuario",
            entidadeId: usuarioId,
            dadosAntigos: dadosAntigos,
            dadosNovos: new { Anonimizado = true },
            sucesso: true
        );

        _logger.LogInformation("Dados do usuário {UsuarioId} anonimizados com sucesso", usuarioId);
    }

    public async Task<int> ProcessarExclusoesAutomaticasAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Iniciando processamento de exclusões automáticas");

        try
        {
            // Buscar usuários marcados para exclusão há mais de 30 dias (LGPD Art. 16)
            var dataLimite = DateTime.UtcNow.AddDays(-30);
            var usuariosParaExcluir = await _usuarioRepository.ObterMarcadosParaExclusaoAsync(dataLimite, cancellationToken);

            _logger.LogInformation("Encontrados {Count} usuários para anonimizar", usuariosParaExcluir.Count);

            int sucessos = 0;
            int falhas = 0;

            foreach (var usuario in usuariosParaExcluir)
            {
                try
                {
                    await AnonimizarDadosUsuarioAsync(usuario.Id, cancellationToken);
                    sucessos++;
                    _logger.LogInformation("Usuário {UsuarioId} anonimizado com sucesso", usuario.Id);
                }
                catch (Exception ex)
                {
                    falhas++;
                    _logger.LogError(ex,
                        "Erro ao anonimizar usuário {UsuarioId}: {Message}",
                        usuario.Id, ex.Message);
                }
            }

            _logger.LogInformation(
                "Processamento de exclusões concluído. Sucessos: {Sucessos}, Falhas: {Falhas}",
                sucessos, falhas);

            return sucessos;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro no processamento de exclusões automáticas: {Message}", ex.Message);
            throw;
        }
    }
}
