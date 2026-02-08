using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AgroSolutions.Propriedades.Infrastructure.Repositories;

public class CulturaRepository : ICulturaRepository
{
    private readonly PropriedadesDbContext _context;
    private IDbConnection Connection => _context.Database.GetDbConnection();

    public CulturaRepository(PropriedadesDbContext context)
    {
        _context = context;
    }

    public async Task<Cultura?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Culturas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<List<Cultura>> ObterPorTalhaoIdAsync(Guid talhaoId, CancellationToken cancellationToken = default)
    {
        return await _context.Culturas
            .AsNoTracking()
            .Where(c => c.TalhaoId == talhaoId)
            .OrderByDescending(c => c.DataPlantio)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Cultura>> ObterPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        return await _context.Culturas
            .Include(c => c.Talhao)
            .AsNoTracking()
            .Where(c => c.Talhao.PropriedadeId == propriedadeId)
            .OrderByDescending(c => c.DataPlantio)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Obtém culturas ativas usando Dapper com JOINs otimizados
    /// Inclui navegação completa: Cultura -> Talhao -> Propriedade (necessária para autorização)
    /// </summary>
    public async Task<List<Cultura>> ObterAtivasAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                c.Id, c.TalhaoId, c.Tipo, c.Variedade, c.AreaPlantada,
                c.DataPlantio, c.DataColheitaPrevista, c.DataColheitaRealizada,
                c.ProducaoEstimada, c.ProducaoReal, c.Observacoes, c.Status,
                c.DataCadastro, c.DataAtualizacao,
                t.Id, t.PropriedadeId, t.Nome, t.Descricao, t.Area,
                t.Latitude, t.Longitude, t.Poligono, t.Status,
                t.DataCadastro, t.DataAtualizacao,
                p.Id, p.ProprietarioId, p.Nome, p.Descricao, p.AreaTotal, p.Tipo,
                p.Cep, p.Endereco, p.Numero, p.Complemento, p.Bairro, p.Cidade, p.Estado,
                p.Latitude, p.Longitude, p.Status, p.DataCadastro, p.DataAtualizacao
            FROM Culturas c
            INNER JOIN Talhoes t ON c.TalhaoId = t.Id
            INNER JOIN Propriedades p ON t.PropriedadeId = p.Id
            WHERE c.Status = @Status
            ORDER BY c.DataPlantio DESC";

        await _context.Database.OpenConnectionAsync(cancellationToken);

        try
        {
            var result = await Connection.QueryAsync<Cultura, Talhao, Propriedade, Cultura>(
                sql,
                (cultura, talhao, propriedade) =>
                {
                    talhao.GetType().GetProperty("Propriedade")!.SetValue(talhao, propriedade);
                    cultura.GetType().GetProperty("Talhao")!.SetValue(cultura, talhao);
                    
                    return cultura;
                },
                new { Status = (int)StatusCultura.Ativa },
                splitOn: "Id,Id"
            );

            return result.ToList();
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<List<Cultura>> ObterPorTipoAsync(TipoCultura tipo, CancellationToken cancellationToken = default)
    {
        return await _context.Culturas
            .AsNoTracking()
            .Where(c => c.Tipo == tipo)
            .OrderByDescending(c => c.DataPlantio)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Culturas.AnyAsync(c => c.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Cultura cultura, CancellationToken cancellationToken = default)
    {
        await _context.Culturas.AddAsync(cultura, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Cultura cultura, CancellationToken cancellationToken = default)
    {
        _context.Culturas.Update(cultura);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cultura = await _context.Culturas.FindAsync(new object[] { id }, cancellationToken);
        if (cultura != null)
        {
            _context.Culturas.Remove(cultura);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
