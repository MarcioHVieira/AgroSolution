using Prometheus;

namespace AgroSolutions.ProcessamentoDados.Infrastructure.Metrics;

/// <summary>
/// Métricas customizadas do Prometheus para ProcessamentoDados
/// </summary>
public static class ProcessamentoDadosMetrics
{
    // Contadores
    public static readonly Counter LeiturasProcessadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_processamento_leituras_processadas_total",
        "Total de leituras processadas com sucesso",
        new CounterConfiguration
        {
            LabelNames = new[] { "tipo_sensor" }
        });

    public static readonly Counter LeiturasComErro = Prometheus.Metrics.CreateCounter(
        "agrosolutions_processamento_leituras_erro_total",
        "Total de leituras que falharam no processamento",
        new CounterConfiguration
        {
            LabelNames = new[] { "tipo_erro" }
        });

    public static readonly Counter MensagensRabbitMQRecebidas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_processamento_rabbitmq_mensagens_recebidas_total",
        "Total de mensagens recebidas do RabbitMQ");

    public static readonly Counter MensagensRabbitMQProcessadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_processamento_rabbitmq_mensagens_processadas_total",
        "Total de mensagens processadas com sucesso do RabbitMQ");

    public static readonly Counter MensagensRabbitMQComErro = Prometheus.Metrics.CreateCounter(
        "agrosolutions_processamento_rabbitmq_mensagens_erro_total",
        "Total de mensagens com erro do RabbitMQ");

    // Histogramas
    public static readonly Histogram TempoProcessamentoLeitura = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_processamento_leitura_duracao_segundos",
        "Tempo de processamento de uma leitura",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10),
            LabelNames = new[] { "tipo_sensor" }
        });

    public static readonly Histogram TempoProcessamentoAgregacao = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_processamento_agregacao_duracao_segundos",
        "Tempo de processamento de agregação de dados",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
            LabelNames = new[] { "tipo_agregacao" }
        });

    // Gauges
    public static readonly Gauge FilaRabbitMQTamanho = Prometheus.Metrics.CreateGauge(
        "agrosolutions_processamento_rabbitmq_fila_tamanho",
        "Tamanho aproximado da fila RabbitMQ");

    public static readonly Gauge LeiturasEmProcessamento = Prometheus.Metrics.CreateGauge(
        "agrosolutions_processamento_leituras_em_processamento",
        "Número de leituras sendo processadas no momento");

    // Sumários
    public static readonly Summary ValoresProcessados = Prometheus.Metrics.CreateSummary(
        "agrosolutions_processamento_valores_processados",
        "Distribuição dos valores processados",
        new SummaryConfiguration
        {
            LabelNames = new[] { "tipo_sensor", "unidade" },
            Objectives = new[]
            {
                new QuantileEpsilonPair(0.5, 0.05),   // Mediana
                new QuantileEpsilonPair(0.9, 0.01),   // P90
                new QuantileEpsilonPair(0.95, 0.01),  // P95
                new QuantileEpsilonPair(0.99, 0.001)  // P99
            }
        });
}
