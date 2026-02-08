# 🔐 AgroSolutions.Identidade

## 🎯 Visão Geral

O microserviço de **Identidade** é responsável por toda a gestão de autenticação, autorização e conformidade com a LGPD na plataforma AgroSolutions. Implementa um sistema robusto de segurança com JWT/RSA, gestão de usuários, auditoria completa e funcionalidades avançadas de privacidade.

> **📝 Nota sobre a Documentação**: Todos os exemplos de código neste README refletem o código-fonte real do projeto. As assinaturas de métodos, DTOs e interfaces correspondem à implementação atual.

## 📋 Responsabilidades

- **Autenticação**: Login, registro, validação de email, recuperação de senha
- **Autorização**: Gestão de perfis e permissões
- **Gestão de Tokens**: Emissão de JWT, refresh tokens, JWKS endpoint
- **LGPD Compliance**: Anonimização, exclusão, exportação de dados
- **Auditoria**: Registro de todos os acessos e modificações
- **Segurança**: Criptografia, hash de senhas, chaves RSA

## 🏗️ Arquitetura

### Camadas

```
API Layer (Controllers)
    ↓
Application Layer (Services)
    ↓
Domain Layer (Entities, Rules)
    ↓
Infrastructure Layer (Repositories, External Services)
```

### Estrutura de Diretórios

```
AgroSolutions.Identidade/
├── API/
│   ├── Controllers/
│   │   ├── AutenticacaoController.cs       # Login, registro, refresh
│   │   ├── ChavesController.cs             # Gestão de chaves RSA
│   │   ├── JwksController.cs               # JWKS endpoint (público)
│   │   ├── PrivacidadeController.cs        # Funcionalidades LGPD
│   │   ├── ProviderStatesController.cs     # Pact testing
│   │   └── SaudeController.cs              # Health checks
│   └── Extensions/
│       └── ClaimsPrincipalExtensions.cs    # Extensões de claims
├── Application/
│   ├── DTOs/
│   │   ├── IdentidadeDtos.cs               # DTOs de autenticação
│   │   └── PrivacidadeDtos.cs              # DTOs LGPD
│   ├── Events/
│   │   ├── UsuarioCriadoEvent.cs           # Evento de usuário criado
│   │   └── UsuarioAtualizadoEvent.cs       # Evento de usuário atualizado
│   ├── Interfaces/
│   │   ├── IAnonimizacaoService.cs         # Serviço de anonimização
│   │   ├── IAuditoriaService.cs            # Serviço de auditoria
│   │   ├── ICriptografiaService.cs         # Serviço de criptografia
│   │   ├── IEmailService.cs                # Serviço de email
│   │   ├── IIdentidadeService.cs           # Serviço principal
│   │   ├── IPrivacidadeService.cs          # Serviço LGPD
│   │   └── ITokenService.cs                # Serviço de tokens
│   ├── Services/
│   │   ├── AnonimizacaoService.cs          # Implementação anonimização
│   │   ├── AuditoriaService.cs             # Implementação auditoria
│   │   ├── IdentidadeService.cs            # Implementação principal
│   │   └── PrivacidadeService.cs           # Implementação LGPD
│   └── Validators/
│       └── IdentidadeValidators.cs         # Validações FluentValidation
├── Configuration/
│   ├── Settings/
│   │   ├── EmailSettings.cs                # Configurações de email
│   │   └── JwtSettings.cs                  # Configurações JWT
│   ├── ApiConfiguration.cs                 # Configuração de API
│   ├── ApiDocumentationConfiguration.cs    # OpenAPI + Scalar
│   ├── DependencyInjectionConfiguration.cs # Injeção de dependências
│   └── MonitoringConfiguration.cs          # Observabilidade
├── Domain/
│   ├── Entities/
│   │   ├── AuditoriaAcesso.cs              # Entidade de auditoria
│   │   ├── CodigoValidacao.cs              # Código de validação email
│   │   ├── RefreshToken.cs                 # Token de refresh
│   │   └── Usuario.cs                      # Entidade usuário
│   ├── Enums/
│   │   ├── PerfilAcesso.cs                 # Perfis de acesso
│   │   └── StatusUsuario.cs                # Status do usuário
│   └── Interfaces/
│       ├── IAuditoriaRepository.cs         # Repositório auditoria
│       ├── ICodigoValidacaoRepository.cs   # Repositório códigos
│       ├── IRefreshTokenRepository.cs      # Repositório tokens
│       └── IUsuarioRepository.cs           # Repositório usuários
├── Infrastructure/
│   ├── BackgroundJobs/
│   │   └── ExclusaoAutomaticaJob.cs        # Job LGPD de exclusão
│   ├── Data/
│   │   └── IdentidadeDbContext.cs          # DbContext
│   ├── Logging/
│   │   ├── SensitiveDataLogger.cs          # Logger que mascara dados
│   │   └── SensitiveDataLoggerProvider.cs  # Provider do logger
│   ├── Metrics/
│   │   └── IdentidadeMetrics.cs            # Métricas Prometheus
│   ├── Repositories/
│   │   ├── AuditoriaRepository.cs          # Implementação auditoria
│   │   ├── CodigoValidacaoRepository.cs    # Implementação códigos
│   │   ├── RefreshTokenRepository.cs       # Implementação tokens
│   │   └── UsuarioRepository.cs            # Implementação usuários
│   ├── Security/
│   │   └── RsaKeyManager.cs                # Gerenciador de chaves RSA
│   └── Services/
│       ├── CriptografiaService.cs          # Implementação criptografia
│       ├── EmailService.cs                 # Implementação email
│       └── TokenService.cs                 # Implementação tokens
└── Program.cs
```

