# ?? AgroSolutions.IngestaoDados

## ?? Visão Geral

O microserviço de **IngestaoDados** é o ponto de entrada de todas as leituras provenientes de sensores IoT na plataforma AgroSolutions. Responsável por receber, validar e armazenar dados de sensores, além de publicar eventos para processamento downstream.

## ?? Responsabilidades

- **Recepção de Leituras**: Endpoint REST para receber dados de sensores
- **Validação**: Validação de dados, qualidade e integridade
- **Cadastro de Sensores**: Gestão do catálogo de sensores
- **Event Publishing**: Publicação de eventos de leituras recebidas
- **Alertas Técnicos**: Detecção de problemas técnicos (bateria baixa, sinal fraco)
- **Métricas de IoT**: Monitoramento de taxa de leituras e qualidade

## ?? Tecnologias e Técnicas Aplicadas

### Core
- **.NET 10** / **C# 14** / **ASP.NET Core**
- **Entity Framework Core 10** - ORM
- **PostgreSQL** / **SQL Server** - Banco de dados

### Mensageria e IoT
- **RabbitMQ** - Publicação de eventos
- **Event-Driven Architecture** - Pipeline assíncrono
- **High Throughput Design** - Otimizado para alto volume de leituras

### Observabilidade
- **Prometheus** - Métricas de IoT (taxa de leituras, latência, qualidade)
- **Health Checks** - Monitoramento de saúde

## ??? Estrutura

```
AgroSolutions.IngestaoDados/
??? API/
?   ??? Controllers/
?       ??? LeiturasController.cs         # Recepção de leituras
?       ??? SensoresController.cs         # Gestão de sensores
??? Application/
?   ??? DTOs/
?   ?   ??? SensoresDtos.cs               # DTOs de sensores e leituras
?   ??? Events/
?   ?   ??? AlertaSensorEvent.cs          # Evento de alerta técnico
?   ?   ??? LeituraRecebidaEvent.cs       # Evento de leitura recebida
?   ??? Interfaces/
?   ?   ??? ILeituraService.cs            # Serviço de leituras
?   ?   ??? IMensageriaService.cs         # Serviço de mensageria
?   ?   ??? ISensorService.cs             # Serviço de sensores
?   ??? Services/
?   ?   ??? LeituraService.cs             # Implementação leituras
?   ?   ??? SensorService.cs              # Implementação sensores
?   ??? Validators/
?       ??? SensorValidators.cs           # Validações FluentValidation
??? Domain/
?   ??? Entities/
?   ?   ??? LeituraSensor.cs              # Entidade de leitura
?   ?   ??? Sensor.cs                     # Entidade de sensor
?   ??? Enums/
?   ?   ??? Enums.cs                      # TipoSensor, QualidadeLeitura, StatusSensor
?   ??? Interfaces/
?       ??? ILeituraSensorRepository.cs   # Repositório de leituras
?       ??? ISensorRepository.cs          # Repositório de sensores
??? Infrastructure/
?   ??? Data/
?   ?   ??? DbInitializer.cs              # Inicializador com sensores exemplo
?   ?   ??? IngestaoDbContext.cs          # DbContext
?   ??? Metrics/
?   ?   ??? SensorMetrics.cs              # Métricas Prometheus
?   ??? Repositories/
?   ?   ??? LeituraSensorRepository.cs    # Implementação leituras
?   ?   ??? SensorRepository.cs           # Implementação sensores
?   ??? Services/
?       ??? MensageriaService.cs          # Implementação publicação eventos
??? Program.cs
```

## ?? Design Patterns Aplicados

### Arquiteturais
- **Event-Driven Architecture**: Pipeline assíncrono de processamento
- **Clean Architecture**: Separação em camadas
- **High Throughput Pattern**: Otimizado para alto volume

### Estruturais
- **Repository Pattern**: Abstração de persistência

### Comportamentais
- **Observer Pattern**: Publicação de eventos

## ?? Modelo de Dados

