# 🤖 AgroSolutions.Análise

## 🎯 Visão Geral

O microserviço de **Análise** é o cérebro da plataforma, contendo o motor de regras inteligente que avalia dados processados e gera alertas contextualizados para os produtores rurais.

## 📋 Responsabilidades

- **Motor de Regras**: Avaliação de regras de negócio configuráveis
- **Geração de Alertas**: Criação de alertas com severidade e recomendações
- **Gestão de Regras**: CRUD de regras de alertas personalizadas
- **Sincronização de Talhões**: Cache local de informações de talhões
- **Event Publishing**: Publicação de alertas gerados

## 💻 Tecnologias

- **.NET 10** / **C# 14** / **EF Core 10**
- **RabbitMQ** - Consumer e Publisher
- **Rules Engine Pattern** - Motor de regras flexível
- **PostgreSQL** / **SQL Server**

## 🏗️ Estrutura

```
Analise/
├── API/Controllers/
│   └── AlertasController.cs                   # Consulta de alertas
├── Application/
│   ├── Events/
│   │   ├── AlertaGeradoEvent.cs               # Evento de alerta gerado
│   │   └── DadosProcessadosEvent.cs           # Evento consumido
│   └── Services/
│       ├── AlertaService.cs                   # Serviço de alertas
│       ├── MotorRegrasService.cs              # Motor de regras
│       └── RegraAlertaService.cs              # Gestão de regras
├── Domain/
│   ├── Entities/
│   │   ├── Alerta.cs                          # Entidade de alerta
│   │   ├── RegraAlerta.cs                     # Entidade de regra
│   │   └── TalhaoInfo.cs                      # Cache de talhão
│   └── Enums/
│       └── Enums.cs                           # TipoAlerta, NivelSeveridade
├── Infrastructure/
│   ├── Services/
│   │   ├── RabbitMQAnaliseConsumerService.cs  # Consumer de dados processados
│   │   ├── TalhaoSyncConsumerService.cs       # Sync de talhões
│   │   └── RabbitMQAlertaPublisherService.cs  # Publisher de alertas
│   └── Metrics/
│       └── AnaliseMetrics.cs                  # Métricas Prometheus
└── Configuration/Settings/
    ├── MotorRegrasSettings.cs                 # Configurações do motor
    ├── RegraCalorExcessivoSettings.cs         # Regra de calor
    ├── RegraSecaSettings.cs                   # Regra de seca
    ├── RegraExcessoUmidadeSettings.cs         # Regra de umidade
    ├── RegraGeadaSettings.cs                  # Regra de geada
    └── RegraRiscoPragaSettings.cs             # Regra de pragas
```

## 📊 Modelo de Dados

### Alerta
```csharp
public class Alerta
{
    public Guid Id { get; set; }
    public Guid TalhaoId { get; set; }
    public TipoAlerta Tipo { get; set; }
    public NivelSeveridade Severidade { get; set; }
    public StatusAlerta Status { get; set; }          // Ativo, Visualizado, Resolvido
    public string Titulo { get; set; }
    public string Mensagem { get; set; }
    public string? Recomendacao { get; set; }
    public decimal? ValorReferencia { get; set; }
    public DateTime DataGeracao { get; set; }
    public DateTime? DataVisualizacao { get; set; }
    public DateTime? DataResolucao { get; set; }
    public string? DadosAdicionais { get; set; }      // JSON com dados extras
    public Guid? UsuarioId { get; set; }
}
```

### RegraAlerta
```csharp
public class RegraAlerta
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public string? Descricao { get; set; }
    public TipoAlerta TipoAlerta { get; set; }
    public NivelSeveridade Severidade { get; set; }
    public bool Ativa { get; set; }
    public string Condicao { get; set; }              // JSON com condições flexíveis
    public string TemplateMensagem { get; set; }
    public string? Recomendacao { get; set; }
    public DateTime DataCriacao { get; set; }
    public DateTime? DataAtualizacao { get; set; }
}
```

### TalhaoInfo
```csharp
public class TalhaoInfo
{
    public Guid Id { get; set; }
    public string Nome { get; set; }
    public Guid PropriedadeId { get; set; }
    public Guid ProprietarioId { get; set; }
    public string EmailProprietario { get; set; }
    public string NomeProprietario { get; set; }
    public DateTime DataSincronizacao { get; set; }
}
```

