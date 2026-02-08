using AgroSolutions.Propriedades.Application.Interfaces;
using AgroSolutions.SharedKernel.Sagas;

namespace AgroSolutions.Propriedades.Application.Sagas.Steps;

/// <summary>
/// Passo 3: Enviar evento de propriedade criada (para criar sensores em outro microsserviço)
/// </summary>
public class PublicarEventoPropriedadeCriadaStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    private readonly IMessageBusPublisher _messageBusPublisher;
    private readonly ILogger<PublicarEventoPropriedadeCriadaStep> _logger;

    public PublicarEventoPropriedadeCriadaStep(
        IMessageBusPublisher messageBusPublisher,
        ILogger<PublicarEventoPropriedadeCriadaStep> logger)
    {
        _messageBusPublisher = messageBusPublisher;
        _logger = logger;
    }

    public async Task<SagaStepResult> ExecuteAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken = default)
    {
        if (!data.PropriedadeId.HasValue)
        {
            return SagaStepResult.Fail("PropriedadeId não foi definida");
        }

        try
        {
            _logger.LogInformation(
                "Publicando evento PropriedadeCriada para propriedade {PropriedadeId}",
                data.PropriedadeId);

            var evento = new PropriedadeCriadaEvent
            {
                PropriedadeId = data.PropriedadeId.Value,
                Nome = data.Nome,
                ProprietarioId = data.ProprietarioId,
                TalhoesIds = data.TalhoesIds,
                DataCriacao = DateTime.UtcNow
            };

            await _messageBusPublisher.PublishAsync(evento, cancellationToken);

            _logger.LogInformation("Evento PropriedadeCriada publicado com sucesso");

            return SagaStepResult.Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao publicar evento: {Message}", ex.Message);
            return SagaStepResult.Fail($"Erro ao publicar evento: {ex.Message}");
        }
    }

    public async Task CompensateAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken = default)
    {
        if (data.PropriedadeId.HasValue)
        {
            try
            {
                _logger.LogWarning(
                    "Compensando: Publicando evento PropriedadeExcluida para {PropriedadeId}",
                    data.PropriedadeId);

                var eventoCompensacao = new PropriedadeExcluidaEvent
                {
                    PropriedadeId = data.PropriedadeId.Value,
                    DataExclusao = DateTime.UtcNow
                };

                await _messageBusPublisher.PublishAsync(eventoCompensacao, cancellationToken);

                _logger.LogInformation("Evento de compensação publicado com sucesso");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao compensar publicação de evento: {Message}", ex.Message);
                throw;
            }
        }

        await Task.CompletedTask;
    }
}

// Eventos
public record PropriedadeCriadaEvent
{
    public Guid PropriedadeId { get; init; }
    public string Nome { get; init; } = string.Empty;
    public Guid ProprietarioId { get; init; }
    public List<Guid> TalhoesIds { get; init; } = new();
    public DateTime DataCriacao { get; init; }
}

public record PropriedadeExcluidaEvent
{
    public Guid PropriedadeId { get; init; }
    public DateTime DataExclusao { get; init; }
}
