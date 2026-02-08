using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;
using AgroSolutions.ProcessamentoDados.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.ProcessamentoDados.Infrastructure.Repositories;

public class AgregacaoDadosRepository : IAgregacaoDadosRepository
{
    private readonly ProcessamentoDbContext _context;

    public AgregacaoDadosRepository(ProcessamentoDbContext context)
    {
        _context = context;
    }

    public async Task<AgregacaoDados?> ObterPorIdAsync(Guid id)
    {
        return await _context.AgregacoesDados.FindAsync(id);
    }

    public async Task<IEnumerable<AgregacaoDados>> ObterPorSensorAsync(
        Guid sensorId,
        TipoAgregacao tipo,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = _context.AgregacoesDados
            .Where(a => a.SensorId == sensorId && a.TipoAgregacao == tipo);

        if (dataInicio.HasValue)
        {
            query = query.Where(a => a.PeriodoInicio >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(a => a.PeriodoInicio < dataFim.Value);
        }

        return await query
            .OrderByDescending(a => a.PeriodoInicio)
            .ToListAsync();
    }

    public async Task<IEnumerable<AgregacaoDados>> ObterPorPropriedadeAsync(
        Guid propriedadeId,
        TipoAgregacao tipo,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = _context.AgregacoesDados
            .Where(a => a.PropriedadeId == propriedadeId && a.TipoAgregacao == tipo);

        if (dataInicio.HasValue)
        {
            query = query.Where(a => a.PeriodoInicio >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(a => a.PeriodoInicio < dataFim.Value);
        }

        return await query
            .OrderByDescending(a => a.PeriodoInicio)
            .ToListAsync();
    }

    public async Task<AgregacaoDados?> ObterPorPeriodoAsync(
        Guid sensorId,
        TipoAgregacao tipo,
        DateTime periodoInicio)
    {
        return await _context.AgregacoesDados
            .FirstOrDefaultAsync(a => 
                a.SensorId == sensorId && 
                a.TipoAgregacao == tipo && 
                a.PeriodoInicio == periodoInicio);
    }

    public async Task AdicionarAsync(AgregacaoDados agregacao)
    {
        _context.AgregacoesDados.Add(agregacao);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(AgregacaoDados agregacao)
    {
        _context.AgregacoesDados.Update(agregacao);
        await _context.SaveChangesAsync();
    }
}
