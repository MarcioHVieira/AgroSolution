using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using AgroSolutions.IngestaoDados.Domain.Interfaces;
using AgroSolutions.IngestaoDados.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.IngestaoDados.Infrastructure.Repositories;

public class SensorRepository : ISensorRepository
{
    private readonly IngestaoDbContext _context;

    public SensorRepository(IngestaoDbContext context)
    {
        _context = context;
    }

    public async Task<Sensor?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sensores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public async Task<Sensor?> ObterPorDeviceIdAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var deviceIdUpper = deviceId.ToUpperInvariant();
        return await _context.Sensores
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.DeviceId == deviceIdUpper, cancellationToken);
    }

    public async Task<List<Sensor>> ObterPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        return await _context.Sensores
            .AsNoTracking()
            .Where(s => s.PropriedadeId == propriedadeId)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sensor>> ObterPorTalhaoIdAsync(Guid talhaoId, CancellationToken cancellationToken = default)
    {
        return await _context.Sensores
            .AsNoTracking()
            .Where(s => s.TalhaoId == talhaoId)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sensor>> ObterPorTipoAsync(TipoSensor tipo, CancellationToken cancellationToken = default)
    {
        return await _context.Sensores
            .AsNoTracking()
            .Where(s => s.Tipo == tipo)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sensor>> ObterPorStatusAsync(StatusSensor status, CancellationToken cancellationToken = default)
    {
        return await _context.Sensores
            .AsNoTracking()
            .Where(s => s.Status == status)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sensor>> ObterAtivosPorPropriedadeAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        return await _context.Sensores
            .AsNoTracking()
            .Where(s => s.PropriedadeId == propriedadeId && s.Status == StatusSensor.Ativo)
            .OrderBy(s => s.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Sensor>> ObterSensoresComBateriaBaixaAsync(CancellationToken cancellationToken = default)
    {
        // Busca sensores que tiveram última leitura com bateria baixa
        var sensoresComBateriaBaixa = await _context.Leituras
            .Where(l => l.NivelBateria.HasValue && l.NivelBateria.Value < 20)
            .GroupBy(l => l.SensorId)
            .Select(g => g.OrderByDescending(l => l.TimestampLeitura).First().SensorId)
            .ToListAsync(cancellationToken);

        return await _context.Sensores
            .AsNoTracking()
            .Where(s => sensoresComBateriaBaixa.Contains(s.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> DeviceIdExisteAsync(string deviceId, CancellationToken cancellationToken = default)
    {
        var deviceIdUpper = deviceId.ToUpperInvariant();
        return await _context.Sensores
            .AnyAsync(s => s.DeviceId == deviceIdUpper, cancellationToken);
    }

    public async Task AdicionarAsync(Sensor sensor, CancellationToken cancellationToken = default)
    {
        await _context.Sensores.AddAsync(sensor, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Sensor sensor, CancellationToken cancellationToken = default)
    {
        _context.Sensores.Update(sensor);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var sensor = await _context.Sensores.FindAsync(new object[] { id }, cancellationToken);
        if (sensor != null)
        {
            _context.Sensores.Remove(sensor);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}