## 🎨 Design Patterns

### Arquiteturais
- **Rules Engine Pattern**: Motor de regras configurável
- **Event-Driven Architecture**: Processamento assíncrono

### Estruturais
- **Repository Pattern**: Abstração de persistência

### Comportamentais
- **Observer Pattern**: Publicação de alertas

## 🎯 Motor de Regras

### Regras Pré-Configuradas

As regras são configuradas via **appsettings.json** usando classes Settings:

#### 1. Calor Excessivo
```csharp
public class RegraCalorExcessivoSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdTemperatura { get; set; } = 35;  // °C
    public int DuracaoHoras { get; set; } = 6;               // Horas consecutivas
    public decimal SeveridadeCritico { get; set; } = 42;     // °C
    public decimal SeveridadeAlto { get; set; } = 38;        // °C
}
```

#### 2. Risco de Seca
```csharp
public class RegraSecaSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdUmidade { get; set; } = 30;      // %
    public int DuracaoHoras { get; set; } = 24;              // Horas consecutivas
    public decimal SeveridadeCritico { get; set; } = 15;     // %
    public decimal SeveridadeAlto { get; set; } = 20;        // %
}
```

#### 3. Excesso de Umidade
```csharp
public class RegraExcessoUmidadeSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdUmidade { get; set; } = 80;      // %
    public int DuracaoHoras { get; set; } = 24;
    public decimal SeveridadeAlto { get; set; } = 90;        // %
}
```

#### 4. Risco de Geada
```csharp
public class RegraGeadaSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdTemperatura { get; set; } = 2;   // °C
    public int DuracaoHoras { get; set; } = 3;
    public decimal SeveridadeCritico { get; set; } = 0;      // °C
    public decimal SeveridadeAlto { get; set; } = 1;         // °C
}
```

#### 5. Risco de Pragas
```csharp
public class RegraRiscoPragaSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal TemperaturaMinima { get; set; } = 25;     // °C
    public decimal TemperaturaMaxima { get; set; } = 30;     // °C
    public decimal UmidadeMinima { get; set; } = 70;         // %
    public int DuracaoHoras { get; set; } = 12;
}
```

### Avaliação de Regras

```csharp
public class MotorRegrasService
{
    // Cache em memória das últimas leituras (48 horas)
    private static readonly ConcurrentDictionary<Guid, List<LeituraCache>> _leiturasCache = new();

    public async Task ProcessarLeituraEAvaliarRegrasAsync(LeituraParaAnaliseDto leitura)
    {
        // 1. Armazena leitura no cache em memória
        ArmazenarLeituraNoCache(leitura);
        
        // 2. Avalia todas as regras aplicáveis
        await AvaliarRegrasParaTalhaoAsync(leitura.TalhaoId);
    }
    
    private async Task AvaliarRegrasParaTalhaoAsync(Guid talhaoId)
    {
        // Avalia cada regra se estiver habilitada nas configurações
        if (_settings.RegrasSeca.Habilitada)
            await AvaliarRegraSecaAsync(talhaoId);
        
        if (_settings.RegrasGeada.Habilitada)
            await AvaliarRegraGeadaAsync(talhaoId);
        
        if (_settings.RegrasCalorExcessivo.Habilitada)
            await AvaliarRegraCalorExcessivoAsync(talhaoId);
        
        if (_settings.RegrasExcessoUmidade.Habilitada)
            await AvaliarRegraExcessoUmidadeAsync(talhaoId);
        
        if (_settings.RegrasRiscoPraga.Habilitada)
            await AvaliarRiscoPragaAsync(talhaoId);
    }
    
    private async Task AvaliarRegraSecaAsync(Guid talhaoId)
    {
        var config = _settings.RegrasSeca;
        var leituras = ObterLeiturasDoCache(talhaoId, config.DuracaoHoras, tipoSensor: 3);
        
        var mediaUmidade = leituras.Average(l => l.Valor);
        var todasAbaixoThreshold = leituras.All(l => l.Valor < config.ThresholdUmidade);
        
        if (todasAbaixoThreshold)
        {
            var jaExisteAlerta = await _alertaRepository.ExisteAlertaAtivoAsync(talhaoId, TipoAlerta.Seca);
            if (!jaExisteAlerta)
            {
                await _alertaService.CriarAsync(new CriarAlertaDto(
                    TalhaoId: talhaoId,
                    Tipo: TipoAlerta.Seca,
                    Severidade: CalcularSeveridade(mediaUmidade, config),
                    Titulo: "Alerta de Seca",
                    Mensagem: $"Umidade do solo abaixo de {config.ThresholdUmidade}% por mais de {config.DuracaoHoras} horas",
                    Recomendacao: "Recomenda-se irrigação imediata",
                    ValorReferencia: mediaUmidade
                ));
            }
        }
    }
}
```