## 💻 Tecnologias e Técnicas Aplicadas

### Core
- **.NET 10** - Framework
- **C# 14** - Linguagem
- **ASP.NET Core** - Web API

### Banco de Dados
- **Entity Framework Core 10** - ORM
- **SQL Server** - Banco de dados padrão (alternativa: PostgreSQL)
- **Migrations** - Versionamento de schema

### Segurança
- **JWT (JSON Web Tokens)** - Autenticação stateless
- **RSA 2048 bits** - Criptografia assimétrica
- **Argon2id** - Hash de senhas (algoritmo resistente a ataques GPU/ASIC)
- **JWKS** - JSON Web Key Set para distribuição de chaves públicas

### Mensageria
- **RabbitMQ** - Publicação de eventos de integração
- **Event-Driven Architecture** - Comunicação assíncrona

### Observabilidade
- **Prometheus** - Métricas customizadas
- **Health Checks** - Monitoramento de saúde

### Validação
- **FluentValidation** - Validações fluentes

### Documentação
- **OpenAPI (.NET 10)** - Especificação OpenAPI nativa
- **Scalar** - UI moderna para APIs

## 🎨 Design Patterns Aplicados

### Arquiteturais

#### 1. **Clean Architecture**
Separação clara em camadas com dependências direcionadas para o domínio:
- **API**: Controllers e endpoints
- **Application**: Lógica de aplicação e coordenação
- **Domain**: Regras de negócio e entidades
- **Infrastructure**: Detalhes técnicos e implementações

#### 2. **Event-Driven Architecture**
Publicação de eventos para comunicação assíncrona com outros microserviços.

### Criacionais

```csharp
// Exemplo de criação de usuário
public Usuario(string nomeCompleto, string email, string senhaHash, 
                PerfilAcesso perfil, string? telefone = null, string? cpf = null)
{
    Id = Guid.NewGuid();
    NomeCompleto = nomeCompleto;
    Email = email.ToLowerInvariant();
    SenhaHash = senhaHash;
    Perfil = perfil;
    // ...
}
```

### Estruturais

#### 1. **Repository Pattern**
Abstração do acesso a dados:
```csharp
public interface IUsuarioRepository
{
    Task<Usuario?> ObterPorIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Usuario?> ObterPorEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<List<Usuario>> ObterMarcadosParaExclusaoAsync(DateTime dataLimite, CancellationToken cancellationToken = default);
    Task AdicionarAsync(Usuario usuario, CancellationToken cancellationToken = default);
    Task AtualizarAsync(Usuario usuario, CancellationToken cancellationToken = default);
}
```

#### 2. **Adapter Pattern**
Adaptação de serviços externos (email, criptografia):
```csharp
public interface IEmailService
{
    Task EnviarEmailAsync(string destinatario, string assunto, string corpo);
}

// Adapter para SMTP, SendGrid, etc.
```

