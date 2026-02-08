using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.SharedKernel.Sagas;

namespace AgroSolutions.Propriedades.Application.Sagas.Steps;

/// <summary>
/// Passo 2: Criar os talhões da propriedade
/// </summary>
public class CriarTalhoesStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    private readonly ITalhaoRepository _repository;
    private readonly ILogger<CriarTalhoesStep> _logger;

    public CriarTalhoesStep(
        ITalhaoRepository repository,
        ILogger<CriarTalhoesStep> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task<SagaStepResult> ExecuteAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken = default)
    {
        if (!data.PropriedadeId.HasValue)
        {
            return SagaStepResult.Fail("PropriedadeId não foi definida no passo anterior");
        }

        try
        {
            _logger.LogInformation(
                "Criando {Count} talhões para propriedade {PropriedadeId}",
                data.Talhoes.Count,
                data.PropriedadeId);

            foreach (var talhaoDto in data.Talhoes)
            {
                var talhao = new Talhao(
                    data.PropriedadeId.Value,
                    talhaoDto.Nome,
                    talhaoDto.Area
                );

                await _repository.AdicionarAsync(talhao, cancellationToken);
                data.TalhoesIds.Add(talhao.Id);

                _logger.LogInformation("Talhão {Nome} criado. ID: {TalhaoId}", talhaoDto.Nome, talhao.Id);
            }

            _logger.LogInformation("Todos os talhões foram criados com sucesso");

            return SagaStepResult.Ok(new Dictionary<string, object>
            {
                ["TalhoesIds"] = data.TalhoesIds
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar talhões: {Message}", ex.Message);
            return SagaStepResult.Fail($"Erro ao criar talhões: {ex.Message}");
        }
    }

    public async Task CompensateAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken = default)
    {
        if (data.TalhoesIds.Any())
        {
            try
            {
                _logger.LogWarning(
                    "Compensando: Excluindo {Count} talhões",
                    data.TalhoesIds.Count);

                foreach (var talhaoId in data.TalhoesIds)
                {
                    var talhao = await _repository.ObterPorIdAsync(talhaoId, cancellationToken);

                    if (talhao != null)
                    {
                        await _repository.RemoverAsync(talhao.Id, cancellationToken);
                        _logger.LogInformation("Talhão {TalhaoId} excluído (compensação)", talhaoId);
                    }
                }

                _logger.LogInformation("Todos os talhões foram excluídos (compensação)");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao compensar criação de talhões: {Message}", ex.Message);
                throw;
            }
        }
    }
}
