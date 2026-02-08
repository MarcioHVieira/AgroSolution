using Prometheus;

namespace AgroSolutions.IngestaoDados.Infrastructure.Metrics;

/// <summary>
/// Métricas Prometheus para monitoramento de sensores e talhões
/// </summary>
public static class SensorMetrics
{
    // Métricas de leituras de sensores
    public static readonly Counter LeiturasRecebidas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_sensor_leituras_total",
        "Total de leituras de sensores recebidas",
        new CounterConfiguration { LabelNames = new[] { "talhao_id", "talhao_nome" } });

    // Temperatura atual por talhão
    public static readonly Gauge Temperatura = Prometheus.Metrics.CreateGauge(
        "agrosolutions_sensor_temperatura",
        "Temperatura atual do sensor em Celsius",
        new GaugeConfiguration
        {
            LabelNames = new[] { "talhao_id", "talhao_nome", "cultura", "sensor_id" }
        });

    // Umidade do solo atual por talhão
    public static readonly Gauge Umidade = Prometheus.Metrics.CreateGauge(
        "agrosolutions_sensor_umidade",
        "Umidade do solo em porcentagem (0-100)",
        new GaugeConfiguration
        {
            LabelNames = new[] { "talhao_id", "talhao_nome", "cultura", "sensor_id" }
        });

    // Precipitação atual por talhão
    public static readonly Gauge Precipitacao = Prometheus.Metrics.CreateGauge(
        "agrosolutions_sensor_precipitacao",
        "Precipitação em milímetros",
        new GaugeConfiguration
        {
            LabelNames = new[] { "talhao_id", "talhao_nome", "cultura", "sensor_id" }
        });

    // Métricas existentes (mantidas para compatibilidade)
    public static readonly Counter SensoresCadastrados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_ingestao_sensores_cadastrados_total",
        "Total de sensores cadastrados");

    public static readonly Counter LeiturasProcessadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_ingestao_leituras_processadas_total",
        "Total de leituras processadas com sucesso");

    public static readonly Counter LeiturasComErro = Prometheus.Metrics.CreateCounter(
        "agrosolutions_ingestao_leituras_erro_total",
        "Total de leituras com erro no processamento");

    public static readonly Histogram TempoProcessamentoLeitura = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_ingestao_processamento_leitura_duracao_segundos",
        "Tempo de processamento de uma leitura de sensor",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10)
        });

    // Métodos auxiliares para atualização
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

