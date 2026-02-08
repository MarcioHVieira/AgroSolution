using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Events;
using AgroSolutions.Propriedades.Application.Interfaces;
using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Data;
using AgroSolutions.SharedKernel.Messaging;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Propriedades.Application.Services;

public class TalhaoService : ITalhaoService
{
    private readonly ITalhaoRepository _talhaoRepository;
    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<TalhaoService> _logger;
    private readonly PropriedadesDbContext _context;

    public TalhaoService(
        ITalhaoRepository talhaoRepository,
        IPropriedadeRepository propriedadeRepository,
        IRabbitMQPublisher publisher,
        ILogger<TalhaoService> logger,
        PropriedadesDbContext context)
    {
        _talhaoRepository = talhaoRepository;
        _propriedadeRepository = propriedadeRepository;
        _publisher = publisher;
        _logger = logger;
        _context = context;
    }

    public async Task<TalhaoDto> CriarAsync(CriarTalhaoDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(dto.PropriedadeId, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {dto.PropriedadeId} não encontrada");

        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para criar talhão nesta propriedade");

        if (!propriedade.PossuiAreaDisponivel(dto.Area))
            throw new InvalidOperationException($"Área disponível insuficiente. Disponível: {propriedade.CalcularAreaDisponivel()}ha, Solicitado: {dto.Area}ha");

        var talhao = new Talhao(
            dto.PropriedadeId,
            dto.Nome,
            dto.Area,
            dto.Descricao,
            dto.Latitude,
            dto.Longitude,
            dto.Poligono
        );

        await _talhaoRepository.AdicionarAsync(talhao, cancellationToken);

        _logger.LogInformation("Talhão {Nome} criado com sucesso na propriedade {PropriedadeId}", dto.Nome, dto.PropriedadeId);

        // Buscar dados do proprietário do Read Model local
        var (emailProprietario, nomeProprietario) = await ObterDadosProprietarioAsync(propriedade.ProprietarioId);

        // Publicar evento para sincronizar outros microserviços (Event-Driven Architecture)
        await _publisher.PublishAsync(new TalhaoCriadoEvent(
            TalhaoId: talhao.Id,
            PropriedadeId: talhao.PropriedadeId,
            Nome: talhao.Nome,
            AreaHectares: talhao.Area,
            Cultura: "N/A",
            Status: talhao.Status.ToString(),
            DataCriacao: talhao.DataCadastro,
            ProprietarioId: propriedade.ProprietarioId,
            EmailProprietario: emailProprietario,
            NomeProprietario: nomeProprietario
        ), "talhao.criado");

        return MapToDto(talhao);
    }

    public async Task<TalhaoDto> ObterPorIdAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {id} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar este talhão");

        return MapToDto(talhao);
    }

    public async Task<List<TalhaoDto>> ObterPorPropriedadeAsync(Guid propriedadeId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(propriedadeId, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {propriedadeId} não encontrada");

        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar os talhões desta propriedade");

        var talhoes = await _talhaoRepository.ObterPorPropriedadeIdAsync(propriedadeId, cancellationToken);
        return talhoes.Select(MapToDto).ToList();
    }

    public async Task<List<TalhaoDto>> ObterDisponiveisAsync(Guid propriedadeId, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(propriedadeId, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {propriedadeId} não encontrada");

        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar os talhões desta propriedade");

        var talhoes = await _talhaoRepository.ObterDisponiveisPorPropriedadeIdAsync(propriedadeId, cancellationToken);
        return talhoes.Select(MapToDto).ToList();
    }

    public async Task<TalhaoDto> AtualizarAsync(Guid id, AtualizarTalhaoDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {id} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar este talhão");

        talhao.Atualizar(
            dto.Nome,
            dto.Area,
            dto.Descricao,
            dto.Latitude,
            dto.Longitude,
            dto.Poligono
        );

        await _talhaoRepository.AtualizarAsync(talhao, cancellationToken);

        _logger.LogInformation("Talhão {Id} atualizado com sucesso", id);

        // Buscar dados do proprietário do Read Model local
        var (emailProprietario, nomeProprietario) = await ObterDadosProprietarioAsync(talhao.Propriedade.ProprietarioId);

        // Publicar evento para sincronizar outros microserviços
        await _publisher.PublishAsync(new TalhaoAtualizadoEvent(
            TalhaoId: talhao.Id,
            PropriedadeId: talhao.PropriedadeId,
            Nome: talhao.Nome,
            AreaHectares: talhao.Area,
            Cultura: "N/A",
            Status: talhao.Status.ToString(),
            DataAtualizacao: DateTime.UtcNow,
            ProprietarioId: talhao.Propriedade.ProprietarioId,
            EmailProprietario: emailProprietario,
            NomeProprietario: nomeProprietario
        ), "talhao.atualizado");

        return MapToDto(talhao);
    }

    public async Task MarcarComoEmUsoAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {id} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para modificar este talhão");

        talhao.MarcarComoEmUso();
        await _talhaoRepository.AtualizarAsync(talhao, cancellationToken);

        _logger.LogInformation("Talhão {Id} marcado como em uso", id);

        // Buscar dados do proprietário do Read Model local
        var (emailProprietario, nomeProprietario) = await ObterDadosProprietarioAsync(talhao.Propriedade.ProprietarioId);

        // Publicar evento de atualização
        await _publisher.PublishAsync(new TalhaoAtualizadoEvent(
            TalhaoId: talhao.Id,
            PropriedadeId: talhao.PropriedadeId,
            Nome: talhao.Nome,
            AreaHectares: talhao.Area,
            Cultura: "N/A",
            Status: talhao.Status.ToString(),
            DataAtualizacao: DateTime.UtcNow,
            ProprietarioId: talhao.Propriedade.ProprietarioId,
            EmailProprietario: emailProprietario,
            NomeProprietario: nomeProprietario
        ), "talhao.atualizado");
    }

    public async Task MarcarComoDisponivelAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {id} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para modificar este talhão");

        talhao.MarcarComoDisponivel();
        await _talhaoRepository.AtualizarAsync(talhao, cancellationToken);

        _logger.LogInformation("Talhão {Id} marcado como disponível", id);

        // Buscar dados do proprietário do Read Model local
        var (emailProprietario, nomeProprietario) = await ObterDadosProprietarioAsync(talhao.Propriedade.ProprietarioId);

        // Publicar evento de atualização
        await _publisher.PublishAsync(new TalhaoAtualizadoEvent(
            TalhaoId: talhao.Id,
            PropriedadeId: talhao.PropriedadeId,
            Nome: talhao.Nome,
            AreaHectares: talhao.Area,
            Cultura: "N/A",
            Status: talhao.Status.ToString(),
            DataAtualizacao: DateTime.UtcNow,
            ProprietarioId: talhao.Propriedade.ProprietarioId,
            EmailProprietario: emailProprietario,
            NomeProprietario: nomeProprietario
        ), "talhao.atualizado");
    }

    public async Task MarcarComoEmDescansoAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {id} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para modificar este talhão");

        talhao.MarcarComoEmDescanso();
        await _talhaoRepository.AtualizarAsync(talhao, cancellationToken);

        _logger.LogInformation("Talhão {Id} marcado como em descanso", id);

        // Buscar dados do proprietário do Read Model local
        var (emailProprietario, nomeProprietario) = await ObterDadosProprietarioAsync(talhao.Propriedade.ProprietarioId);

        // Publicar evento de atualização
        await _publisher.PublishAsync(new TalhaoAtualizadoEvent(
            TalhaoId: talhao.Id,
            PropriedadeId: talhao.PropriedadeId,
            Nome: talhao.Nome,
            AreaHectares: talhao.Area,
            Cultura: "N/A",
            Status: talhao.Status.ToString(),
            DataAtualizacao: DateTime.UtcNow,
            ProprietarioId: talhao.Propriedade.ProprietarioId,
            EmailProprietario: emailProprietario,
            NomeProprietario: nomeProprietario
        ), "talhao.atualizado");
    }

    public async Task RemoverAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var talhao = await _talhaoRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (talhao == null)
            throw new KeyNotFoundException($"Talhão com ID {id} não encontrado");

        if (!ehAdmin && talhao.Propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para remover este talhão");

        if (talhao.Culturas.Any())
            throw new InvalidOperationException("Não é possível remover um talhão que possui culturas cadastradas");

        await _talhaoRepository.RemoverAsync(id, cancellationToken);

        _logger.LogInformation("Talhão {Id} removido com sucesso", id);
    }

    private static TalhaoDto MapToDto(Talhao talhao)
    {
        return new TalhaoDto(
            talhao.Id,
            talhao.PropriedadeId,
            talhao.Nome,
            talhao.Descricao,
            talhao.Area,
            talhao.Latitude,
            talhao.Longitude,
            talhao.Poligono,
            talhao.Status,
            talhao.PossuiCulturaAtiva(),
            talhao.Culturas.Count,
            talhao.DataCadastro,
            talhao.DataAtualizacao
        );
    }

    /// <summary>
    /// Busca dados do proprietário do Read Model local (sincronizado via eventos do Identidade)
    /// </summary>
    private async Task<(string Email, string Nome)> ObterDadosProprietarioAsync(Guid proprietarioId)
    {
        var usuario = await _context.UsuariosInfo
            .FirstOrDefaultAsync(u => u.Id == proprietarioId);
        
        if (usuario != null)
        {
            return (usuario.Email, usuario.NomeCompleto);
        }

        // Fallback: se não encontrou no Read Model, retorna valores genéricos
        _logger.LogWarning("Usuário {UsuarioId} não encontrado no Read Model local. Usando valores genéricos.", proprietarioId);
        return ("proprietario@agrosolutions.com", "Proprietário");
    }
}

