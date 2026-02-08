using AgroSolutions.ProcessamentoDados.Domain.Entities;
using AgroSolutions.ProcessamentoDados.Domain.Enums;
using AgroSolutions.ProcessamentoDados.Domain.Interfaces;
using AgroSolutions.ProcessamentoDados.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.ProcessamentoDados.Infrastructure.Repositories;

public class LeituraProcessadaRepository : ILeituraProcessadaRepository
{
    private readonly ProcessamentoDbContext _context;

    public LeituraProcessadaRepository(ProcessamentoDbContext context)
    {
        _context = context;
    }

    public async Task<LeituraProcessada?> ObterPorIdAsync(Guid id)
    {
        return await _context.LeiturasProcessadas.FindAsync(id);
    }

    public async Task<LeituraProcessada?> ObterPorLeituraOrigemIdAsync(Guid leituraOrigemId)
    {
        return await _context.LeiturasProcessadas
            .FirstOrDefaultAsync(l => l.LeituraOrigemId == leituraOrigemId);
    }

    public async Task<IEnumerable<LeituraProcessada>> ObterPorSensorAsync(
        Guid sensorId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = _context.LeiturasProcessadas
            .Where(l => l.SensorId == sensorId);

        if (dataInicio.HasValue)
        {
            query = query.Where(l => l.TimestampLeitura >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(l => l.TimestampLeitura < dataFim.Value);
        }

        return await query
            .OrderByDescending(l => l.TimestampLeitura)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeituraProcessada>> ObterPorPropriedadeAsync(
        Guid propriedadeId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = _context.LeiturasProcessadas
            .Where(l => l.PropriedadeId == propriedadeId);

        if (dataInicio.HasValue)
        {
            query = query.Where(l => l.TimestampLeitura >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(l => l.TimestampLeitura < dataFim.Value);
        }

        return await query
            .OrderByDescending(l => l.TimestampLeitura)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeituraProcessada>> ObterPorTalhaoAsync(
        Guid talhaoId,
        DateTime? dataInicio = null,
        DateTime? dataFim = null)
    {
        var query = _context.LeiturasProcessadas
            .Where(l => l.TalhaoId == talhaoId);

        if (dataInicio.HasValue)
        {
            query = query.Where(l => l.TimestampLeitura >= dataInicio.Value);
        }

        if (dataFim.HasValue)
        {
            query = query.Where(l => l.TimestampLeitura < dataFim.Value);
        }

        return await query
            .OrderByDescending(l => l.TimestampLeitura)
            .ToListAsync();
    }

    public async Task<IEnumerable<LeituraProcessada>> ObterComFalhaAsync(int limit = 100)
    {
        return await _context.LeiturasProcessadas
            .Where(l => l.Status == StatusProcessamento.Falha)
            .OrderBy(l => l.DataCriacao)
            .Take(limit)
            .ToListAsync();
    }

    public async Task AdicionarAsync(LeituraProcessada leitura)
    {
        _context.LeiturasProcessadas.Add(leitura);
        await _context.SaveChangesAsync();
    }

    public async Task AtualizarAsync(LeituraProcessada leitura)
    {
        _context.LeiturasProcessadas.Update(leitura);
        await _context.SaveChangesAsync();
    }

    public async Task<int> ContarPorStatusAsync(StatusProcessamento status)
    {
        return await _context.LeiturasProcessadas
            .CountAsync(l => l.Status == status);
    }
}
