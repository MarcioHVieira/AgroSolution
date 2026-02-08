using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Interfaces;
using AgroSolutions.Analise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Analise.Infrastructure.Repositories;

public class RegraAlertaRepository : IRegraAlertaRepository
{
    private readonly AnaliseDbContext _context;

    public RegraAlertaRepository(AnaliseDbContext context)
    {
        _context = context;
    }

    public async Task<RegraAlerta?> ObterPorIdAsync(Guid id)
    {
        return await _context.RegrasAlertas.FindAsync(id);
    }

    public async Task<IEnumerable<RegraAlerta>> ObterTodasAsync()
    {
        return await _context.RegrasAlertas
            .OrderBy(r => r.Nome)
            .ToListAsync();
    }

    public async Task<IEnumerable<RegraAlerta>> ObterAtivasAsync()
    {
        return await _context.RegrasAlertas
            .Where(r => r.Ativa)
            .OrderBy(r => r.Nome)
            .ToListAsync();
    }

    public async Task<RegraAlerta> AdicionarAsync(RegraAlerta regra)
    {
        _context.RegrasAlertas.Add(regra);
        await _context.SaveChangesAsync();
        return regra;
    }

    public async Task AtualizarAsync(RegraAlerta regra)
    {
        _context.RegrasAlertas.Update(regra);
        await _context.SaveChangesAsync();
    }

    public async Task DeletarAsync(Guid id)
    {
        var regra = await ObterPorIdAsync(id);
        if (regra != null)
        {
            _context.RegrasAlertas.Remove(regra);
            await _context.SaveChangesAsync();
        }
    }
}
