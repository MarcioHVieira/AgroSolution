using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Events;
using AgroSolutions.Propriedades.Application.Interfaces;
using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;

namespace AgroSolutions.Propriedades.Application.Services;

public class PropriedadeService : IPropriedadeService
{
    private readonly IPropriedadeRepository _propriedadeRepository;
    private readonly IUsuarioInfoRepository _usuarioInfoRepository;
    private readonly IRabbitMQPublisher _publisher;
    private readonly ILogger<PropriedadeService> _logger;

    public PropriedadeService(
        IPropriedadeRepository propriedadeRepository,
        IUsuarioInfoRepository usuarioInfoRepository,
        IRabbitMQPublisher publisher,
        ILogger<PropriedadeService> logger)
    {
        _propriedadeRepository = propriedadeRepository;
        _usuarioInfoRepository = usuarioInfoRepository;
        _publisher = publisher;
        _logger = logger;
    }

    public async Task<PropriedadeDto> CriarAsync(Guid proprietarioId, CriarPropriedadeDto dto, CancellationToken cancellationToken = default)
    {
        var propriedade = new Propriedade(
            proprietarioId,
            dto.Nome,
            dto.AreaTotal,
            dto.Tipo,
            dto.Cep,
            dto.Endereco,
            dto.Bairro,
            dto.Cidade,
            dto.Estado,
            dto.Descricao,
            dto.Numero,
            dto.Complemento,
            dto.Latitude,
            dto.Longitude
        );

        await _propriedadeRepository.AdicionarAsync(propriedade, cancellationToken);

        _logger.LogInformation("Propriedade {Nome} criada com sucesso para o proprietário {ProprietarioId}", dto.Nome, proprietarioId);

        // Buscar dados do proprietário do Read Model local
        var (emailProprietario, nomeProprietario) = await ObterDadosProprietarioAsync(proprietarioId);

        // Publicar evento PropriedadeCriada
        await _publisher.PublishAsync(new PropriedadeCriadaEvent(
            PropriedadeId: propriedade.Id,
            Nome: propriedade.Nome,
            Endereco: $"{propriedade.Endereco}, {propriedade.Cidade}/{propriedade.Estado}",
            AreaTotal: propriedade.AreaTotal,
            ProprietarioId: propriedade.ProprietarioId,
            DataCriacao: propriedade.DataCadastro,
            EmailProprietario: emailProprietario,
            NomeProprietario: nomeProprietario
        ), "propriedade.criada");

        return MapToDto(propriedade);
    }

    public async Task<PropriedadeDto> ObterPorIdAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {id} não encontrada");

        // Verifica se o usuário tem permissão para acessar esta propriedade
        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para acessar esta propriedade");

