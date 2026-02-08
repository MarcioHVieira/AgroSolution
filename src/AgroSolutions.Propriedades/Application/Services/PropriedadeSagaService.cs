using AgroSolutions.Propriedades.Application.Sagas;
using AgroSolutions.Propriedades.Application.Sagas.Steps;
using AgroSolutions.SharedKernel.Sagas;

namespace AgroSolutions.Propriedades.Application.Services;

/// <summary>
/// Serviço que orquestra a criação completa de propriedade usando Saga Pattern
/// </summary>
public class PropriedadeSagaService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PropriedadeSagaService> _logger;

    public PropriedadeSagaService(
        IServiceProvider serviceProvider,
        ILogger<PropriedadeSagaService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Cria propriedade completa com talhões usando Saga Pattern
    /// Garante consistência distribuída com rollback automático em caso de falha
    /// </summary>
    public async Task<SagaExecutionResult> CriarPropriedadeCompletaAsync(
        CriarPropriedadeCompletaDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Iniciando criação de propriedade completa via Saga: {Nome} com {TalhoesCount} talhões",
            dto.Nome,
            dto.Talhoes.Count);

        using var scope = _serviceProvider.CreateScope();

        // Resolver os passos da saga
        var criarPropriedadeStep = scope.ServiceProvider.GetRequiredService<CriarPropriedadeStep>();
        var criarTalhoesStep = scope.ServiceProvider.GetRequiredService<CriarTalhoesStep>();
        var publicarEventoStep = scope.ServiceProvider.GetRequiredService<PublicarEventoPropriedadeCriadaStep>();

        // Criar orquestrador e adicionar passos
        var orchestrator = new SagaOrchestrator<CriarPropriedadeCompletaDto>(
            scope.ServiceProvider.GetRequiredService<ILogger<SagaOrchestrator<CriarPropriedadeCompletaDto>>>())
            .AddStep(criarPropriedadeStep)      // Passo 1: Criar propriedade
            .AddStep(criarTalhoesStep)          // Passo 2: Criar talhões
            .AddStep(publicarEventoStep);       // Passo 3: Publicar evento

        // Executar saga
        var result = await orchestrator.ExecuteAsync(dto, cancellationToken);

        if (result.Success)
        {
            _logger.LogInformation(
                "Propriedade completa criada com sucesso via Saga. PropriedadeId: {PropriedadeId}",
                dto.PropriedadeId);
        }
        else
        {
            _logger.LogError(
                "Falha ao criar propriedade completa via Saga: {ErrorMessage}",
                result.ErrorMessage);
        }

        return result;
    }
}
