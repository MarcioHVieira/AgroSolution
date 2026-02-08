using Prometheus;

namespace AgroSolutions.Propriedades.Infrastructure.Metrics;

public static class PropriedadesMetrics
{
    public static readonly Counter PropriedadesCriadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_propriedades_criadas_total",
        "Total de propriedades criadas");

    public static readonly Counter TalhoesCriados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_propriedades_talhoes_criados_total",
        "Total de talhões criados");

    public static readonly Counter CulturasCriadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_propriedades_culturas_criadas_total",
        "Total de culturas criadas");

    public static readonly Counter SensoresVinculados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_propriedades_sensores_vinculados_total",
        "Total de sensores vinculados a talhões",
        new CounterConfiguration { LabelNames = new[] { "tipo_sensor" } });

    public static readonly Counter ConsultasRealizadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_propriedades_consultas_realizadas_total",
        "Total de consultas realizadas",
        new CounterConfiguration { LabelNames = new[] { "endpoint" } });

    public static readonly Histogram TempoConsultaPropriedade = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_propriedades_consulta_propriedade_duracao_segundos",
        "Tempo de consulta de propriedade",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Histogram TempoConsultaTalhao = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_propriedades_consulta_talhao_duracao_segundos",
        "Tempo de consulta de talhão",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Histogram TempoCriacaoPropriedade = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_propriedades_criacao_propriedade_duracao_segundos",
        "Tempo de criação de propriedade",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) });

    public static readonly Gauge PropriedadesAtivas = Prometheus.Metrics.CreateGauge(
        "agrosolutions_propriedades_ativas",
        "Número de propriedades ativas");

    public static readonly Gauge TalhoesAtivos = Prometheus.Metrics.CreateGauge(
        "agrosolutions_propriedades_talhoes_ativos",
        "Número de talhões ativos");

    public static readonly Gauge AreaTotalMonitorada = Prometheus.Metrics.CreateGauge(
        "agrosolutions_propriedades_area_total_hectares",
        "Área total monitorada em hectares");

    public static readonly Gauge SensoresPorTalhao = Prometheus.Metrics.CreateGauge(
        "agrosolutions_propriedades_sensores_por_talhao",
        "Número médio de sensores por talhão");
}
