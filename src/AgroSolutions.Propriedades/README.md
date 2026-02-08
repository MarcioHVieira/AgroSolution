# 🏡 AgroSolutions.Propriedades

## 🎯 Visão Geral

O microserviço de **Propriedades** é responsável pela gestão completa de propriedades rurais, talhões e culturas na plataforma AgroSolutions. Implementa o padrão Saga para transações distribuídas e mantém sincronização de dados de usuários através de eventos.

## 📋 Responsabilidades

- **Gestão de Propriedades**: CRUD completo de propriedades rurais
- **Gestão de Talhões**: Divisões de terra dentro das propriedades
- **Gestão de Culturas**: Catálogo de culturas agrícolas
- **Saga Orchestration**: Transações distribuídas com compensação
- **Event Publishing**: Publicação de eventos de domínio
- **User Synchronization**: Sincronização de dados de usuários via eventos

## 🏗️ Arquitetura

### Estrutura de Diretórios

```
AgroSolutions.Propriedades/
├── API/
│   ├── Controllers/
│   │   ├── CulturasController.cs                      # Endpoints de culturas
│   │   ├── PropriedadesController.cs                  # Endpoints de propriedades
│   │   └── TalhoesController.cs                       # Endpoints de talhões
│   └── Extensions/
│       └── UserContextExtensions.cs                   # Extensões de contexto de usuário
├── Application/
│   ├── DTOs/
│   │   └── PropriedadesDtos.cs                        # Data Transfer Objects
│   ├── Events/
│   │   ├── PropriedadeAtualizadaEvent.cs              # Evento de propriedade atualizada
│   │   ├── PropriedadeCriadaEvent.cs                  # Evento de propriedade criada
│   │   ├── TalhaoAtualizadoEvent.cs                   # Evento de talhão atualizado
│   │   └── TalhaoCriadoEvent.cs                       # Evento de talhão criado
│   ├── Interfaces/
│   │   ├── ICulturaService.cs                         # Interface do serviço de culturas
│   │   ├── IMessageBusPublisher.cs                    # Interface de publicação
│   │   ├── IPropriedadeService.cs                     # Interface do serviço de propriedades
│   │   └── ITalhaoService.cs                          # Interface do serviço de talhões
│   ├── Sagas/
│   │   ├── CriarPropriedadeCompletaDto.cs             # DTO da saga
│   │   ├── PropriedadeSagaService.cs                  # Serviço orquestrador
│   │   └── Steps/
│   │       ├── CriarPropriedadeStep.cs                # Passo 1: Criar propriedade
│   │       ├── CriarTalhoesStep.cs                    # Passo 2: Criar talhões
│   │       └── PublicarEventoPropriedadeCriadaStep.cs # Passo 3: Publicar evento
│   ├── Services/
│   │   ├── CulturaService.cs                          # Implementação culturas
│   │   ├── PropriedadeSagaService.cs                  # Implementação saga
│   │   ├── PropriedadeService.cs                      # Implementação propriedades
│   │   └── TalhaoService.cs                           # Implementação talhões
│   └── Validators/
│       └── PropriedadesValidators.cs                  # Validações FluentValidation
├── Configuration/
│   ├── ApiConfiguration.cs                            # Configuração de API
│   ├── ApiDocumentationConfiguration.cs               # OpenAPI + Scalar
│   ├── DependencyInjectionConfiguration.cs            # Injeção de dependências
│   └── MonitoringConfiguration.cs                     # Observabilidade
├── Domain/
│   ├── Entities/
│   │   ├── Cultura.cs                                 # Entidade cultura
│   │   ├── Propriedade.cs                             # Entidade propriedade
│   │   ├── Talhao.cs                                  # Entidade talhão
│   │   └── UsuarioInfo.cs                             # Cache de dados de usuário
│   ├── Enums/
│   │   └── Enums.cs                                   # Enumerações do domínio
│   └── Interfaces/
│       ├── ICulturaRepository.cs                      # Repositório de culturas
│       ├── IPropriedadeRepository.cs                  # Repositório de propriedades
│       ├── ITalhaoRepository.cs                       # Repositório de talhões
│       └── IUsuarioInfoRepository.cs                  # Repositório de usuários
├── Infrastructure/
│   ├── Data/
│   │   ├── DatabaseMigrator.cs                        # Migrator customizado
│   │   └── PropriedadesDbContext.cs                   # DbContext
│   ├── Metrics/
│   │   └── PropriedadesMetrics.cs                     # Métricas Prometheus
│   ├── Repositories/
│   │   ├── CulturaRepository.cs                       # Implementação culturas
│   │   ├── PropriedadeRepository.cs                   # Implementação propriedades
│   │   ├── TalhaoRepository.cs                        # Implementação talhões
│   │   └── UsuarioInfoRepository.cs                   # Implementação usuários
│   └── Services/
│       └── UsuarioSyncConsumerService.cs              # Consumer de eventos de usuário
└── Program.cs
```