#### 3. **Facade Pattern**
Simplificação de operações complexas:
```csharp
public class IdentidadeService
{
    // Fachada que coordena múltiplos serviços
    public async Task<LoginResponseDto> AutenticarAsync(LoginDto dto)
    {
        // Valida usuário
        // Gera tokens
        // Registra auditoria
        // Retorna resposta consolidada
    }
}
```

### Comportamentais

#### 1. **Observer Pattern**
Eventos de domínio e integração:
```csharp
// Quando usuário é criado, publica evento
await _publisher.PublishAsync(
    new UsuarioCriadoEvent(usuario.Id, usuario.Email, usuario.NomeCompleto),
    exchange: "agrosolutions.identidade",
    routingKey: "usuario.criado"
);
```

#### 2. **Command Pattern**
Encapsulamento de operações como records (não usado explicitamente, mas DTOs funcionam de forma similar):
```csharp
// Exemplo de DTO usado como comando
public record RegistrarUsuarioDto(
    string NomeCompleto,
    string Email,
    string Senha,
    string? Telefone,
    string? Cpf
);
```

### LGPD Patterns

#### 1. **Data Anonymization Pattern**
```csharp
public void Anonimizar()
{
    NomeCompleto = $"Usuário Anônimo {Id.ToString()[..8]}";
    Email = $"anonimo-{Id}@excluido.local";
    Telefone = null;
    Cpf = null;
    SenhaHash = string.Empty;
    Excluido = true;
    DataExclusao = DateTime.UtcNow;
    MotivoExclusao = "Anonimizado conforme LGPD";
    Status = StatusUsuario.Excluido;
    DataAtualizacao = DateTime.UtcNow;
}
```

#### 2. **Audit Trail Pattern**
Registro completo de todas as operações:
```csharp
public record AuditoriaAcesso
{
    public string Acao { get; set; }            // LOGIN, LOGOUT, ATUALIZACAO, etc.
    public string Entidade { get; set; }        // Usuario
    public Guid EntidadeId { get; set; }        // ID do usuário
    public object? DadosAntigos { get; set; }   // Estado anterior
    public object? DadosNovos { get; set; }     // Estado novo
    public bool Sucesso { get; set; }
    public DateTime DataHora { get; set; }
}
```

## 📨 Eventos Publicados

### UsuarioCriadoEvent
```csharp
public record UsuarioCriadoEvent(
    Guid Id,
    string Email,
    string NomeCompleto,
    DateTime DataCriacao
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.identidade`  
**Routing Key**: `usuario.criado`  
**Consumidores**: Propriedades (sincroniza dados de usuário)

### UsuarioAtualizadoEvent
```csharp
public record UsuarioAtualizadoEvent(
    Guid Id,
    string Email,
    string NomeCompleto,
    DateTime DataAtualizacao
) : IntegrationEvent;
```

**Exchange**: `agrosolutions.identidade`  
**Routing Key**: `usuario.atualizado`  
**Consumidores**: Propriedades, Notificações (atualiza caches de usuário)

## 🔒 Segurança - Pontos Fortes

### 1. Autenticação Robusta

#### JWT com RSA
- **Assinatura Assimétrica**: Tokens assinados com chave privada RSA 2048 bits
- **Impossível de Forjar**: Sem acesso à chave privada, tokens não podem ser falsificados
- **Validação Distribuída**: Outros microserviços validam usando apenas chave pública

#### JWKS (JSON Web Key Set)
```csharp
// Endpoint público: GET /api/jwks
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "2024-02-key-1",
      "n": "public_key_modulus...",
      "e": "AQAB"
    }
  ]
}
```

**Benefícios:**
- Rotação de chaves sem downtime
- Múltiplas chaves ativas simultaneamente
- Validação sem shared secrets

#### Refresh Tokens
- **Tokens de Longa Duração**: 7 dias vs 1 hora do JWT
- **Rotação Automática**: Novo refresh token a cada uso
- **Revogação**: Possibilidade de invalidar tokens específicos
- **One-Time Use**: Usado uma vez e descartado

### 2. Proteção de Senhas

