using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.Identidade.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Identidade.Infrastructure.Repositories;

public class CodigoValidacaoRepository : ICodigoValidacaoRepository
{
    private readonly IdentidadeDbContext _context;

    public CodigoValidacaoRepository(IdentidadeDbContext context)
    {
        _context = context;
    }

    public async Task<CodigoValidacao?> ObterPorCodigoAsync(string codigo, CancellationToken cancellationToken = default)
    {
        return await _context.CodigosValidacao
            .Include(c => c.Usuario)
            .FirstOrDefaultAsync(c => c.Codigo == codigo, cancellationToken);
    }

    public async Task<CodigoValidacao?> ObterUltimoCodigoValidoAsync(Guid usuarioId, CancellationToken cancellationToken = default)
    {
        return await _context.CodigosValidacao
            .Where(c => c.UsuarioId == usuarioId && !c.Utilizado && c.DataExpiracao > DateTime.UtcNow)
            .OrderByDescending(c => c.DataCriacao)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task AdicionarAsync(CodigoValidacao codigoValidacao, CancellationToken cancellationToken = default)
    {
        await _context.CodigosValidacao.AddAsync(codigoValidacao, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(CodigoValidacao codigoValidacao, CancellationToken cancellationToken = default)
    {
        _context.CodigosValidacao.Update(codigoValidacao);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