**Características do Motor:**
- ✅ **Cache em Memória**: Mantém últimas 48 horas de leituras
- ✅ **Métodos Específicos**: Cada regra tem lógica própria
- ✅ **Evita Duplicação**: Verifica se alerta já existe antes de criar
- ✅ **Cálculo de Severidade**: Dinâmico baseado em thresholds configurados
- ✅ **Período de Cobertura**: Valida se tem dados suficientes antes de avaliar

## 📨 Eventos

### Consumido: DadosProcessadosEvent
```csharp
// Recebe de: ProcessamentoDados
// Ação: Avalia regras e gera alertas
```

### Consumido: TalhaoCriadoEvent / TalhaoAtualizadoEvent
```csharp
// Recebe de: Propriedades
// Ação: Sincroniza cache local de talhões
```

### Publicado: AlertaGeradoEvent
```csharp
public record AlertaGeradoEvent(
    Guid AlertaId,
    Guid TalhaoId,
    TipoAlerta Tipo,
    NivelSeveridade Severidade,
    string Titulo,
    string Mensagem,
    string? Recomendacao,
    DateTime DataGeracao,
    decimal? ValorReferencia,
    Guid? DestinatarioId,
    string? EmailDestinatario,
    string? NomeDestinatario
);
```

**Exchange**: `agrosolutions.analise`  
**Routing Key**: `alerta.gerado`  
**Consumidores**: Notificações (para envio aos produtores)

## 🌐 Endpoints

### GET /api/alertas/{id}
Obtém alerta por ID.

### GET /api/alertas/talhao/{talhaoId}
Obtém todos os alertas de um talhão.

### GET /api/alertas/ativos
Obtém todos os alertas ativos.

### POST /api/alertas
Criar novo alerta manualmente (requer role Admin ou Técnico).

### PATCH /api/alertas/{id}/status
Atualizar status do alerta.

### PUT /api/alertas/{id}/visualizar
Marca alerta como visualizado.

### PUT /api/alertas/{id}/resolver
Marca alerta como resolvido.

### GET /api/alertas/estatisticas
Obtém estatísticas de alertas.

**Response:**
```json
{
  "totalAlertas": 150,
  "alertasPorSeveridade": {
    "Critico": 5,
    "Alto": 20,
    "Medio": 80,
    "Baixo": 40,
    "Informativo": 5
  },
  "alertasPorTipo": {
    "CalorExcessivo": 30,
    "Seca": 40,
    "ExcessoUmidade": 25,
    "Geada": 5,
    "RiscoPraga": 45,
    "IrrigacaoRecomendada": 5
  },
  "taxaResolucao": 85.5
}
```

## 🎯 Métricas

