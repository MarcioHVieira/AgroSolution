using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Data;
using Dapper;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace AgroSolutions.Propriedades.Infrastructure.Repositories;

public class TalhaoRepository : ITalhaoRepository
{
    private readonly PropriedadesDbContext _context;
    private IDbConnection Connection => _context.Database.GetDbConnection();

    public TalhaoRepository(PropriedadesDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Obtém talhão por ID usando Dapper para melhor performance
    /// Inclui navegação para Propriedade (necessária para autorização) e Culturas
    /// </summary>
    public async Task<Talhao?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        const string sql = @"
            SELECT 
                t.Id, t.PropriedadeId, t.Nome, t.Descricao, t.Area, 
                t.Latitude, t.Longitude, t.Poligono, t.Status, 
                t.DataCadastro, t.DataAtualizacao,
                p.Id, p.ProprietarioId, p.Nome, p.Descricao, p.AreaTotal, p.Tipo,
                p.Cep, p.Endereco, p.Numero, p.Complemento, p.Bairro, p.Cidade, p.Estado,
                p.Latitude, p.Longitude, p.Status, p.DataCadastro, p.DataAtualizacao,
                c.Id, c.TalhaoId, c.Tipo, c.Variedade, c.AreaPlantada, 
                c.DataPlantio, c.DataColheitaPrevista, c.DataColheitaRealizada,
                c.ProducaoEstimada, c.ProducaoReal, c.Observacoes, c.Status,
                c.DataCadastro, c.DataAtualizacao
            FROM Talhoes t
            INNER JOIN Propriedades p ON t.PropriedadeId = p.Id
            LEFT JOIN Culturas c ON t.Id = c.TalhaoId
            WHERE t.Id = @Id";

        await _context.Database.OpenConnectionAsync(cancellationToken);
        
        try
        {
            var talhaoDict = new Dictionary<Guid, Talhao>();

            var result = await Connection.QueryAsync<Talhao, Propriedade, Cultura?, Talhao>(
                sql,
                (talhao, propriedade, cultura) =>
                {
                    if (!talhaoDict.TryGetValue(talhao.Id, out var talhaoEntry))
                    {
                        talhaoEntry = talhao;
                        talhaoEntry.GetType().GetProperty("Propriedade")!.SetValue(talhaoEntry, propriedade);
                        talhaoDict.Add(talhao.Id, talhaoEntry);
                    }

                    if (cultura != null)
                    {
                        var culturas = talhaoEntry.Culturas as List<Cultura> ?? new List<Cultura>();
                        culturas.Add(cultura);
                        talhaoEntry.GetType().GetProperty("Culturas")!.SetValue(talhaoEntry, culturas);
                    }

                    return talhaoEntry;
                },
                new { Id = id },
                splitOn: "Id,Id"
            );

            return talhaoDict.Values.FirstOrDefault();
        }
        finally
        {
            await _context.Database.CloseConnectionAsync();
        }
    }

    public async Task<List<Talhao>> ObterPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        return await _context.Talhoes
            .Include(t => t.Culturas)
            .AsNoTracking()
            .Where(t => t.PropriedadeId == propriedadeId)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<Talhao>> ObterDisponiveisPorPropriedadeIdAsync(Guid propriedadeId, CancellationToken cancellationToken = default)
    {
        return await _context.Talhoes
            .AsNoTracking()
            .Where(t => t.PropriedadeId == propriedadeId && t.Status == StatusTalhao.Disponivel)
            .OrderBy(t => t.Nome)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExisteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Talhoes.AnyAsync(t => t.Id == id, cancellationToken);
    }

    public async Task AdicionarAsync(Talhao talhao, CancellationToken cancellationToken = default)
    {
        await _context.Talhoes.AddAsync(talhao, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task AtualizarAsync(Talhao talhao, CancellationToken cancellationToken = default)
    {
        _context.Talhoes.Update(talhao);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task RemoverAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var talhao = await _context.Talhoes.FindAsync(new object[] { id }, cancellationToken);
        if (talhao != null)
        {
            _context.Talhoes.Remove(talhao);
            await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