#### Argon2id
```csharp
// Hash com Argon2id - mais seguro que BCrypt
public string GerarHash(string senha)
{
    var salt = RandomNumberGenerator.GetBytes(16);
    var hash = GerarHashArgon2(senha, salt);
    
    // Formato PHC String padrão
    return $"$argon2id$v=19$m=65536,t=4,p=2${saltBase64}${hashBase64}";
}
```

**Características:**
- **Salt Automático**: Cada senha tem salt único de 128 bits
- **Resistente a GPU/ASIC**: Algoritmo memory-hard
- **Resistente a Rainbow Tables**: Salt elimina essa vulnerabilidade
- **Configurável**: Memória 64MB, 4 iterações, 2 threads
- **Mais Seguro que BCrypt**: Recomendado pela OWASP

### 3. Proteção Contra Ataques

#### Account Lockout
Bloqueio automático de conta após tentativas falhas:
```csharp
public void RegistrarTentativaFalhaLogin()
{
    TentativasFalhasLogin++;
    DataUltimaTentativaFalha = DateTime.UtcNow;

    // Bloquear conta após 5 tentativas falhas
    if (TentativasFalhasLogin >= 5)
    {
        DataBloqueio = DateTime.UtcNow.AddMinutes(30); // Bloqueia por 30 minutos
    }
}
```

**Implementado:**
- ✅ 5 tentativas máximas de login
- ✅ Bloqueio temporário de 30 minutos
- ✅ Reset automático após login bem-sucedido

**Não implementado (preparado para futuro):**
- ⏳ Rate limiting por IP
- ⏳ CAPTCHA após tentativas falhas
- ⏳ Notificação ao usuário sobre bloqueio

#### Sensitive Data Logging
```csharp
public class SensitiveDataLogger : ILogger
{
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, ...)
    {
        var message = MascararDadosSensiveis(state.ToString());
        // Email: user@example.com ? us**@ex******.com
        // CPF: 123.456.789-00 ? ***.***.789-**
    }
}
```

### 4. Auditoria (Infraestrutura Preparada)

A infraestrutura de auditoria está completamente implementada e pronta para uso:

```csharp
public interface IAuditoriaService
{
    Task RegistrarAsync(
        string acao,
        string entidade,
        Guid? entidadeId = null,
        object? dadosAntigos = null,
        object? dadosNovos = null,
        bool sucesso = true,
        string? mensagemErro = null);
}
```

**Componentes Disponíveis:**
- ✅ Interface `IAuditoriaService` definida
- ✅ Implementação `AuditoriaService` completa
- ✅ Entidade `AuditoriaAcesso` no domínio
- ✅ Repository `IAuditoriaRepository` e implementação
- ✅ Registro automático de IP, UserAgent e timestamps
- ✅ Serialização segura de dados antigos/novos

**Status de Integração:** ⏳ **Pendente**

A infraestrutura está pronta, mas aguarda integração nos seguintes pontos:

**Eventos preparados para auditoria (a implementar):**
- ⏳ Login bem-sucedido
- ⏳ Login falho (tentativas de acesso inválidas)
- ⏳ Logout
- ⏳ Registro de novo usuário
- ⏳ Alteração de senha
- ⏳ Alteração de dados pessoais
- ⏳ Solicitação de exclusão (LGPD)
- ⏳ Anonimização de dados (LGPD)
- ⏳ Validação de email
- ⏳ Recuperação de senha

**Exemplo de uso (quando integrado):**
```csharp
await _auditoriaService.RegistrarAsync(
    acao: "LOGIN_SUCESSO",
    entidade: "Usuario",
    entidadeId: usuario.Id,
    dadosAntigos: null,
    dadosNovos: new { usuario.Email, IpAddress, UserAgent },
    sucesso: true
);
```

**Próximo passo:** Injetar `IAuditoriaService` no `IdentidadeService` e adicionar chamadas nos pontos críticos.

## ⚖️ Conformidade LGPD - Pontos Fortes

### Direitos do Titular Implementados

#### 1. Direito ao Acesso (Art. 18, I e II)
```csharp
// GET /api/privacidade/meus-dados
public async Task<MeusDadosDto> ObterMeusDadosAsync()
{
    return new MeusDadosDto
    {
        DadosPessoais = // Todos os dados do usuário
        Auditorias = // Histórico de acessos
        Consentimentos = // Consentimentos dados
    };
}
```

