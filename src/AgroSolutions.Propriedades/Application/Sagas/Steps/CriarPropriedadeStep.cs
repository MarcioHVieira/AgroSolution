using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.SharedKernel.Sagas;

namespace AgroSolutions.Propriedades.Application.Sagas.Steps;

/// <summary>
/// Passo 1: Criar a propriedade
/// </summary>
public class CriarPropriedadeStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    private readonly IPropriedadeRepository _repository;
    private readonly ILogger<CriarPropriedadeStep> _logger;

    public CriarPropriedadeStep(
        IPropriedadeRepository repository,
        ILogger<CriarPropriedadeStep> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SagaStepResult> ExecuteAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Criando propriedade: {Nome}", data.Nome);

            var propriedade = new Propriedade(
                data.ProprietarioId,
                data.Nome,
                data.AreaTotal,
                TipoPropriedade.Fazenda,
                "00000-000",
                "Endereço padrão",
                "Bairro padrão",
                "Cidade padrão",
                "Estado padrão",
                data.Descricao
            );

            await _repository.AdicionarAsync(propriedade, cancellationToken);

            data.PropriedadeId = propriedade.Id;

            _logger.LogInformation(
                "Propriedade criada com sucesso. ID: {PropriedadeId}",
                propriedade.Id);

            return SagaStepResult.Ok(new Dictionary<string, object>
            {
                ["PropriedadeId"] = propriedade.Id
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar propriedade: {Message}", ex.Message);
            return SagaStepResult.Fail($"Erro ao criar propriedade: {ex.Message}");
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
                    "Compensando: Excluindo propriedade {PropriedadeId}",
                    data.PropriedadeId);

                var propriedade = await _repository.ObterPorIdAsync(
                    data.PropriedadeId.Value,
                    cancellationToken);

                if (propriedade != null)
                {
                    await _repository.RemoverAsync(propriedade.Id, cancellationToken);
                    _logger.LogInformation("Propriedade excluída com sucesso (compensação)");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao compensar criação de propriedade: {Message}", ex.Message);
                throw;
            }
        }
    }
}
