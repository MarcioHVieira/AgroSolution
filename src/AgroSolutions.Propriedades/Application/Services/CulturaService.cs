using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Interfaces;
using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Interfaces;

namespace AgroSolutions.Propriedades.Application.Services;

public class CulturaService : ICulturaService
{
    private readonly ICulturaRepository _culturaRepository;
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly ILogger<CulturaService> _logger;

    public CulturaService(
        ICulturaRepository culturaRepository,
        ITalhaoRepository talhaoRepository,
        IPropriedadeRepository propriedadeRepository,
        ILogger<CulturaService> logger)
    {
        _culturaRepository = culturaRepository;
        _talhaoRepository = talhaoRepository;
        _propriedadeRepository = propriedadeRepository;
        _logger = logger;
    }

    public async Task<CulturaDto> CriarAsync(CriarCulturaDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(dto.TalhaoId, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {dto.TalhaoId} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para criar cultura neste talhão");

        if (dto.AreaPlantada > talhao.Area)
            throw new InvalidOperationException($"Área plantada ({dto.AreaPlantada}ha) excede área do talhão ({talhao.Area}ha)");

        var cultura = new Cultura(
            dto.TalhaoId,
            dto.Tipo,
            dto.Variedade,
            dto.AreaPlantada,
            dto.DataPlantio,
            dto.DataColheitaPrevista,
            dto.ProducaoEstimada,
            dto.Observacoes
        );

        await _culturaRepository.AdicionarAsync(cultura, cancellationToken);

        talhao.MarcarComoEmUso();
        await _talhaoRepository.AtualizarAsync(talhao, cancellationToken);

        _logger.LogInformation("Cultura {Tipo} criada com sucesso no talhão {TalhaoId}", dto.Tipo, dto.TalhaoId);

        return MapToDto(cultura);
    }

    public async Task<CulturaDto> ObterPorIdAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var cultura = await _culturaRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (cultura == null)
            throw new KeyNotFoundException($"Cultura com ID {id} não encontrada");

        if (!ehAdmin && cultura.Talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar esta cultura");

        return MapToDto(cultura);
    }

    public async Task<List<CulturaDto>> ObterPorTalhaoAsync(Guid talhaoId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(talhaoId, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {talhaoId} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar as culturas deste talhão");

        var culturas = await _culturaRepository.ObterPorTalhaoIdAsync(talhaoId, cancellationToken);
        return culturas.Select(MapToDto).ToList();
    }

    public async Task<List<CulturaDto>> ObterPorPropriedadeAsync(Guid propriedadeId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(propriedadeId, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {propriedadeId} não encontrada");

        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar as culturas desta propriedade");

        var culturas = await _culturaRepository.ObterPorPropriedadeIdAsync(propriedadeId, cancellationToken);
        return culturas.Select(MapToDto).ToList();
    }

    public async Task<List<CulturaDto>> ObterAtivasAsync(Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var culturas = await _culturaRepository.ObterAtivasAsync(cancellationToken);
        
        if (!ehAdmin)
        {
            culturas = culturas.Where(c => c.Talhao.Propriedade.ProprietarioId == usuarioId).ToList();
        }
        
        return culturas.Select(MapToDto).ToList();
    }

    public async Task<CulturaDto> AtualizarAsync(Guid id, AtualizarCulturaDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var cultura = await _culturaRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (cultura == null)
            throw new KeyNotFoundException($"Cultura com ID {id} não encontrada");

        if (!ehAdmin && cultura.Talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar esta cultura");

        cultura.Atualizar(
            dto.Tipo,
            dto.Variedade,
            dto.AreaPlantada,
            dto.DataPlantio,
            dto.DataColheitaPrevista,
            dto.ProducaoEstimada,
            dto.Observacoes
        );

        await _culturaRepository.AtualizarAsync(cultura, cancellationToken);

        _logger.LogInformation("Cultura {Id} atualizada com sucesso", id);

        return MapToDto(cultura);
    }

    public async Task<CulturaDto> RegistrarColheitaAsync(Guid id, RegistrarColheitaDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var cultura = await _culturaRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (cultura == null)
            throw new KeyNotFoundException($"Cultura com ID {id} não encontrada");

        if (!ehAdmin && cultura.Talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para registrar colheita nesta cultura");

        cultura.RegistrarColheita(dto.DataColheita, dto.ProducaoReal, dto.Observacoes);
        await _culturaRepository.AtualizarAsync(cultura, cancellationToken);

        _logger.LogInformation("Colheita registrada para cultura {Id}. Produção: {Producao}t", id, dto.ProducaoReal);

        return MapToDto(cultura);
    }

    public async Task CancelarAsync(Guid id, string motivo, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var cultura = await _culturaRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (cultura == null)
            throw new KeyNotFoundException($"Cultura com ID {id} não encontrada");

        if (!ehAdmin && cultura.Talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para cancelar esta cultura");

        cultura.Cancelar(motivo);
        await _culturaRepository.AtualizarAsync(cultura, cancellationToken);

        _logger.LogInformation("Cultura {Id} cancelada. Motivo: {Motivo}", id, motivo);
    }

    public async Task RemoverAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var cultura = await _culturaRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (cultura == null)
            throw new KeyNotFoundException($"Cultura com ID {id} não encontrada");

        if (!ehAdmin && cultura.Talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para remover esta cultura");

        await _culturaRepository.RemoverAsync(id, cancellationToken);

        _logger.LogInformation("Cultura {Id} removida com sucesso", id);
    }

    private static CulturaDto MapToDto(Cultura cultura)
    {
        return new CulturaDto(
            cultura.Id,
            cultura.TalhaoId,
            cultura.Tipo,
            cultura.Variedade,
            cultura.AreaPlantada,
            cultura.DataPlantio,
            cultura.DataColheitaPrevista,
            cultura.DataColheitaRealizada,
            cultura.ProducaoEstimada,
            cultura.ProducaoReal,
            cultura.CalcularProdutividade(),
            cultura.Observacoes,
            cultura.Status,
            cultura.DataCadastro,
            cultura.DataAtualizacao
        );
    }
}
