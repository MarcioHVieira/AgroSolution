using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;
using AgroSolutions.Analise.Domain.Interfaces;
using AgroSolutions.Analise.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Analise.Infrastructure.Repositories;

public class AlertaRepository : IAlertaRepository
{
    private readonly AnaliseDbContext _context;

    public AlertaRepository(AnaliseDbContext context)
    {
        _context = context;
    }

    public async Task<Alerta?> ObterPorIdAsync(Guid id)
    {
        return await _context.Alertas.FindAsync(id);
    }

    public async Task<IEnumerable<Alerta>> ObterTodosPorTalhaoAsync(Guid talhaoId)
    {
        return await _context.Alertas
            .Where(a => a.TalhaoId == talhaoId)
            .OrderByDescending(a => a.DataGeracao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> ObterAtivosAsync()
    {
        return await _context.Alertas
            .Where(a => a.Status == StatusAlerta.Ativo)
            .OrderByDescending(a => a.DataGeracao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> ObterPorStatusAsync(StatusAlerta status)
    {
        return await _context.Alertas
            .Where(a => a.Status == status)
            .OrderByDescending(a => a.DataGeracao)
            .ToListAsync();
    }

    public async Task<IEnumerable<Alerta>> ObterPorTipoAsync(TipoAlerta tipo)
    {
        return await _context.Alertas
            .Where(a => a.Tipo == tipo)
            .OrderByDescending(a => a.DataGeracao)
            .ToListAsync();
    }

    public async Task<Alerta> AdicionarAsync(Alerta alerta)
    {
        _context.Alertas.Add(alerta);
        await _context.SaveChangesAsync();
        return alerta;
    }

    public async Task AtualizarAsync(Alerta alerta)
    {
        _context.Alertas.Update(alerta);
        await _context.SaveChangesAsync();
    }

    public async Task DeletarAsync(Guid id)
    {
        var alerta = await ObterPorIdAsync(id);
        if (alerta != null)
        {
            _context.Alertas.Remove(alerta);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExisteAlertaAtivoAsync(Guid talhaoId, TipoAlerta tipo)
    {
        return await _context.Alertas
            .AnyAsync(a => a.TalhaoId == talhaoId && a.Tipo == tipo && a.Status == StatusAlerta.Ativo);
    }
}