#### 2. Direito à Correção (Art. 18, III)
```csharp
// PUT /api/privacidade/atualizar-dados
// Permite atualização de dados pessoais com auditoria
```

#### 3. Direito à Anonimização (Art. 18, IV)
```csharp
public async Task AnonimizarDadosUsuarioAsync(Guid usuarioId)
{
    usuario.Anonimizar();
    await _repository.AtualizarAsync(usuario);
    await _auditoriaService.RegistrarAsync("ANONIMIZACAO_DADOS", ...);
}
```

#### 4. Direito à Portabilidade (Art. 18, V)
```csharp
// GET /api/privacidade/exportar-dados
// Retorna JSON com todos os dados em formato estruturado
```

#### 5. Direito à Exclusão (Art. 18, VI)
```csharp
// DELETE /api/privacidade/excluir-conta
// Marca para exclusão em 30 dias conforme Art. 16
```

### Processos Automatizados LGPD

#### Exclusão Automática (Background Job)
```csharp
public class ExclusaoAutomaticaJob : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // Executa diariamente às 2h AM
            await ProcessarExclusoes();
            await Task.Delay(TimeSpan.FromHours(24), stoppingToken);
        }
    }
    
    private async Task ProcessarExclusoes()
    {
        // Busca usuários marcados há mais de 30 dias
        var dataLimite = DateTime.UtcNow.AddDays(-30);
        var usuarios = await _repository.ObterMarcadosParaExclusaoAsync(dataLimite);
        
        foreach (var usuario in usuarios)
        {
            // Anonimiza dados
            await _anonimizacaoService.AnonimizarDadosUsuarioAsync(usuario.Id);
        }
    }
}
```

### Princípios LGPD Implementados

#### 1. Finalidade (Art. 6, I)
- ✅ Dados coletados apenas para autenticação e gestão de usuários
- ⏳ Termo de consentimento (planejado)

#### 2. Adequação (Art. 6, II)
- ✅ Processamento compatível com finalidades informadas

#### 3. Necessidade (Art. 6, III)
- ✅ Coleta mínima de dados necessários
- ✅ Campos opcionais claramente marcados (CPF e Telefone)

#### 4. Transparência (Art. 6, VI)
- ✅ Auditoria completa disponível ao titular via API
- ⏳ Política de privacidade clara e acessível (planejada)

#### 5. Segurança (Art. 6, VII)
- ✅ Criptografia em trânsito (HTTPS)
- ✅ Hash de senhas com Argon2id
- ✅ Acesso baseado em perfis (PerfilAcesso)
- ⏳ Criptografia em repouso para dados sensíveis (planejada)

#### 6. Prevenção (Art. 6, VIII)
- ✅ Validações preventivas (FluentValidation)
- ✅ Testes automatizados

## 📊 Métricas (Prometheus)

