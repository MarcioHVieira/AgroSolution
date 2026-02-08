using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.Identidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Identidade.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IdentidadeDbContext _context;

    public RefreshTokenRepository(IdentidadeDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> ObterPorTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .FirstOrDefaultAsync(rt => rt.Token == token, cancellationToken);
    }

    public async Task<RefreshToken?> ObterPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UsuarioId == usuarioId && !rt.Revogado && rt.DataExpiracao > DateTime.UtcNow)
            .OrderByDescending(rt => rt.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<RefreshToken>> ObterTodosPorUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.UsuarioId == usuarioId)
            .OrderByDescending(rt => rt.DataCriacao)
            .ToListAsync(cancellationToken);
    }

    public async Task AdicionarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(RefreshToken refreshToken, CancellationToken cancellationToken = default)
    {
        _context.RefreshTokens.Update(refreshToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RevogarTodosDoUsuarioAsync(Guid usuarioId, string motivo, CancellationToken cancellationToken = default)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UsuarioId == usuarioId && !rt.Revogado)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revogar(motivo);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverExpiradosAsync(CancellationToken cancellationToken = default)
    {
        var dataLimite = DateTime.UtcNow.AddDays(-30); // Remove tokens expirados há mais de 30 dias

        var tokensExpirados = await _context.RefreshTokens
            .Where(rt => rt.DataExpiracao < dataLimite)
            .ToListAsync(cancellationToken);

        _context.RefreshTokens.RemoveRange(tokensExpirados);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