## 💻 Tecnologias e Técnicas Aplicadas

### Core
- **.NET 10** - Framework
- **C# 14** - Linguagem
- **ASP.NET Core** - Web API

### Banco de Dados
- **Entity Framework Core 10** - ORM
- **SQL Server** - Banco de dados principal (suporte também para PostgreSQL)
- **Database per Service** - Isolamento de dados

### Mensageria
- **RabbitMQ** - Message broker
- **Event-Driven Architecture** - Comunicação assíncrona
- **Consumer Services** - Consumo de eventos em background

### Observabilidade
- **Prometheus** - Métricas customizadas
- **Health Checks** - Monitoramento de saúde

### Validação
- **FluentValidation** - Validações fluentes

## 🎨 Design Patterns Aplicados

### Arquiteturais

#### 1. **Saga Pattern (Orchestration)**
Orquestração de transações distribuídas para criação completa de propriedade:

```csharp
public class PropriedadeSagaService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<PropriedadeSagaService> _logger;

    public async Task<SagaExecutionResult> CriarPropriedadeCompletaAsync(
        CriarPropriedadeCompletaDto dto,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "Iniciando criação de propriedade completa via Saga: {Nome} com {TalhoesCount} talhões",
            dto.Nome,
            dto.Talhoes.Count);

        using var scope = _serviceProvider.CreateScope();

        // Resolver os passos da saga via Dependency Injection
        var criarPropriedadeStep = scope.ServiceProvider.GetRequiredService<CriarPropriedadeStep>();
        var criarTalhoesStep = scope.ServiceProvider.GetRequiredService<CriarTalhoesStep>();
        var publicarEventoStep = scope.ServiceProvider.GetRequiredService<PublicarEventoPropriedadeCriadaStep>();

        // Criar orquestrador e adicionar passos
        var orchestrator = new SagaOrchestrator<CriarPropriedadeCompletaDto>(
            scope.ServiceProvider.GetRequiredService<ILogger<SagaOrchestrator<CriarPropriedadeCompletaDto>>>())
            .AddStep(criarPropriedadeStep)      // Passo 1
            .AddStep(criarTalhoesStep)          // Passo 2
            .AddStep(publicarEventoStep);       // Passo 3

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
```

**Passos da Saga:**
1. **CriarPropriedadeStep**: Persiste a propriedade no banco
2. **CriarTalhoesStep**: Persiste os talhões associados
3. **PublicarEventoPropriedadeCriadaStep**: Publica evento de integração

**Compensação Automática:**
- Se passo 2 falhar → Remove propriedade criada no passo 1
- Se passo 3 falhar → Remove talhões (passo 2) E propriedade (passo 1)

**Recursos Avançados:**
- ✅ **Dependency Injection**: Passos resolvidos via DI com scope próprio
- ✅ **Logging Estruturado**: Logs detalhados em cada etapa da saga
- ✅ **CancellationToken**: Suporte para cancelamento de operações
- ✅ **Rollback Automático**: Compensação executada automaticamente em falhas
- ✅ **Ordem Inversa de Compensação**: Rollback sempre em ordem reversa da execução
- ✅ **Compensação Resiliente**: Erros em compensação não impedem outros passos
- ✅ **Tratamento de Exceções**: Try-catch em cada passo com fallback para compensação

#### 2. **Event-Driven Architecture**
Comunicação assíncrona através de eventos de integração.