        return MapToDto(propriedade);
    }

    public async Task<List<PropriedadeDto>> ObterPorProprietarioAsync(Guid proprietarioId, CancellationToken cancellationToken = default)
    {
        var propriedades = await _propriedadeRepository.ObterPorProprietarioIdAsync(proprietarioId, cancellationToken);
        return propriedades.Select(MapToDto).ToList();
    }

    public async Task<List<PropriedadeDto>> ObterTodasAsync(Guid usuarioId, bool ehAdmin, int pagina = 1, int tamanhoPagina = 20, CancellationToken cancellationToken = default)
    {
        // Admin vê todas, usuário comum vê apenas as suas
        if (ehAdmin)
        {
            var todasPropriedades = await _propriedadeRepository.ObterTodasAsync(pagina, tamanhoPagina, cancellationToken);
            return todasPropriedades.Select(MapToDto).ToList();
        }
        else
        {
            var propriedadesUsuario = await _propriedadeRepository.ObterPorProprietarioIdAsync(usuarioId, cancellationToken);
            return propriedadesUsuario.Select(MapToDto).ToList();
        }
    }

    public async Task<PropriedadeDto> AtualizarAsync(Guid id, AtualizarPropriedadeDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {id} não encontrada");

        // Verifica permissão
        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar esta propriedade");

        propriedade.Atualizar(
            dto.Nome,
            dto.AreaTotal,
            dto.Tipo,
            dto.Descricao,
            dto.Latitude,
            dto.Longitude
        );

        await _propriedadeRepository.AtualizarAsync(propriedade, cancellationToken);

        _logger.LogInformation("Propriedade {Id} atualizada com sucesso", id);

        return MapToDto(propriedade);
    }

    public async Task<PropriedadeDto> AtualizarEnderecoAsync(Guid id, AtualizarEnderecoPropriedadeDto dto, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {id} não encontrada");

        // Verifica permissão
        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para atualizar esta propriedade");

        propriedade.AtualizarEndereco(
            dto.Cep,
            dto.Endereco,
            dto.Bairro,
            dto.Cidade,
            dto.Estado,
            dto.Numero,
            dto.Complemento
        );

        await _propriedadeRepository.AtualizarAsync(propriedade, cancellationToken);

        _logger.LogInformation("Endereço da propriedade {Id} atualizado com sucesso", id);

        return MapToDto(propriedade);
    }

    public async Task AtivarAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {id} não encontrada");

        // Verifica permissão
        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para ativar esta propriedade");

        propriedade.Ativar();
        await _propriedadeRepository.AtualizarAsync(propriedade, cancellationToken);

        _logger.LogInformation("Propriedade {Id} ativada com sucesso", id);
    }

    public async Task InativarAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {id} não encontrada");

        // Verifica permissão
        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para inativar esta propriedade");

        propriedade.Inativar();
        await _propriedadeRepository.AtualizarAsync(propriedade, cancellationToken);

        _logger.LogInformation("Propriedade {Id} inativada com sucesso", id);
    }

    public async Task RemoverAsync(Guid id, Guid usuarioId, bool ehAdmin, CancellationToken cancellationToken = default)
    {
        var propriedade = await _propriedadeRepository.ObterPorIdAsync(id, cancellationToken);
        
        if (propriedade == null)
            throw new KeyNotFoundException($"Propriedade com ID {id} não encontrada");

        // Verifica permissão
        if (!ehAdmin && propriedade.ProprietarioId != usuarioId)
            throw new UnauthorizedAccessException("Você não tem permissão para remover esta propriedade");

        if (propriedade.Talhoes.Any())
            throw new InvalidOperationException("Não é possível remover uma propriedade que possui talhões cadastrados");

        await _propriedadeRepository.RemoverAsync(id, cancellationToken);

        _logger.LogInformation("Propriedade {Id} removida com sucesso", id);
    }

    private static PropriedadeDto MapToDto(Propriedade propriedade)
    {
        return new PropriedadeDto(
            propriedade.Id,
            propriedade.ProprietarioId,
            propriedade.Nome,
            propriedade.Descricao,
            propriedade.AreaTotal,
            propriedade.CalcularAreaDisponivel(),
            propriedade.Tipo,
            propriedade.Cep,
            propriedade.Endereco,
            propriedade.Numero,
            propriedade.Complemento,
            propriedade.Bairro,
            propriedade.Cidade,
            propriedade.Estado,
            propriedade.Latitude,
            propriedade.Longitude,
            propriedade.Status,
            propriedade.Talhoes.Count,
            propriedade.DataCadastro,
            propriedade.DataAtualizacao
        );
    }

    /// <summary>
    /// Busca dados do proprietário do Read Model local (sincronizado via eventos do Identidade)
    /// </summary>
    private async Task<(string Email, string Nome)> ObterDadosProprietarioAsync(Guid proprietarioId)
    {
        var dadosUsuario = await _usuarioInfoRepository.ObterDadosUsuarioAsync(proprietarioId);
        
        if (dadosUsuario.HasValue)
        {
            return (dadosUsuario.Value.Email, dadosUsuario.Value.NomeCompleto);
        }

        _logger.LogWarning("Usuário {UsuarioId} não encontrado no Read Model local. Usando valores genéricos.", proprietarioId);
        return ("proprietario@agrosolutions.com", "Proprietário");
    }
}

