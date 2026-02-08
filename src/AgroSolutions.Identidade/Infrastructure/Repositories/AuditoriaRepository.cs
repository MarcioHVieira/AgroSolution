using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.Identidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Identidade.Infrastructure.Repositories;

/// <summary>
/// Repositório de auditoria de acessos
/// </summary>
public class AuditoriaRepository : IAuditoriaRepository
{
    private readonly IdentidadeDbContext _context;
    private readonly ILogger<AuditoriaRepository> _logger;

    public AuditoriaRepository(IdentidadeDbContext context, ILogger<AuditoriaRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task AdicionarAsync(AuditoriaAcesso auditoria, CancellationToken cancellationToken = default)
    {
        try
        {
            await _context.AuditoriasAcesso.AddAsync(auditoria, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao adicionar auditoria: {Message}", ex.Message);
            // Não propagar a exceção para não quebrar o fluxo principal
        }
    }

    public async Task<List<AuditoriaAcesso>> ObterPorUsuarioAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasAcesso
            .Where(a => a.UsuarioId == usuarioId)
            .OrderByDescending(a => a.DataHora)
            .Take(100) // Limitar a 100 registros mais recentes
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditoriaAcesso>> ObterPorPeriodoAsync(
        DateTime dataInicio, 
        DateTime dataFim, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasAcesso
            .Where(a => a.DataHora >= dataInicio && a.DataHora <= dataFim)
            .OrderByDescending(a => a.DataHora)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<AuditoriaAcesso>> ObterPorAcaoAsync(string acao, CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasAcesso
            .Where(a => a.Acao == acao)
            .OrderByDescending(a => a.DataHora)
            .Take(100)
            .ToListAsync(cancellationToken);
    }

    public async Task<int> ContarTentativasLoginFalhasAsync(
        string email, 
        DateTime desde, 
        CancellationToken cancellationToken = default)
    {
        return await _context.AuditoriasAcesso
            .Where(a => a.Acao == "LOGIN_FALHOU" 
                && a.DataHora >= desde 
                && a.Sucesso == false)
            .CountAsync(cancellationToken);
    }
}
