# 📊 AgroSolutions.ProcessamentoDados

## 🎯 Visão Geral

O microserviço de **ProcessamentoDados** é responsável por agregar e processar leituras de sensores, gerando estatísticas e insights que alimentam o motor de regras de análise.

## 📋 Responsabilidades

- **Agregação de Dados**: Cálculo de médias, min, max, desvio padrão
- **Janelas Temporais**: Agregações por hora, dia, semana, mês
- **Classificação de Qualidade**: Análise de qualidade das leituras (Normal, Suspeita, Inválida)
- **Armazenamento de Agregações**: Persistência de dados processados
- **Event Publishing**: Publicação de dados processados para análise

## 💻 Tecnologias

- **.NET 10** / **C# 14** / **EF Core 10**
- **RabbitMQ** - Consumer e Publisher
- **Background Service** - Processamento contínuo
- **SQL Server** - Banco de dados padrão (alternativa: PostgreSQL)

## 🏗️ Estrutura

```
ProcessamentoDados/
├── API/Controllers/
│   ├── AgregacoesController.cs           # Consulta de agregações
│   └── LeiturasController.cs             # Consulta de leituras processadas
├── Application/
│   ├── Events/
│   │   └── DadosProcessadosEvent.cs      # Evento de dados agregados
│   │   └── LeituraRecebidaEvent.cs       # Evento consumido
│   ├── Services/
│   │   ├── AgregacaoService.cs           # Serviço de agregação
│   │   └── ProcessamentoService.cs       # Serviço de processamento
├── Domain/
│   └── Entities/
│       ├── AgregacaoDados.cs             # Dados agregados
│       └── LeituraProcessada.cs          # Leitura processada
└── Infrastructure/
    ├── Services/
    │   └── RabbitMQConsumerService.cs    # Consumer de leituras
    └── Metrics/
        └── ProcessamentoDadosMetrics.cs  # Métricas Prometheus
```

## 📊 Modelo de Dados

### LeituraProcessada
```csharp
public class LeituraProcessada
{
    public Guid Id { get; private set; }
    public Guid LeituraOrigemId { get; private set; }             // ID da leitura original (IngestaoDados)
    public Guid SensorId { get; private set; }
    public string DeviceId { get; private set; }
    public Guid PropriedadeId { get; private set; }
    public Guid? TalhaoId { get; private set; }
    public TipoSensor TipoSensor { get; private set; }
    public decimal Valor { get; private set; }
    public string Unidade { get; private set; }
    public DateTime TimestampLeitura { get; private set; }        // Timestamp do sensor
    public DateTime TimestampRecebimento { get; private set; }    // Timestamp do IngestaoDados
    public DateTime TimestampProcessamento { get; private set; }  // Timestamp do processamento
    public QualidadeLeitura Qualidade { get; private set; }
    public StatusProcessamento Status { get; private set; }       // Processado, Falha
    public int? NivelBateria { get; private set; }
    public int? IntensidadeSinal { get; private set; }
    public string? DadosAdicionais { get; private set; }
    public string? MensagemErro { get; private set; }
}
```

### AgregacaoDados
```csharp
public class AgregacaoDados
{
    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    public string DeviceId { get; private set; }
    public Guid PropriedadeId { get; private set; }
    public Guid? TalhaoId { get; private set; }
    public TipoSensor TipoSensor { get; private set; }
    public TipoAgregacao TipoAgregacao { get; private set; }      // Horaria, Diaria, Semanal, Mensal
    public DateTime PeriodoInicio { get; private set; }
    public DateTime PeriodoFim { get; private set; }
    public int TotalLeituras { get; private set; }
    public decimal? ValorMinimo { get; private set; }
    public decimal? ValorMaximo { get; private set; }
    public decimal? ValorMedio { get; private set; }
    public decimal? DesvioPadrao { get; private set; }
    public string Unidade { get; private set; }
    public int LeiturasNormais { get; private set; }
    public int LeiturasSuspeitas { get; private set; }
    public int LeiturasInvalidas { get; private set; }
}
```

## 📨 Eventos

### Consumido: LeituraRecebidaEvent
```csharp
// Recebe de: IngestaoDados
// Ação: Processa e agrega leitura
```

### Publicado: DadosProcessadosEvent
```csharp
public record DadosProcessadosEvent(
    Guid Id,
    Guid LeituraOrigemId,
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    Guid? TalhaoId,
    TipoSensor TipoSensor,
    decimal Valor,
    string Unidade,
    DateTime TimestampLeitura,
    DateTime TimestampProcessamento,
    QualidadeLeitura Qualidade,
    int? NivelBateria,
    int? IntensidadeSinal,
    string? DadosAdicionais
);
```

**Nota**: Este evento representa uma **leitura individual processada**, não uma agregação. Cada leitura recebida do IngestaoDados gera um evento DadosProcessadosEvent após ser armazenada.

**Exchange**: `agrosolutions.processamento`  
**Routing Key**: `dados.processados`  
**Consumidores**: Análise (para avaliação de regras)

## 🎨 Design Patterns

### Arquiteturais
- **Event-Driven Architecture**: Pipeline assíncrono
- **Batch Processing**: Processamento em lotes para eficiência
- **Time-Series Pattern**: Otimizado para séries temporais