#### 3. **Clean Architecture**
Separação em camadas com dependências direcionadas ao domínio.

### Criacionais

**Nota**: O projeto utiliza construtores diretos para criação de entidades. Não há uso explícito de patterns criacionais como Factory ou Builder.

```csharp
// Exemplo real - construtor direto
public Propriedade(string nome, string endereco, decimal areaTotal, Guid proprietarioId)
{
    Id = Guid.NewGuid();
    Nome = nome;
    Endereco = endereco;
    AreaTotal = areaTotal;
    ProprietarioId = proprietarioId;
    DataCriacao = DateTime.UtcNow;
}
```

### Estruturais

#### 1. **Repository Pattern**
Abstração do acesso a dados:
```csharp
public interface IPropriedadeRepository
{
    Task<Propriedade?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Propriedade>> ObterPorProprietarioAsync(Guid proprietarioId, CancellationToken cancellationToken);
    Task<PaginatedResult<Propriedade>> ObterPaginadoAsync(int pageNumber, int pageSize, CancellationToken cancellationToken);
}
```

#### 2. **Adapter Pattern**
Adaptação de serviços externos (RabbitMQ):
```csharp
public interface IMessageBusPublisher
{
    Task PublishAsync<T>(T evento) where T : IntegrationEvent;
}
```

#### 3. **Aggregate Pattern (DDD)**
Propriedade como agregado raiz que controla Talhões:
```csharp
public class Propriedade
{
    private readonly List<Talhao> _talhoes = new();
    public IReadOnlyCollection<Talhao> Talhoes => _talhoes.AsReadOnly();
    
    public void AdicionarTalhao(Talhao talhao)
    {
        // Regras de negócio
        ValidarAreaTotal();
        _talhoes.Add(talhao);
    }
}
```

### Comportamentais

#### 1. **Observer Pattern**
Publicação e consumo de eventos:
```csharp
// Publicação
await _publisher.PublishAsync(
    new PropriedadeCriadaEvent(
        propriedade.Id,
        propriedade.Nome,
        proprietario.Email,
        proprietario.NomeCompleto
    )
);

// Consumo (UsuarioSyncConsumerService)
consumer.ReceivedAsync += async (model, ea) =>
{
    var evento = Deserialize<UsuarioCriadoEvent>(ea.Body);
    await SincronizarUsuario(evento);
};
```

### DDD Patterns

#### 1. **Aggregate Root**
```csharp
public class Propriedade : BaseEntity, IAggregateRoot
{
    // Propriedade controla ciclo de vida dos Talhões
    private List<Talhao> _talhoes = new();
    
    public void AdicionarTalhao(Talhao talhao)
    {
        ValidarTalhao(talhao);
        _talhoes.Add(talhao);
    }
}
```
```csharp
public record TalhaoCriadoDomainEvent : DomainEvent
{
    public Guid TalhaoId { get; init; }
    public Guid PropriedadeId { get; init; }
}
```

## 📨 Eventos

### Eventos Publicados

#### PropriedadeCriadaEvent
```csharp
public record PropriedadeCriadaEvent(
    Guid PropriedadeId,
    string Nome,
    string? Endereco,
    decimal? AreaTotal,
    Guid ProprietarioId,
    DateTime DataCriacao,
    string EmailProprietario,
    string NomeProprietario
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.propriedades`  
**Routing Key**: `propriedade.criada`  
**Consumidores**: Notificações (para configurar destinatários de alertas)

#### PropriedadeAtualizadaEvent
```csharp
public record PropriedadeAtualizadaEvent(
    Guid PropriedadeId,
    string Nome,
    string? Endereco,
    decimal? AreaTotal,
    DateTime DataAtualizacao
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.propriedades`  
**Routing Key**: `propriedade.atualizada`  
**Consumidores**: Notificações (para atualizar cache de propriedades no read model)

#### TalhaoCriadoEvent
```csharp
public record TalhaoCriadoEvent(
    Guid TalhaoId,
    Guid PropriedadeId,
    string Nome,
    decimal? Area,
    Guid? CulturaId,
    string? NomeCultura
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.propriedades`  
**Routing Key**: `talhao.criado`  
**Consumidores**: Análise (para habilitar regras de alertas)