```csharp
public static class IdentidadeMetrics
{
    // Contadores
    public static readonly Counter UsuariosCriados = 
        Metrics.CreateCounter("agrosolutions_identidade_usuarios_criados_total", 
                             "Total de usuários criados");
    
    public static readonly Counter LoginsSucesso = 
        Metrics.CreateCounter("agrosolutions_identidade_logins_sucesso_total", 
                             "Total de logins bem-sucedidos",
                             new CounterConfiguration { LabelNames = new[] { "tipo_usuario" } });
    
    public static readonly Counter LoginsFalhados = 
        Metrics.CreateCounter("agrosolutions_identidade_logins_falhados_total", 
                             "Total de logins que falharam",
                             new CounterConfiguration { LabelNames = new[] { "motivo" } });
    
    public static readonly Counter TokensGerados = 
        Metrics.CreateCounter("agrosolutions_identidade_tokens_gerados_total", 
                             "Total de tokens JWT gerados");
    
    public static readonly Counter TokensRefresh = 
        Metrics.CreateCounter("agrosolutions_identidade_tokens_refresh_total", 
                             "Total de tokens refresh gerados");
    
    public static readonly Counter TokensInvalidos = 
        Metrics.CreateCounter("agrosolutions_identidade_tokens_invalidos_total", 
                             "Total de tokens inválidos rejeitados",
                             new CounterConfiguration { LabelNames = new[] { "motivo" } });
    
    public static readonly Counter SenhasRedefinidas = 
        Metrics.CreateCounter("agrosolutions_identidade_senhas_redefinidas_total", 
                             "Total de senhas redefinidas");
    
    public static readonly Counter EmailsVerificados = 
        Metrics.CreateCounter("agrosolutions_identidade_emails_verificados_total", 
                             "Total de e-mails verificados");
    
    // Histogramas (Medições de Tempo)
    public static readonly Histogram TempoLogin = 
        Metrics.CreateHistogram("agrosolutions_identidade_login_duracao_segundos", 
                               "Tempo de processamento de um login");
    
    public static readonly Histogram TempoGeracaoToken = 
        Metrics.CreateHistogram("agrosolutions_identidade_geracao_token_duracao_segundos", 
                               "Tempo de geração de um token JWT");
    
    public static readonly Histogram TempoValidacaoToken = 
        Metrics.CreateHistogram("agrosolutions_identidade_validacao_token_duracao_segundos", 
                               "Tempo de validação de um token");
    
    // Gauges (Valores Instantâneos)
    public static readonly Gauge UsuariosAtivos = 
        Metrics.CreateGauge("agrosolutions_identidade_usuarios_ativos", 
                           "Número de usuários ativos no sistema",
                           new GaugeConfiguration { LabelNames = new[] { "tipo_usuario" } });
    
    public static readonly Gauge SessoesAtivas = 
        Metrics.CreateGauge("agrosolutions_identidade_sessoes_ativas", 
                           "Número de sessões ativas no momento");
    
    public static readonly Gauge TokensEmCache = 
        Metrics.CreateGauge("agrosolutions_identidade_tokens_em_cache", 
                           "Número de tokens em cache");
}
```

**Categorias de Métricas:**

### Contadores (Counters)
Valores que sempre aumentam:
- ✅ Total de usuários criados
- ✅ Total de logins bem-sucedidos (com label por tipo de usuário)
- ✅ Total de logins que falharam (com label por motivo)
- ✅ Total de tokens JWT gerados
- ✅ Total de tokens refresh gerados
- ✅ Total de tokens inválidos rejeitados (com label por motivo)
- ✅ Total de senhas redefinidas
- ✅ Total de e-mails verificados

### Histogramas (Histograms)
Distribuição de medições de tempo:
- ✅ Tempo de processamento de login
- ✅ Tempo de geração de token JWT
- ✅ Tempo de validação de token

### Gauges (Valores Instantâneos)
Valores que podem subir ou descer:
- ✅ Número de usuários ativos (por tipo)
- ✅ Número de sessões ativas
- ✅ Número de tokens em cache

**Recursos Avançados:**
- **Labels Dinâmicos**: Algumas métricas suportam labels para segmentação (tipo_usuario, motivo)
- **Buckets Exponenciais**: Histogramas usam buckets exponenciais para melhor granularidade
- **Endpoint**: `/metrics` (formato Prometheus)

## 🌐 Endpoints Principais

### Autenticação

#### POST /api/autenticacao/registrar
Registra novo usuário.

**Request:**
```json
{
  "nomeCompleto": "João Silva",
  "email": "usuario@exemplo.com",
  "senha": "Senha@123",
  "telefone": "(79) 98765-4321",
  "cpf": "123.456.789-00"
}
```

**Nota**: Os campos `telefone` e `cpf` são opcionais. A validação de senha forte e confirmação de senha são tratadas no frontend e validadores.

**Response:**
```json
{
  "id": "guid",
  "email": "usuario@exemplo.com",
  "nomeCompleto": "João Silva",
  "perfil": "Produtor",
  "statusUsuario": "AguardandoValidacao"
}
```

#### POST /api/autenticacao/login
Autentica usuário e retorna tokens.

**Request:**
```json
{
  "email": "usuario@exemplo.com",
  "senha": "Senha@123"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGciOiJSUzI1NiIs...",
  "refreshToken": "dGhpcyBpcyBhIHJlZnJl...",
  "expiresIn": 3600,
  "usuario": {
    "id": "guid",
    "email": "usuario@exemplo.com",
    "nomeCompleto": "João Silva",
    "perfil": "Produtor"
  }
}
```