### Sensor
```csharp
public class Sensor
{
    public Guid Id { get; private set; }
    public Guid PropriedadeId { get; private set; }
    public Guid? TalhaoId { get; private set; }
    public string DeviceId { get; private set; }              // ID do dispositivo físico
    public string Nome { get; private set; }
    public TipoSensor Tipo { get; private set; }              // Tipo de sensor
    public string? Fabricante { get; private set; }           // Fabricante do sensor
    public string? Modelo { get; private set; }               // Modelo do sensor
    public decimal? Latitude { get; private set; }            // Localização geográfica
    public decimal? Longitude { get; private set; }
    public decimal? Altitude { get; private set; }            // Altitude em metros
    public int IntervaloLeituraMinutos { get; private set; }  // Intervalo de leitura
    public StatusSensor Status { get; private set; }          // Status operacional
    public DateTime? UltimaLeitura { get; private set; }
    public DateTime? UltimaCalibracao { get; private set; }
    
    // Navegação
    public ICollection<LeituraSensor> Leituras { get; private set; }
}
```

**Nota**: `NivelBateria` e `IntensidadeSinal` são propriedades de `LeituraSensor`, não de `Sensor`.
```

### LeituraSensor
```csharp
public class LeituraSensor
{
    public Guid Id { get; private set; }
    public Guid SensorId { get; private set; }
    public decimal Valor { get; private set; }
    public string Unidade { get; private set; }                 // °C, %, mm, etc.
    public DateTime TimestampLeitura { get; private set; }      // Timestamp do sensor
    public DateTime TimestampRecebimento { get; private set; }  // Timestamp do servidor
    public QualidadeLeitura Qualidade { get; private set; }     // Normal, Suspeita, Invalida, Calibrada
    public int? NivelBateria { get; private set; }              // 0-100%
    public int? IntensidadeSinal { get; private set; }          // RSSI em dBm
    public string? DadosAdicionais { get; private set; }        // JSON com dados extras
    public string? Observacoes { get; private set; }            // Observações sobre a leitura
    
    // Navegação
    public Sensor Sensor { get; private set; }
    
    // Métodos auxiliares
    public bool BateriaBaixa() => NivelBateria.HasValue && NivelBateria.Value < 20;
    public bool SinalFraco() => IntensidadeSinal.HasValue && IntensidadeSinal.Value < -80;
}
```

### Enums

#### TipoSensor
```csharp
public enum TipoSensor
{
    Temperatura = 1,
    UmidadeAr = 2,
    UmidadeSolo = 3,
    Precipitacao = 4,           // Pluviômetro
    VelocidadeVento = 5,
    DirecaoVento = 6,
    PressaoAtmosferica = 7,
    PHSolo = 8,                 // pH do solo
    CondutividadeSolo = 9,      // Condutividade elétrica do solo
    RadiacaoSolar = 10
}
```

#### QualidadeLeitura
```csharp
public enum QualidadeLeitura
{
    Normal = 1,      // Leitura dentro dos parâmetros esperados
    Suspeita = 2,    // Leitura suspeita (fora dos padrões habituais)
    Invalida = 3,    // Leitura inválida (erro de sensor)
    Calibrada = 4    // Leitura calibrada/ajustada
}
```

## ?? Eventos Publicados

### LeituraRecebidaEvent
```csharp
public record LeituraRecebidaEvent(
    Guid Id,
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    Guid? TalhaoId,
    TipoSensor TipoSensor,
    decimal Valor,
    string Unidade,
    DateTime TimestampLeitura,
    DateTime TimestampRecebimento,
    QualidadeLeitura Qualidade,
    int? NivelBateria,
    int? IntensidadeSinal,
    bool BateriaBaixa,
    bool SinalFraco,
    TimeSpan LatenciaRecebimento,
    string? DadosAdicionais
);
```

**Exchange**: `agrosolutions.ingestao`  
**Routing Key**: `leitura.recebida`  
**Consumidores**: ProcessamentoDados

### AlertaSensorEvent
```csharp
public record AlertaSensorEvent(
    Guid SensorId,
    string DeviceId,
    Guid PropriedadeId,
    TipoAlerta TipoAlerta,  // BateriaBaixa, SinalFraco, SensorOffline, ValorAnomalo, CalibracaoNecessaria
    string Mensagem,
    DateTime Timestamp
);