#### TalhaoAtualizadoEvent
```csharp
public record TalhaoAtualizadoEvent(
    Guid TalhaoId,
    Guid PropriedadeId,
    string Nome,
    Guid? CulturaId,
    string? NomeCultura
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.propriedades`  
**Routing Key**: `talhao.atualizado`  
**Consumidores**: Análise (para ajustar regras de alertas)

### Eventos Consumidos

#### UsuarioCriadoEvent
```csharp
// Produzido por: Identidade
// Propósito: Sincronizar dados de usuário
public record UsuarioCriadoEvent(
    Guid Id,
    string Email,
    string NomeCompleto,
    DateTime DataCriacao
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.identidade`  
**Routing Key**: `usuario.criado`  
**Queue**: `propriedades.usuario.sync`  
**Ação**: Cria registro em `UsuarioInfo` para cache local

#### UsuarioAtualizadoEvent
```csharp
// Produzido por: Identidade
// Propósito: Manter sincronização de dados
public record UsuarioAtualizadoEvent(
    Guid Id,
    string Email,
    string NomeCompleto,
    DateTime DataAtualizacao
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.identidade`  
**Routing Key**: `usuario.atualizado`  
**Queue**: `propriedades.usuario.sync`  
**Ação**: Atualiza registro em `UsuarioInfo`

## 📊 Modelo de Dados

### Entidades

#### Propriedade
```csharp
public class Propriedade : BaseEntity
{
    public string Nome { get; set; }
    public string? Endereco { get; set; }
    public decimal? AreaTotal { get; set; }
    public Guid ProprietarioId { get; set; }
    
    // Navegação
    public UsuarioInfo Proprietario { get; set; }
    public ICollection<Talhao> Talhoes { get; set; }
}
```

**Validações:**
- Nome: obrigatório, 3-200 caracteres
- AreaTotal: opcional, se preenchido > 0
- ProprietarioId: obrigatório, deve existir

#### Talhao
```csharp
public class Talhao : BaseEntity
{
    public Guid PropriedadeId { get; set; }
    public string Nome { get; set; }
    public decimal? Area { get; set; }
    public Guid? CulturaId { get; set; }
    public string? Localizacao { get; set; }
    
    // Navegação
    public Propriedade Propriedade { get; set; }
    public Cultura? Cultura { get; set; }
}
```

**Validações:**
- Nome: obrigatório, 2-100 caracteres
- Area: opcional, se preenchido > 0 e <= AreaTotal da propriedade
- CulturaId: opcional, deve existir se preenchido

#### Cultura
```csharp
public class Cultura : BaseEntity
{
    public string Nome { get; set; }
    public string? NomeCientifico { get; set; }
    public string? Descricao { get; set; }
    public int? TempoMedioCrescimentoDias { get; set; }
    
    // Navegação
    public ICollection<Talhao> Talhoes { get; set; }
}
```

**Exemplos de Culturas:**
- Soja (*Glycine max*)
- Milho (*Zea mays*)
- Trigo (*Triticum aestivum*)
- Café (*Coffea arabica*)
- Cana-de-açúcar (*Saccharum officinarum*)

#### UsuarioInfo
```csharp
public class UsuarioInfo : BaseEntity
{
    public string Email { get; set; }
    public string NomeCompleto { get; set; }
    
    // Navegação
    public ICollection<Propriedade> Propriedades { get; set; }
}
```

**Propósito**: Cache local de dados de usuários para evitar chamadas síncronas ao microserviço de Identidade.

### Relacionamentos

```
UsuarioInfo 1 ────── * Propriedade
                            │
                            │ 1
                            │
                            * Talhao * ────── 1 Cultura
```

## 🔄 Saga - Criação Completa de Propriedade

### Fluxo