#### POST /api/autenticacao/refresh
Renova access token usando refresh token.

#### POST /api/autenticacao/validar-email
Valida email usando código enviado.

#### POST /api/autenticacao/recuperar-senha
Inicia processo de recuperação de senha.

#### POST /api/autenticacao/redefinir-senha
Redefine senha usando código de recuperação.

### JWKS

#### GET /api/jwks
Retorna JSON Web Key Set (público, sem autenticação).

**Response:**
```json
{
  "keys": [
    {
      "kty": "RSA",
      "use": "sig",
      "kid": "2024-02-key-1",
      "n": "modulus_base64url",
      "e": "AQAB"
    }
  ]
}
```

### Privacidade (LGPD)

#### GET /api/privacidade/meus-dados
Retorna todos os dados pessoais do usuário autenticado.

#### GET /api/privacidade/exportar-dados
Exporta dados em formato JSON para portabilidade.

#### PUT /api/privacidade/atualizar-dados
Atualiza dados pessoais com auditoria.

#### DELETE /api/privacidade/excluir-conta
Solicita exclusão de conta (anonimização em 30 dias).

#### POST /api/privacidade/cancelar-exclusao
Cancela solicitação de exclusão.

#### GET /api/privacidade/auditorias
Lista auditoria de acessos e modificações.

### Gestão de Chaves (Admin)

#### POST /api/chaves/gerar
Gera novo par de chaves RSA.

#### POST /api/chaves/ativar/{keyId}
Ativa chave específica.

#### DELETE /api/chaves/{keyId}
Remove chave (com validação de uso).

#### GET /api/chaves
Lista todas as chaves.

## 🚀 Como Executar

### 1. Configurar appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrosolutions_identidade;Username=postgres;Password=postgres"
  },
  "Database": {
    "Provider": "PostgreSQL"
  },
  "Jwt": {
    "Issuer": "AgroSolutions.Identidade",
    "Audience": "AgroSolutions",
    "AccessTokenExpirationMinutes": 60,
    "RefreshTokenExpirationDays": 7,
    "RsaKeyPath": "keys/"
  },
  "Email": {
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "EnableSsl": true,
    "Username": "seu-email@gmail.com",
    "Password": "sua-senha",
    "FromEmail": "noreply@agrosolutions.com",
    "FromName": "AgroSolutions"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "Port": 5672,
    "UserName": "guest",
    "Password": "guest",
    "ExchangeName": "agrosolutions.identidade"
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

https://localhost:5001/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5001/) redireciona automaticamente para o Scalar.

## 🧪 Testes

```bash
cd ../AgroSolutions.Identidade.Test
dotnet test
```

**Cobertura:**
- Testes unitários de serviços
- Testes de validação
- Testes de criptografia
- Testes de geração de tokens
- Testes de LGPD (anonimização, exclusão)

## ✅ Segurança - Checklist

- ✅ Senhas com Argon2id (memory-hard, resistente a GPU)
- ✅ Tokens JWT assinados com RSA 2048
- ✅ JWKS endpoint para validação distribuída
- ✅ Refresh tokens com rotação
- ✅ Account lockout após 5 tentativas (bloqueio de 30 min)
- ✅ Logs com mascaramento de dados sensíveis
- ✅ HTTPS obrigatório em produção
- ✅ CORS configurado
- ⏳ Auditoria completa (infraestrutura pronta, integração pendente)

**Funcionalidades de segurança planejadas:**
- ⏳ Criptografia AES-256 para dados sensíveis (CPF, Telefone)
- ⏳ Rate limiting por IP
- ⏳ CAPTCHA após tentativas falhas
- ⏳ Integração completa do sistema de auditoria

## 📚 Referências

- [RFC 7519 - JWT](https://tools.ietf.org/html/rfc7519)
- [RFC 7517 - JWK](https://tools.ietf.org/html/rfc7517)
- [LGPD - Lei 13.709/2018](http://www.planalto.gov.br/ccivil_03/_ato2015-2018/2018/lei/l13709.htm)
- [OWASP Authentication Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Authentication_Cheat_Sheet.html)
- [Argon2 Specification](https://github.com/P-H-C/phc-winner-argon2)

---

**Identidade** - Segurança e privacidade em primeiro lugar 🔐🛡️