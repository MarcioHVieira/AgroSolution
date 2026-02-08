# 📦 AgroSolutions.SharedKernel

## 🎯 Visão Geral

O **SharedKernel** é a biblioteca compartilhada que fornece componentes transversais, infraestrutura comum e abstrações reutilizáveis para todos os microserviços da plataforma AgroSolutions. Seguindo os princípios de DRY (Don't Repeat Yourself) e garantindo consistência arquitetural em toda a solução.

## 📋 Responsabilidades

- **Configuração Padronizada**: Templates de configuração para microserviços
- **Abstrações de Mensageria**: Interfaces e implementações para RabbitMQ
- **Observabilidade**: Métricas Prometheus e health checks
- **Segurança**: JWT, autenticação e criptografia
- **Banco de Dados**: Abstrações e strategies para múltiplos providers (SQL Server/PostgreSQL)
- **Exceções Customizadas**: Hierarquia de exceções de negócio
- **Value Objects**: CPF, Email e outros objetos de valor
- **Middlewares**: Exception handling, UTF-8 encoding, CORS
- **Sagas**: Orquestrador de transações distribuídas

## 🏗️ Estrutura

```
AgroSolutions.SharedKernel/
├── Application/
│   ├── DTOs/
│   │   ├── ApiResponse.cs                # Resposta padronizada de API
│   │   ├── ErrorDetails.cs               # Detalhes de erro
│   │   └── PaginatedResult.cs            # Resultado paginado
│   └── Exceptions/
│       ├── BusinessException.cs          # Exceção de regra de negócio
│       ├── NotFoundException.cs          # Exceção de recurso não encontrado
│       └── ValidationException.cs        # Exceção de validação
├── Configuration/
│   ├── ApiConfiguration.cs               # Configuração de controllers e API
│   ├── ConsumerConfiguration.cs          # Configuração de consumers RabbitMQ
│   ├── ConsumerSettings.cs               # Settings de consumers
│   ├── CorsConfiguration.cs              # Configuração CORS
│   ├── DatabaseConfiguration.cs          # Configuração de banco de dados
│   ├── JwtAuthenticationConfiguration.cs # Autenticação JWT
│   ├── MicroserviceExtensions.cs         # Extensões padronizadas
│   ├── RabbitMQConfiguration.cs          # Configuração RabbitMQ
│   ├── StandardDatabaseConfiguration.cs  # Template de banco padrão
│   └── Utf8EncodingMiddleware.cs         # Middleware UTF-8
├── Constants/
│   └── AppConstants.cs                   # Constantes da aplicação
├── Database/
│   ├── DatabaseProviderStrategies.cs     # Strategies SQL Server/PostgreSQL
│   ├── DatabaseSettings.cs               # Settings de banco
│   └── IDatabaseProviderStrategy.cs      # Interface de strategy
├── Domain/
│   ├── Entities/
│   ¦   └── BaseEntity.cs                 # Entidade base com auditoria
│   └── ValueObjects/
│       ├── CPF.cs                        # Value Object CPF
│       └── Email.cs                      # Value Object Email
├── Events/
│   ├── DomainEvent.cs                    # Evento de domínio
│   └── IntegrationEvent.cs               # Evento de integração
├── Filters/
│   └── ApiResponseFilter.cs              # Filtro de resposta padronizada
├── Infrastructure/
│   └── Extensions/
│       ├── DateTimeExtensions.cs         # Extensões de DateTime
│       └── StringExtensions.cs           # Extensões de String
├── Messaging/
│   ├── FlexibleEnumConverter.cs          # Conversor JSON para enums
│   ├── IRabbitMQPublisher.cs             # Interface de publicação
│   ├── RabbitMQMessageDeserializer.cs    # Deserializador de mensagens
│   ├── RabbitMQPublisher.cs              # Implementação do publisher
│   └── RabbitMQSettings.cs               # Settings do RabbitMQ
├── Middleware/
│   └── ExceptionHandlingMiddleware.cs    # Middleware de exceções global
└── Sagas/
    ├── ISagaStep.cs                      # Interface de passo de saga
    └── SagaOrchestrator.cs               # Orquestrador de saga
```

## 💻 Tecnologias Utilizadas

### Core
- **.NET 10** - Framework base
- **C# 14** - Linguagem de programação

### Banco de Dados
- **Entity Framework Core 10** - ORM
- **Microsoft.EntityFrameworkCore.SqlServer** - Provider SQL Server (padrão)
- **Npgsql.EntityFrameworkCore.PostgreSQL** - Provider PostgreSQL (alternativa)

### Mensageria
- **RabbitMQ.Client 7.x** - Cliente RabbitMQ oficial

### Observabilidade
- **Prometheus.NetCore** - Métricas

### Autenticação
- **Microsoft.AspNetCore.Authentication.JwtBearer** - Autenticação JWT
- **System.IdentityModel.Tokens.Jwt** - Manipulação de tokens

### Documentação
- **OpenAPI (.NET 10)** - Especificação OpenAPI nativa do .NET
- **Scalar.AspNetCore** - UI moderna para visualização de APIs

## 🎨 Design Patterns Aplicados

### Criacionais

#### Strategy Pattern
```csharp
public interface IDatabaseProviderStrategy
{
    string ProviderName { get; }
    void Configure(DbContextOptionsBuilder options, DatabaseSettings settings);
    string GetHealthCheckName();
}
```
**Nota**: Interface definida mas implementações diretas via `DatabaseConfiguration.ConfigureDatabase()`.

O código atualmente usa uma abordagem mais simples com switch statement:
```csharp
var databaseProvider = configuration["DatabaseProvider"] ?? "SqlServer";

if (databaseProvider.Equals("PostgreSQL", StringComparison.OrdinalIgnoreCase))
    optionsBuilder.UseNpgsql(connectionString);
else if (databaseProvider.Equals("SqlServer", StringComparison.OrdinalIgnoreCase))
    optionsBuilder.UseSqlServer(connectionString);
```

### Estruturais

#### Repository Pattern
```csharp
// Interface base para repositórios
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> ObterTodosAsync(CancellationToken cancellationToken = default);
    Task AdicionarAsync(T entidade, CancellationToken cancellationToken = default);
    Task AtualizarAsync(T entidade, CancellationToken cancellationToken = default);
    Task RemoverAsync(Guid id, CancellationToken cancellationToken = default);
}
```

#### Facade Pattern
```csharp
// MicroserviceExtensions simplifica configurações complexas
public static IServiceCollection AddStandardMicroserviceServices<TContext>(
    this IServiceCollection services,
    IConfiguration configuration,
    IHostEnvironment environment)
    where TContext : DbContext
{
    // Agrega múltiplas configurações em uma chamada simples
    services.AddDatabaseConfiguration<TContext>(configuration, environment);
    services.AddJwtAuthentication(configuration, environment);
    services.AddRabbitMQPublisher(configuration);
    services.AddHealthChecks();
    return services;
}
```

### Comportamentais

#### Template Method Pattern
```csharp
public abstract class BaseEntity
{
    public Guid Id { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
    
    // Template method - comportamento padrão que pode ser sobrescrito
    public virtual void PreencherAuditoria()
    {
        if (Id == Guid.Empty)
            DataCriacao = DateTime.UtcNow;
        else
            DataAtualizacao = DateTime.UtcNow;
    }
}
```

#### Saga Pattern (Orchestration)
```csharp
public class SagaOrchestrator<TData>
{
    private readonly List<ISagaStep<TData>> _steps = new();
    
    public async Task<SagaExecutionResult> ExecuteAsync(TData data)
    {
        foreach (var step in _steps)
        {
            var result = await step.ExecuteAsync(data);
            if (!result.Success)
            {
                await CompensateAsync(data); // Rollback automático
                return SagaExecutionResult.Fail(result.ErrorMessage);
            }
        }
        return SagaExecutionResult.Ok();
    }
}
```

#### Chain of Responsibility
```csharp
// Pipeline de middlewares
app.UseUtf8Encoding();           // Middleware 1
app.UseExceptionHandling();      // Middleware 2
app.UseAuthentication();         // Middleware 3
app.UseAuthorization();          // Middleware 4
```

### Outros Patterns

#### Value Object Pattern
```csharp
public record Email
{
    public string Valor { get; }
    
    private Email(string valor) => Valor = valor;
    
    public static Result<Email> Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result<Email>.Fail("Email não pode ser vazio");
            
        if (!EmailValidator.IsValid(email))
            return Result<Email>.Fail("Email inválido");
            
        return Result<Email>.Ok(new Email(email));
    }
}
```

## 🔧 Componentes Principais

### 1. Configuração de Microserviços

#### MicroserviceExtensions
Fornece métodos de extensão para configuração padronizada de todos os microserviços:

```csharp
// Configuração completa em uma linha
builder.Services.AddStandardMicroserviceServices<MyDbContext>(
    builder.Configuration, 
    builder.Environment
);

// Pipeline padronizado
app.UseStandardMicroservicePipeline<MyDbContext>(
    serviceName: "AgroSolutions.MyService"
);
```

**Funcionalidades incluídas:**
- Configuração de banco de dados (SQL Server ou PostgreSQL)
- Autenticação JWT
- RabbitMQ Publisher
- Health Checks (liveness, readiness)
- Exception handling
- UTF-8 encoding
- CORS

### 2. Mensageria com RabbitMQ

#### RabbitMQPublisher
Publisher robusto com retry, reconnection e proper disposal:

```csharp
public interface IRabbitMQPublisher
{
    Task PublishAsync<T>(T message, string exchange, string routingKey) 
        where T : IntegrationEvent;
}

// Uso
await _publisher.PublishAsync(
    new UsuarioCriadoEvent(userId, email, nome),
    exchange: "agrosolutions.identidade",
    routingKey: "usuario.criado"
);
```

**Características:**
- Reconnection automática em falhas
- Connection pooling
- Serialização JSON automática
- Enriquecimento com correlation ID
- Logs estruturados

#### IntegrationEvent
Classe base para todos os eventos de integração:

```csharp
public abstract record IntegrationEvent
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public DateTime OccurredOn { get; init; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public int Version { get; init; } = 1;
    public string? CorrelationId { get; init; }
}

// Implementação
public record UsuarioCriadoEvent(
    Guid UserId,
    string Email,
    string NomeCompleto
) : IntegrationEvent;
```

### 3. Observabilidade

#### Prometheus Metrics
Métricas expostas via Prometheus em todos os microserviços:

```csharp
// Métricas são expostas automaticamente no endpoint /metrics
// Configurado via Prometheus.NetCore
```

**Funcionalidades:**
- Endpoint /metrics com formato Prometheus
- Métricas HTTP automáticas (requisições, duração, etc.)
- Métricas customizadas por domínio
- Health checks integrados

### 4. Autenticação JWT

#### JwtAuthenticationConfiguration
Configuração unificada de autenticação JWT usando JWKS:

```csharp
services.AddJwtAuthentication(configuration, environment);
```

**Características:**
- Validação usando JWKS (JSON Web Key Set)
- Suporte a múltiplos issuers
- Clock skew configurável
- Validação de audience
- Logs detalhados em desenvolvimento

**Fluxo de Validação:**
1. Microserviço recebe token JWT no header Authorization
2. Extrai kid (Key ID) do header do token
3. Busca chave pública correspondente no JWKS endpoint
4. Valida assinatura usando RSA
5. Valida claims (issuer, audience, expiration)
6. Popula ClaimsPrincipal para autorização

### 5. Exception Handling

#### ExceptionHandlingMiddleware
Middleware global que captura e trata exceções:

```csharp
app.UseExceptionHandling();
```

**Tratamento por tipo:**

| Exceção | Status Code | Resposta |
|---------|-------------|----------|
| `ValidationException` | 400 Bad Request | Erros de validação detalhados |
| `NotFoundException` | 404 Not Found | Mensagem de recurso não encontrado |
| `BusinessException` | 400 Bad Request | Mensagem de regra de negócio |
| `UnauthorizedException` | 401 Unauthorized | Falha de autenticação |
| `Exception` (genérica) | 500 Internal Server Error | Erro interno (detalhes ocultos em produção) |

**Resposta padronizada:**
```json
{
  "success": false,
  "message": "Mensagem de erro",
  "errors": {
    "campo1": ["Erro 1", "Erro 2"],
    "campo2": ["Erro 3"]
  },
  "timestamp": "2024-02-03T10:30:00Z",
  "traceId": "00-abc123-xyz789-00"
}
```

### 6. Banco de Dados

#### DatabaseConfiguration
Configuração abstrata que suporta múltiplos providers:

```csharp
services.AddDatabaseConfiguration<MyDbContext>(configuration, environment);
```

**Providers suportados:**
- **SQL Server** (padrão - `"DatabaseProvider": "SqlServer"`)
- **PostgreSQL** (alternativa - `"DatabaseProvider": "PostgreSQL"`)

**Nota**: As classes `SqlServerProviderStrategy` e `PostgreSqlProviderStrategy` existem, mas o código usa diretamente `DatabaseConfiguration.ConfigureDatabase()` que faz o switch internamente.

**Funcionalidades:**
- Retry policy automático
- Connection pooling
- Command timeout configurável
- Health checks
- Migrations automáticas (opcional)

#### DatabaseProviderStrategy
Strategy pattern para trocar providers sem modificar código:

```csharp
{
  "Database": {
    "Provider": "SqlServer",  // ou "PostgreSQL"
    "ConnectionString": "...",
    "MaxRetryCount": 3,
    "MaxRetryDelaySeconds": 30,
    "CommandTimeoutSeconds": 30
  }
}
```

### 7. Sagas

#### SagaOrchestrator
Orquestrador de transações distribuídas com compensação automática:

```csharp
var saga = new SagaOrchestrator<CriarPropriedadeDto>(_logger)
    .AddStep(new CriarPropriedadeStep())
    .AddStep(new CriarTalhoesStep())
    .AddStep(new PublicarEventoStep());

var result = await saga.ExecuteAsync(dto);

if (!result.Success)
{
    // Compensação automática já foi executada
    _logger.LogError("Saga falhou: {Error}", result.ErrorMessage);
}
```

**Características:**
- Execução sequencial de passos
- Compensação automática em ordem reversa
- Suporte a CancellationToken
- Logs detalhados de cada passo
- Rollback completo em caso de falha

**Interface ISagaStep:**
```csharp
public interface ISagaStep<TData>
{
    Task<StepResult> ExecuteAsync(TData data, CancellationToken cancellationToken);
    Task CompensateAsync(TData data, CancellationToken cancellationToken);
}
```

### 8. Value Objects

#### CPF
```csharp
var cpfResult = CPF.Create("123.456.789-00");
if (cpfResult.IsSuccess)
{
    var cpf = cpfResult.Value;
    // Usa CPF validado
}
```

**Validações:**
- Formato correto
- Dígitos verificadores
- CPFs conhecidos inválidos

#### Email
```csharp
var emailResult = Email.Create("usuario@exemplo.com");
if (emailResult.IsSuccess)
{
    var email = emailResult.Value;
    // Usa email validado
}
```

**Validações:**
- Formato RFC 5322
- Domínio válido

## 🚀 Como Usar

### 1. Adicionar Referência

No arquivo `.csproj` do microserviço:

```xml
<ItemGroup>
  <ProjectReference Include="..\AgroSolutions.SharedKernel\AgroSolutions.SharedKernel.csproj" />
</ItemGroup>
```

### 2. Configurar Program.cs

```csharp
using AgroSolutions.SharedKernel.Configuration;

var builder = WebApplication.CreateBuilder(args);

// Adiciona serviços padronizados
builder.Services.AddStandardMicroserviceServices<MyDbContext>(
    builder.Configuration,
    builder.Environment
);

// Adiciona serviços específicos do microserviço
builder.Services.AddScoped<IMyService, MyService>();

var app = builder.Build();

// Usa pipeline padronizado
app.UseStandardMicroservicePipeline<MyDbContext>(
    serviceName: "AgroSolutions.MyService"
);

app.Run();
```

### 3. Publicar Eventos

```csharp
public class MyService
{
    private readonly IRabbitMQPublisher _publisher;
    
    public async Task CriarUsuarioAsync(CriarUsuarioDto dto)
    {
        // Lógica de criação
        var usuario = new Usuario(dto);
        await _repository.AdicionarAsync(usuario);
        
        // Publica evento
        await _publisher.PublishAsync(
            new UsuarioCriadoEvent(usuario.Id, usuario.Email, usuario.Nome),
            exchange: "agrosolutions.identidade",
            routingKey: "usuario.criado"
        );
    }
}
```

### 4. Consumir Eventos

```csharp
// Implementar consumer usando RabbitMQ.Client
public class MyConsumerService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = JsonSerializer.Deserialize<UsuarioCriadoEvent>(body);
            
            // Processa evento
            await ProcessarUsuarioCriado(message);
            
            // Acknowledge
            await _channel.BasicAckAsync(ea.DeliveryTag, false);
        };
        
        await _channel.BasicConsumeAsync(
            queue: "meu-servico.usuario-criado",
            autoAck: false,
            consumer: consumer
        );
    }
}
```

### 5. Usar Sagas

```csharp
// Definir passos da saga
public class CriarPropriedadeStep : ISagaStep<CriarPropriedadeCompletaDto>
{
    public async Task<StepResult> ExecuteAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken)
    {
        try
        {
            var propriedade = new Propriedade(data.Nome, data.Endereco);
            await _repository.AdicionarAsync(propriedade, cancellationToken);
            
            data.PropriedadeId = propriedade.Id; // Passa para próximos passos
            return StepResult.Ok();
        }
        catch (Exception ex)
        {
            return StepResult.Fail(ex.Message);
        }
    }
    
    public async Task CompensateAsync(
        CriarPropriedadeCompletaDto data,
        CancellationToken cancellationToken)
    {
        if (data.PropriedadeId.HasValue)
        {
            await _repository.RemoverAsync(data.PropriedadeId.Value, cancellationToken);
        }
    }
}

// Orquestrar saga
var saga = new SagaOrchestrator<CriarPropriedadeCompletaDto>(_logger)
    .AddStep(_serviceProvider.GetRequiredService<CriarPropriedadeStep>())
    .AddStep(_serviceProvider.GetRequiredService<CriarTalhoesStep>())
    .AddStep(_serviceProvider.GetRequiredService<PublicarEventoStep>());

var result = await saga.ExecuteAsync(dto);
```

## 🧪 Testes

O SharedKernel possui testes unitários abrangentes:

```bash
cd src/AgroSolutions.SharedKernel.Test
dotnet test
```

**Cobertura de testes:**
- Value Objects (CPF, Email)
- Saga Orchestrator
- Exception Handling Middleware
- Database Provider Strategies
- RabbitMQ Publisher

## 🔒 Segurança

### Proteções Implementadas

- **JWT Validation**: Validação rigorosa de tokens
- **Exception Sanitization**: Detalhes de erro ocultos em produção
- **SQL Injection Protection**: Queries parametrizadas via EF Core
- **Sensitive Data Logging**: Evita logging de dados sensíveis

### Boas Práticas

- Sempre use `CancellationToken` em operações async
- Valide entrada usando Value Objects
- Use exceções customizadas para fluxos de negócio
- Configure timeouts apropriados

## 📊 Métricas e Monitoramento

### Métricas Prometheus

Cada microserviço expõe automaticamente:
- **HTTP Metrics**: Taxa de requisições, duração, status codes
- **Health Checks**: Status de dependências (banco, RabbitMQ)
- **Custom Metrics**: Métricas específicas por domínio

**Endpoints:**
- `/metrics` - Métricas no formato Prometheus
- `/health` - Health check em JSON

## 🔄 Extensibilidade

### Adicionar Novo Database Provider

1. Implementar `IDatabaseProviderStrategy`:

```csharp
public class MySqlProviderStrategy : IDatabaseProviderStrategy
{
    public string ProviderName => "MySQL";
    
    public void Configure(DbContextOptionsBuilder options, DatabaseSettings settings)
    {
        options.UseMySQL(settings.ConnectionString, mysqlOptions =>
        {
            // Configurações específicas do MySQL
        });
    }
    
    public string GetHealthCheckName() => "mysql";
}
```

2. Adicionar suporte em `DatabaseConfiguration.ConfigureDatabase()`:

```csharp
else if (databaseProvider.Equals("MySQL", StringComparison.OrdinalIgnoreCase))
{
    optionsBuilder.UseMySQL(connectionString);
}
```

### Adicionar Novo Middleware

```csharp
public class CustomMiddleware
{
    private readonly RequestDelegate _next;
    
    public CustomMiddleware(RequestDelegate next)
    {
        _next = next;
    }
    
    public async Task InvokeAsync(HttpContext context)
    {
        // Lógica antes
        
        await _next(context);
        
        // Lógica depois
    }
}

// Extension method
public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder app)
{
    return app.UseMiddleware<CustomMiddleware>();
}
```

## 📚 Referências

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [Microservices Patterns - Chris Richardson](https://microservices.io/patterns/index.html)
- [Saga Pattern](https://microservices.io/patterns/data/saga.html)
- [RabbitMQ Best Practices](https://www.rabbitmq.com/best-practices.html)
- [Prometheus Documentation](https://prometheus.io/docs/introduction/overview/)

---

**SharedKernel** - A fundação sólida da plataforma AgroSolutions 📦🏗️