```
1. Recebe DTO com propriedade + talhões
2. Inicia SagaOrchestrator
3. Executa CriarPropriedadeStep
   ✅ Sucesso → Adiciona à lista de executados → Prossegue
   ❌ Falha → Tenta compensar (lista vazia, sem ação) → Retorna erro
4. Executa CriarTalhoesStep
   ✅ Sucesso → Adiciona à lista de executados → Prossegue
   ❌ Falha → Compensa em ordem inversa (remove propriedade) → Retorna erro
5. Executa PublicarEventoStep
   ✅ Sucesso → Saga concluída com sucesso
   ❌ Falha → Compensa em ordem inversa (remove talhões e propriedade) → Retorna erro
6. Retorna SagaExecutionResult (sucesso ou falha com mensagem de erro)
```

**Detalhes Técnicos do Fluxo:**

- **Rastreamento de Passos**: Cada passo bem-sucedido é adicionado à lista `_executedSteps`
- **Ordem de Compensação**: Sempre na ordem **inversa** da execução (último executado é o primeiro compensado)
- **CancellationToken**: Verificado antes de cada passo (`cancellationToken.ThrowIfCancellationRequested()`)
- **Tratamento de Exceções**: Qualquer exceção durante execução aciona compensação automática
- **Compensação Resiliente**: Erros durante compensação são logados mas não impedem outros passos de serem compensados

### Implementação dos Passos

#### CriarPropriedadeStep
```csharp
public class CriarPropriedadeStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    private readonly IPropriedadeRepository _repository;
    private readonly ILogger<CriarPropriedadeStep> _logger;

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
```

#### CriarTalhoesStep
```csharp
public class CriarTalhoesStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    private readonly ITalhaoRepository _repository;
    private readonly ILogger<CriarTalhoesStep> _logger;

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
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao compensar criação de talhões: {Message}", ex.Message);
                throw;
            }
        }
    }
}
```

#### PublicarEventoPropriedadeCriadaStep
```csharp
public class PublicarEventoPropriedadeCriadaStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    private readonly IMessageBusPublisher _messageBusPublisher;
    private readonly ILogger<PublicarEventoPropriedadeCriadaStep> _logger;

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
                _logger.LogError(ex, "Erro ao publicar evento de compensação: {Message}", ex.Message);
                // Não propaga exceção para não quebrar o fluxo de compensação
            }
        }
    }
}
```

**Observações sobre a Implementação:**
- ✅ **Tipo de Retorno**: Usa `SagaStepResult` (não `StepResult`)
- ✅ **Tratamento de Exceções**: Try-catch em cada passo com logging detalhado
- ✅ **Validações**: Verifica pré-condições antes de executar
- ✅ **Metadata**: `ExecuteAsync` retorna dicionário com dados do passo
- ✅ **Compensação**: Publica evento `PropriedadeExcluidaEvent` no passo 3

## 📊 Métricas (Prometheus)

```csharp
public class PropriedadesMetrics
{
    // Contadores
    public static readonly Counter PropriedadesCriadas = 
        Metrics.CreateCounter("propriedades_criadas_total", "Total de propriedades criadas");
    
    public static readonly Counter TalhoesCriados = 
        Metrics.CreateCounter("talhoes_criados_total", "Total de talhões criados");
    
    public static readonly Counter SagasFalhadas = 
        Metrics.CreateCounter("propriedades_sagas_falhadas_total", "Sagas falhadas");
    
    // Gauges
    public static readonly Gauge PropriedadesAtivas = 
        Metrics.CreateGauge("propriedades_ativas", "Propriedades ativas");
    
    public static readonly Gauge AreaTotalCadastrada = 
        Metrics.CreateGauge("propriedades_area_total_hectares", "Área total cadastrada");
    
    // Histogramas
    public static readonly Histogram DuracaoSaga = 
        Metrics.CreateHistogram("propriedades_saga_duracao_segundos", "Duração da saga");
}
```

## 🌐 Endpoints Principais

### Propriedades

#### GET /api/propriedades
Lista propriedades do usuário autenticado.

#### GET /api/propriedades/{id}
Obtém propriedade por ID.

#### GET /api/propriedades/{id}/completa
Obtém propriedade com talhões.

#### POST /api/propriedades
Cria nova propriedade simples.

#### POST /api/propriedades/completa
Cria propriedade com talhões (Saga).

