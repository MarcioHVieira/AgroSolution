using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Propriedades.Infrastructure.Repositories;

public class PropriedadeRepository : IPropriedadeRepository
{
    private readonly PropriedadesDbContext _context;

    public PropriedadeRepository(PropriedadesDbContext context)
    {
        _context = context;
    }

    public async Task<Propriedade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Propriedades
            .Include(p => p.Talhoes)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<List<Propriedade>> ObterPorProprietarioIdAsync(Guid proprietarioId, CancellationToken cancellationToken = default)
    {
        return await _context.Propriedades
            .Include(p => p.Talhoes)
            .AsNoTracking()
            .Where(p => p.ProprietarioId == proprietarioId)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Propriedade>> ObterTodasAsync(int pagina = 1, int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        return await _context.Propriedades
            .AsNoTracking()
            .OrderBy(p => p.Nome)
            .Skip((pagina - 1) * tamanhoPagina)
            .Take(tamanhoPagina)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Propriedade>> ObterPorCidadeAsync(string cidade, CancellationToken cancellationToken = default)
    {
        return await _context.Propriedades
            .AsNoTracking()
            .Where(p => p.Cidade == cidade)
            .OrderBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Propriedade>> ObterPorEstadoAsync(string estado, CancellationToken cancellationToken = default)
    {
        return await _context.Propriedades
            .AsNoTracking()
            .Where(p => p.Estado == estado)
            .OrderBy(p => p.Cidade)
            .ThenBy(p => p.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Propriedades.AnyAsync(p => p.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Propriedade propriedade, CancellationToken cancellationToken = default)
    {
        await _context.Propriedades.AddAsync(propriedade, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Propriedade propriedade, CancellationToken cancellationToken = default)
    {
        _context.Propriedades.Update(propriedade);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var propriedade = await _context.Propriedades.FindAsync(new object[] { id }, cancellationToken);
        if (propriedade != null)
        {
            _context.Propriedades.Remove(propriedade);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