public enum TipoAlerta
{
    BateriaBaixa = 1,
    SinalFraco = 2,
    SensorOffline = 3,
    ValorAnomalo = 4,
    CalibracaoNecessaria = 5
}
```

**Exchange**: `agrosolutions.ingestao`  
**Routing Key**: `alerta.sensor`  
**Consumidores**: Notificações

## ?? Endpoints Principais

### Leituras

#### POST /api/leituras
Recebe leitura de sensor.

**Request:**
```json
{
  "sensorId": "guid",
  "deviceId": "SENSOR-001",
  "valor": 25.5,
  "timestampLeitura": "2024-02-03T10:30:00Z",
  "nivelBateria": 85,
  "intensidadeSinal": 90,
  "dadosAdicionais": "{\"firmware\": \"v1.2.3\"}"
}
```

**Response:**
```json
{
  "id": "guid",
  "qualidade": "Excelente",
  "latenciaRecebimento": "00:00:00.1234567",
  "alertasGerados": ["BateriaBaixa"]
}
```

#### POST /api/leituras/lote
Recebe lote de leituras (bulk insert).

**Request:**
```json
{
  "leituras": [
    { "sensorId": "guid", "valor": 25.5, ... },
    { "sensorId": "guid", "valor": 26.0, ... }
  ]
}
```

#### GET /api/leituras/sensor/{sensorId}
Lista leituras de um sensor (paginado).

#### GET /api/leituras/{id}
Obtém leitura por ID.

### Sensores

#### GET /api/sensores
Lista sensores do usuário.

#### GET /api/sensores/{id}
Obtém sensor por ID.

#### GET /api/sensores/propriedade/{propriedadeId}
Lista sensores de uma propriedade.

#### POST /api/sensores
Cadastra novo sensor.

**Request:**
```json
{
  "deviceId": "SENSOR-001",
  "nome": "Sensor Temperatura Talhão A",
  "tipoSensor": "Temperatura",
  "propriedadeId": "guid",
  "talhaoId": "guid",
  "localizacao": "Centro do talhão"
}
```

#### PUT /api/sensores/{id}
Atualiza sensor.

#### DELETE /api/sensores/{id}
Remove sensor (soft delete).

## ?? Métricas (Prometheus)

```csharp
public static class SensorMetrics
{
    // Métricas de leituras de sensores
    public static readonly Counter LeiturasRecebidas = 
        Metrics.CreateCounter(
            "agrosolutions_sensor_leituras_total",
            "Total de leituras de sensores recebidas",
            new CounterConfiguration { 
                LabelNames = new[] { "talhao_id", "talhao_nome" } 
            });

    // Temperatura atual por talhão
    public static readonly Gauge Temperatura = 
        Metrics.CreateGauge(
            "agrosolutions_sensor_temperatura",
            "Temperatura atual do sensor em Celsius",
            new GaugeConfiguration
            {
                LabelNames = new[] { "talhao_id", "talhao_nome", "cultura", "sensor_id" }
            });

    // Umidade do solo atual por talhão
    public static readonly Gauge Umidade = 
        Metrics.CreateGauge(
            "agrosolutions_sensor_umidade",
            "Umidade do solo em porcentagem (0-100)",
            new GaugeConfiguration
            {
                LabelNames = new[] { "talhao_id", "talhao_nome", "cultura", "sensor_id" }
            });

    // Precipitação atual por talhão
    public static readonly Gauge Precipitacao = 
        Metrics.CreateGauge(
            "agrosolutions_sensor_precipitacao",
            "Precipitação em milímetros",
            new GaugeConfiguration
            {
                LabelNames = new[] { "talhao_id", "talhao_nome", "cultura", "sensor_id" }
            });

    // Métricas de gestão de sensores
    public static readonly Counter SensoresCadastrados = 
        Metrics.CreateCounter(
            "agrosolutions_ingestao_sensores_cadastrados_total",
            "Total de sensores cadastrados");

    public static readonly Counter LeiturasProcessadas = 
        Metrics.CreateCounter(
            "agrosolutions_ingestao_leituras_processadas_total",
            "Total de leituras processadas com sucesso");

