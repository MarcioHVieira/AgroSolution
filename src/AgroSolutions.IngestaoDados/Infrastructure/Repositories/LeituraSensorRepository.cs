using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using AgroSolutions.IngestaoDados.Domain.Interfaces;
using AgroSolutions.IngestaoDados.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.IngestaoDados.Infrastructure.Repositories;

public class LeituraSensorRepository : ILeituraSensorRepository
{
    private readonly IngestaoDbContext _context;

    public LeituraSensorRepository(IngestaoDbContext context)
    {
        _context = context;
    }

    public async Task<LeituraSensor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Leituras
            .Include(l => l.Sensor)
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task<List<LeituraSensor>> ObterPorSensorIdAsync(Guid sensorId, int limite = 100, CancellationToken cancellationToken = default)
    {
        return await _context.Leituras
            .AsNoTracking()
            .Where(l => l.SensorId == sensorId)
            .OrderByDescending(l => l.TimestampLeitura)
            .Take(limite)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LeituraSensor>> ObterPorPropriedadeIdAsync(Guid propriedadeId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        return await _context.Leituras
            .Include(l => l.Sensor)
            .AsNoTracking()
            .Where(l => l.Sensor.PropriedadeId == propriedadeId &&
                       l.TimestampLeitura >= dataInicio &&
                       l.TimestampLeitura <= dataFim)
            .OrderByDescending(l => l.TimestampLeitura)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<LeituraSensor>> ObterPorPeriodoAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        return await _context.Leituras
            .AsNoTracking()
            .Where(l => l.SensorId == sensorId &&
                       l.TimestampLeitura >= dataInicio &&
                       l.TimestampLeitura <= dataFim)
            .OrderBy(l => l.TimestampLeitura)
            .ToListAsync(cancellationToken);
    }

    public async Task<LeituraSensor?> ObterUltimaLeituraAsync(Guid sensorId, CancellationToken cancellationToken = default)
    {
        return await _context.Leituras
            .AsNoTracking()
            .Where(l => l.SensorId == sensorId)
            .OrderByDescending(l => l.TimestampLeitura)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<List<LeituraSensor>> ObterLeiturasAnomalasAsync(Guid sensorId, CancellationToken cancellationToken = default)
    {
        return await _context.Leituras
            .AsNoTracking()
            .Where(l => l.SensorId == sensorId &&
                       (l.Qualidade == QualidadeLeitura.Suspeita || l.Qualidade == QualidadeLeitura.Invalida))
            .OrderByDescending(l => l.TimestampLeitura)
            .Take(50)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal?> ObterMediaPeriodoAsync(Guid sensorId, DateTime dataInicio, DateTime dataFim, CancellationToken cancellationToken = default)
    {
        var leituras = await _context.Leituras
            .AsNoTracking()
            .Where(l => l.SensorId == sensorId &&
                       l.TimestampLeitura >= dataInicio &&
                       l.TimestampLeitura <= dataFim &&
                       l.Qualidade == QualidadeLeitura.Normal)
            .ToListAsync(cancellationToken);

        return leituras.Any() ? leituras.Average(l => l.Valor) : null;
    }

    public async Task AdicionarAsync(LeituraSensor leitura, CancellationToken cancellationToken = default)
    {
        await _context.Leituras.AddAsync(leitura, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AdicionarLoteAsync(List<LeituraSensor> leituras, CancellationToken cancellationToken = default)
    {
        await _context.Leituras.AddRangeAsync(leituras, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(LeituraSensor leitura, CancellationToken cancellationToken = default)
    {
        _context.Leituras.Update(leitura);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var leitura = await _context.Leituras.FindAsync(new object[] { id }, cancellationToken);
        if (leitura != null)
        {
            _context.Leituras.Remove(leitura);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RemoverAntigasAsync(DateTime dataLimite, CancellationToken cancellationToken = default)
    {
        var leiturasAntigas = await _context.Leituras
            .Where(l => l.TimestampLeitura < dataLimite)
            .ToListAsync(cancellationToken);

        _context.Leituras.RemoveRange(leiturasAntigas);
        await _context.SaveChangesAsync(cancellationToken);
    }
}