### Estruturais
- **Repository Pattern**: Abstração de persistência

### Comportamentais
- **Observer Pattern**: Consumer de eventos

## 📊 Agregações

### Tipos de Agregação

1. **Horária**: Últimas 24 horas, agregação por hora
2. **Diária**: Últimos 30 dias, agregação por dia
3. **Semanal**: Últimas 12 semanas, agregação por semana
4. **Mensal**: Últimos 12 meses, agregação por mês

### Métricas Calculadas

- **Valor Médio**: Média aritmética
- **Valor Mínimo**: Valor mínimo registrado
- **Valor Máximo**: Valor máximo registrado
- **Desvio Padrão**: Variabilidade dos dados
- **Número de Leituras**: Quantidade de leituras consideradas
- **Qualidade**: Contagem de leituras normais, suspeitas e inválidas

## 🌐 Endpoints

### Agregações

#### GET /api/agregacoes
Consulta agregações com filtros.

**Query Params:**
- `sensorId`: ID do sensor
- `propriedadeId`: ID da propriedade
- `talhaoId`: ID do talhão
- `tipoSensor`: Tipo de sensor
- `tipoAgregacao`: Horaria, Diaria, Semanal, Mensal
- `periodoInicio`: Data início
- `periodoFim`: Data fim

#### GET /api/agregacoes/{id}
Obtém agregação por ID.

#### POST /api/agregacoes/gerar-horaria
Gera agregação horária manualmente.

#### POST /api/agregacoes/gerar-diaria
Gera agregação diária manualmente.

#### POST /api/agregacoes/gerar-semanal
Gera agregação semanal manualmente.

#### POST /api/agregacoes/gerar-mensal
Gera agregação mensal manualmente.

### Leituras

#### GET /api/leituras
Consulta leituras processadas (paginado).

**Query Params:**
- `sensorId`: ID do sensor
- `propriedadeId`: ID da propriedade
- `talhaoId`: ID do talhão
- `dataInicio`: Data início
- `dataFim`: Data fim
- `status`: Status do processamento
- `qualidade`: Qualidade da leitura
- `pagina`: Número da página
- `tamanhoPagina`: Tamanho da página

#### GET /api/leituras/{id}
Obtém leitura processada por ID.

#### GET /api/leituras/estatisticas
Obtém estatísticas de processamento.

#### POST /api/leituras/reprocessar-falhas
Reprocessa leituras com falha.

## 🎯 Métricas

```csharp
public static class ProcessamentoDadosMetrics
{
    // Contadores
    public static readonly Counter LeiturasProcessadas = 
        Metrics.CreateCounter(
            "agrosolutions_processamento_leituras_processadas_total",
            "Total de leituras processadas com sucesso",
            new CounterConfiguration { LabelNames = new[] { "tipo_sensor" } });

    public static readonly Counter LeiturasComErro = 
        Metrics.CreateCounter(
            "agrosolutions_processamento_leituras_erro_total",
            "Total de leituras que falharam no processamento",
            new CounterConfiguration { LabelNames = new[] { "tipo_erro" } });

    public static readonly Counter MensagensRabbitMQRecebidas = 
        Metrics.CreateCounter(
            "agrosolutions_processamento_rabbitmq_mensagens_recebidas_total",
            "Total de mensagens recebidas do RabbitMQ");

    // Histogramas
    public static readonly Histogram TempoProcessamentoLeitura = 
        Metrics.CreateHistogram(
            "agrosolutions_processamento_leitura_duracao_segundos",
            "Tempo de processamento de uma leitura",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 10),
                LabelNames = new[] { "tipo_sensor" }
            });

    public static readonly Histogram TempoProcessamentoAgregacao = 
        Metrics.CreateHistogram(
            "agrosolutions_processamento_agregacao_duracao_segundos",
            "Tempo de processamento de agregação de dados",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
                LabelNames = new[] { "tipo_agregacao" }
            });

    // Gauges
    public static readonly Gauge LeiturasEmProcessamento = 
        Metrics.CreateGauge(
            "agrosolutions_processamento_leituras_em_processamento",
            "Número de leituras sendo processadas no momento");
}
```

## 🌟 Pontos Fortes

### 1. Processamento Eficiente
- **Batch Processing**: Processa lotes para eficiência
- **Window Functions**: Agregações por janelas temporais
- **Indexed Queries**: Consultas otimizadas

### 2. Múltiplos Níveis de Agregação
- **Horária**: Para análise recente
- **Diária**: Para tendências de médio prazo
- **Semanal**: Para análise de longo prazo

## 🚀 Como Executar

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=agrosolutions_processamento;Username=postgres;Password=postgres"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "ExchangeName": "agrosolutions.processamento"
  },
  "Processamento": {
    "IntervaloAgregacaoMinutos": 60,
    "TamanhoLoteProcessamento": 1000
  }
}
```

```bash
dotnet ef database update
dotnet run
```

**Documentação da API**: https://localhost:5005/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5005/) redireciona automaticamente para o Scalar.

---

**ProcessamentoDados** - Transformando dados brutos em insights ????