    public static readonly Counter LeiturasComErro = 
        Metrics.CreateCounter(
            "agrosolutions_ingestao_leituras_erro_total",
            "Total de leituras com erro no processamento");

    public static readonly Histogram TempoProcessamentoLeitura = 
        Metrics.CreateHistogram(
            "agrosolutions_ingestao_processamento_leitura_duracao_segundos",
            "Tempo de processamento de uma leitura de sensor",
            new HistogramConfiguration
            {
                Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
            });

    // Método auxiliar para atualização de leituras
    public static void AtualizarLeituraSensor(
        string talhaoId, 
        string talhaoNome, 
        string cultura, 
        string sensorId,
        double temperatura,
        double umidade,
        double precipitacao)
    {
        Temperatura.WithLabels(talhaoId, talhaoNome, cultura, sensorId).Set(temperatura);
        Umidade.WithLabels(talhaoId, talhaoNome, cultura, sensorId).Set(umidade);
        Precipitacao.WithLabels(talhaoId, talhaoNome, cultura, sensorId).Set(precipitacao);
        LeiturasRecebidas.WithLabels(talhaoId, talhaoNome).Inc();
    }
}
```

**Categorias de Métricas:**

### Métricas de Leituras de Sensores (com labels por talhão)
- ? **LeiturasRecebidas**: Total de leituras recebidas (segmentado por talhão)
- ? **Temperatura**: Valor atual de temperatura (por talhão, cultura e sensor)
- ? **Umidade**: Valor atual de umidade do solo (por talhão, cultura e sensor)
- ? **Precipitacao**: Valor atual de precipitação (por talhão, cultura e sensor)

### Métricas de Gestão
- ? **SensoresCadastrados**: Total de sensores cadastrados no sistema
- ? **LeiturasProcessadas**: Total de leituras processadas com sucesso
- ? **LeiturasComErro**: Total de leituras com erro no processamento

### Métricas de Performance
- ? **TempoProcessamentoLeitura**: Histograma com distribuição do tempo de processamento (buckets exponenciais)

**Recursos Avançados:**
- **Labels Dinâmicos**: Métricas principais segmentadas por talhão, cultura e sensor
- **Método Auxiliar**: `AtualizarLeituraSensor()` atualiza múltiplas métricas atomicamente
- **Buckets Exponenciais**: Histograma otimizado para capturar variações de latência
- **Endpoint**: `/metrics` (formato Prometheus)

## ?? Pontos Fortes

### 1. High Throughput Design
- **Bulk Insert**: Endpoint para lote de leituras
- **Async All The Way**: Operações não-bloqueantes
- **Connection Pooling**: Otimização de conexões DB
- **Indexação Estratégica**: Índices em campos de busca frequente

### 2. Validação Multicamada
- **Validação de Entrada**: FluentValidation nos DTOs
- **Validação de Negócio**: Regras de qualidade e alertas
- **Validação de Sensor**: Verifica se sensor está ativo

### 3. Detecção Proativa de Problemas
- **Bateria Baixa**: Alerta quando < 20%
- **Sinal Fraco**: Alerta quando RSSI < -80 dBm (intensidade de sinal wireless)
- **Classificação Automática**: Análise de qualidade de leitura (Normal, Suspeita, Inválida, Calibrada)

### 4. Rastreabilidade Completa
- **Dois Timestamps**: Leitura (sensor) e Recebimento (servidor)
- **Latência Calculada**: Diferença entre timestamps
- **Dados Adicionais**: JSON flexível para metadados

## ?? Como Executar

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=agrosolutions_ingestao;User Id=sa;Password=SuaSenha123;TrustServerCertificate=True"
  },
  "Database": {
    "Provider": "SqlServer"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "ExchangeName": "agrosolutions.ingestao"
  }
}
```

```bash
dotnet ef database update
dotnet run
```

**Documentação da API**: https://localhost:5004/scalar/v1

**Nota**: Acesso direto à raiz (https://localhost:5004/) redireciona automaticamente para o Scalar.

---

**IngestaoDados** - A porta de entrada do mundo IoT ????