```csharp
public static class AnaliseMetrics
{
    // Métricas para Dashboard de Talhões
    public static readonly Gauge TalhaoStatus = 
        Metrics.CreateGauge(
            "agrosolutions_talhao_status",
            "Status atual do talhão (0=Crítico, 1=Alerta, 2=Normal)",
            new GaugeConfiguration { LabelNames = new[] { "talhao_id", "talhao_nome", "cultura" } });

    public static readonly Gauge AlertasAtivos = 
        Metrics.CreateGauge(
            "agrosolutions_alertas_ativos",
            "Alertas atualmente ativos por tipo (0=Inativo, 1=Ativo)",
            new GaugeConfiguration { LabelNames = new[] { "tipo", "talhao_nome", "talhao_id" } });

    public static readonly Counter AlertasGeradosPorTipo = 
        Metrics.CreateCounter(
            "agrosolutions_alertas_gerados_total",
            "Total de alertas gerados por tipo",
            new CounterConfiguration { LabelNames = new[] { "tipo", "severidade" } });

    // Métricas gerais
    public static readonly Counter RegrasAvaliadas = 
        Metrics.CreateCounter(
            "agrosolutions_analise_regras_avaliadas_total",
            "Total de regras de alerta avaliadas",
            new CounterConfiguration { LabelNames = new[] { "tipo_regra" } });

    public static readonly Counter AlertasGerados = 
        Metrics.CreateCounter(
            "agrosolutions_analise_alertas_gerados_total",
            "Total de alertas gerados",
            new CounterConfiguration { LabelNames = new[] { "severidade", "tipo_regra" } });

    public static readonly Counter AlertasPublicados = 
        Metrics.CreateCounter(
            "agrosolutions_analise_alertas_publicados_total",
            "Total de alertas publicados no RabbitMQ",
            new CounterConfiguration { LabelNames = new[] { "severidade" } });
}
```

## 🌟 Pontos Fortes

### 1. Motor de Regras Flexível
- **Configurável**: Regras definidas via C# Settings no appsettings.json
- **Habilitação Dinâmica**: Regras podem ser ativadas/desativadas em runtime via configuração
- **Extensível**: Fácil adição de novas regras implementando métodos específicos
- **Cache em Memória**: Mantém últimas 48 horas de leituras para avaliação eficiente
- **Thresholds Customizáveis**: Cada regra tem thresholds configuráveis para severidade

### 2. Alertas Contextualizados
- **Severidade**: Crítico, Alto, Médio, Baixo, Informativo
- **Recomendações**: Ações sugeridas por especialistas
- **Valor de Referência**: Dados que geraram o alerta
- **Destinatário Identificado**: Email e nome do produtor
- **Status Detalhado**: Ativo, Visualizado, EmAndamento, Resolvido, Ignorado

### 3. Cache Local de Talhões
- **Performance**: Evita consultas síncronas
- **Disponibilidade**: Funciona mesmo se Propriedades estiver offline
- **Sincronização**: Atualização via eventos

### 4. Histórico e Estatísticas
- **Rastreabilidade**: Todo alerta registrado
- **Análise de Tendências**: Estatísticas agregadas
- **Taxa de Resolução**: Métrica de eficácia

## 🚀 Como Executar

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=agrosolutions_analise;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True"
  },
  "Database": {
    "Provider": "SqlServer"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "ExchangeName": "agrosolutions.analise"
  },
  "MotorRegras": {
    "StartupDelaySeconds": 10,
    "IntervaloAvaliacaoMinutos": 5,
    "RegrasCalorExcessivo": {
      "Habilitada": true,
      "ThresholdTemperatura": 35.0,
      "DuracaoHoras": 6,
      "SeveridadeCritico": 42.0,
      "SeveridadeAlto": 38.0
    },
    "RegrasSeca": {
      "Habilitada": true,
      "ThresholdUmidade": 30.0,
      "DuracaoHoras": 24,
      "SeveridadeCritico": 15.0,
      "SeveridadeAlto": 20.0
    },
    "RegrasGeada": {
      "Habilitada": true,
      "ThresholdTemperatura": 2.0,
      "DuracaoHoras": 3
    },
    "RegrasExcessoUmidade": {
      "Habilitada": true,
      "ThresholdUmidade": 80.0,
      "DuracaoHoras": 24
    },
    "RegrasRiscoPraga": {
      "Habilitada": true,
      "TemperaturaMinima": 25.0,
      "TemperaturaMaxima": 30.0,
      "UmidadeMinima": 70.0,
      "DuracaoHoras": 12
    }
  }
}
```

```bash
dotnet ef database update
dotnet run
```

**Documentação da API**: https://localhost:5006/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5006/) redireciona automaticamente para o Scalar.

---

**Análise** - Inteligência artificial para decisões agrícolas ????