**Request:**
```json
{
  "nome": "Fazenda Boa Vista",
  "endereco": "Rodovia BR-153, Km 45, Zona Rural",
  "areaTotal": 500.50,
  "talhoes": [
    {
      "nome": "Talhão A",
      "area": 150.25,
      "culturaId": "guid",
      "localizacao": "Norte da propriedade"
    },
    {
      "nome": "Talhão B",
      "area": 200.00,
      "culturaId": "guid",
      "localizacao": "Sul da propriedade"
    }
  ]
}
```

#### PUT /api/propriedades/{id}
Atualiza propriedade existente.

#### DELETE /api/propriedades/{id}
Remove propriedade (soft delete).

### Talhões

#### GET /api/talhoes
Lista talhões do usuário.

#### GET /api/talhoes/{id}
Obtém talhão por ID.

#### GET /api/talhoes/propriedade/{propriedadeId}
Lista talhões de uma propriedade.

#### POST /api/talhoes
Cria novo talhão.

#### PUT /api/talhoes/{id}
Atualiza talhão existente.

#### DELETE /api/talhoes/{id}
Remove talhão.

### Culturas

#### GET /api/culturas
Lista todas as culturas cadastradas.

#### GET /api/culturas/{id}
Obtém cultura por ID.

#### POST /api/culturas
Cria nova cultura (Admin).

#### PUT /api/culturas/{id}
Atualiza cultura (Admin).

#### DELETE /api/culturas/{id}
Remove cultura (Admin).

## 🚀 Como Executar

### 1. Configurar appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrosolutions_propriedades;Username=postgres;Password=postgres"
  },
  "Database": {
    "Provider": "PostgreSQL"
  },
  "Identidade": {
    "Url": "https://localhost:5001"
  },
  "Jwt": {
    "Issuer": "AgroSolutions.Identidade",
    "Audience": "AgroSolutions"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "ExchangeName": "agrosolutions.propriedades"
  }
}
```

### 2. Executar Migrations

```bash
dotnet ef database update
```

### 3. Executar o Serviço

```bash
dotnet run
```

### 4. Acessar a Documentação da API

https://localhost:5002/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5002/) redireciona automaticamente para o Scalar.

## 🧪 Testes

```bash
cd ../AgroSolutions.Propriedades.Test
dotnet test
```

**Cobertura:**
- Testes unitários de serviços
- Testes de validação
- Testes de saga (happy path e compensação)
- Testes de repositórios
- Testes arquiteturais

## 🌟 Pontos Fortes

### 1. Saga Pattern para Transações Distribuídas
- **Consistência Eventual**: Garante integridade mesmo em falhas
- **Compensação Automática**: Rollback automático de passos executados
- **Auditoria**: Logs detalhados de cada passo da saga

### 2. Event-Driven Architecture
- **Desacoplamento**: Não depende sincronicamente de outros serviços
- **Resiliência**: Mensagens persistidas garantem entrega
- **Escalabilidade**: Múltiplos consumers podem processar eventos

### 3. Cache Local de Usuários (UsuarioInfo)
- **Performance**: Evita chamadas síncronas ao Identidade
- **Disponibilidade**: Funciona mesmo se Identidade estiver offline
- **Sincronização Eventual**: Mantém dados atualizados via eventos

### 4. Domain-Driven Design (DDD)
- **Invariantes**: Propriedade valida nome, área total e endereço obrigatórios
- **Encapsulamento**: Propriedades com setters privados protegem integridade
- **Métodos de Consulta**: `CalcularAreaDisponivel()` e `PossuiAreaDisponivel()` auxiliam validações
- **Responsabilidade da Application**: A camada de Application deve verificar área disponível antes de criar talhões

**Nota**: A validação de área dos talhões não é imposta automaticamente pela entidade. É responsabilidade da camada de Application chamar `PossuiAreaDisponivel()` antes de persistir novos talhões.

## 📚 Referências

- [Saga Pattern - Chris Richardson](https://microservices.io/patterns/data/saga.html)
- [Domain-Driven Design - Eric Evans](https://www.domainlanguage.com/ddd/)
- [Aggregate Pattern](https://martinfowler.com/bliki/DDD_Aggregate.html)

---

**Propriedades** - Gestão inteligente de propriedades rurais 🏡🌾
