using Prometheus;

namespace AgroSolutions.Analise.Infrastructure.Metrics;

public static class AnaliseMetrics
{
    // ========== MÉTRICAS PARA DASHBOARD DE TALHÕES ==========
    
    /// <summary>
    /// Status do talhão: 0=Crítico, 1=Alerta, 2=Normal
    /// </summary>
    public static readonly Gauge TalhaoStatus = Prometheus.Metrics.CreateGauge(
        "agrosolutions_talhao_status",
        "Status atual do talhão (0=Crítico, 1=Alerta, 2=Normal)",
        new GaugeConfiguration { LabelNames = new[] { "talhao_id", "talhao_nome", "cultura" } });

    /// <summary>
    /// Alertas atualmente ativos por tipo (0=Inativo, 1=Ativo)
    /// </summary>
    public static readonly Gauge AlertasAtivos = Prometheus.Metrics.CreateGauge(
        "agrosolutions_alertas_ativos",
        "Alertas atualmente ativos por tipo (0=Inativo, 1=Ativo)",
        new GaugeConfiguration { LabelNames = new[] { "tipo", "talhao_nome", "talhao_id" } });

    /// <summary>
    /// Total de alertas gerados por tipo
    /// </summary>
    public static readonly Counter AlertasGeradosPorTipo = Prometheus.Metrics.CreateCounter(
        "agrosolutions_alertas_gerados_total",
        "Total de alertas gerados por tipo",
        new CounterConfiguration { LabelNames = new[] { "tipo", "severidade" } });

    // ========== MÉTRICAS EXISTENTES ==========
    
    public static readonly Counter RegrasAvaliadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_analise_regras_avaliadas_total",
        "Total de regras de alerta avaliadas",
        new CounterConfiguration { LabelNames = new[] { "tipo_regra" } });

    public static readonly Counter AlertasGerados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_analise_alertas_gerados_total",
        "Total de alertas gerados",
        new CounterConfiguration { LabelNames = new[] { "severidade", "tipo_regra" } });

    public static readonly Counter AlertasPublicados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_analise_alertas_publicados_total",
        "Total de alertas publicados no RabbitMQ",
        new CounterConfiguration { LabelNames = new[] { "severidade" } });

    public static readonly Counter MensagensRabbitMQRecebidas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_analise_rabbitmq_mensagens_recebidas_total",
        "Total de mensagens recebidas do RabbitMQ");

    public static readonly Counter MensagensRabbitMQProcessadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_analise_rabbitmq_mensagens_processadas_total",
        "Total de mensagens processadas com sucesso");

    public static readonly Counter MensagensRabbitMQComErro = Prometheus.Metrics.CreateCounter(
        "agrosolutions_analise_rabbitmq_mensagens_erro_total",
        "Total de mensagens com erro");

    public static readonly Histogram TempoAvaliacaoRegra = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_analise_avaliacao_regra_duracao_segundos",
        "Tempo de avaliação de uma regra",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10),
            LabelNames = new[] { "tipo_regra" }
        });

    public static readonly Histogram TempoGeracaoAlerta = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_analise_geracao_alerta_duracao_segundos",
        "Tempo de geração de um alerta",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.001, 2, 10),
            LabelNames = new[] { "severidade" }
        });

    public static readonly Gauge RegrasAtivas = Prometheus.Metrics.CreateGauge(
        "agrosolutions_analise_regras_ativas",
        "Número de regras de alerta ativas",
        new GaugeConfiguration { LabelNames = new[] { "tipo_regra" } });

    public static readonly Gauge AlertasAbertos = Prometheus.Metrics.CreateGauge(
        "agrosolutions_analise_alertas_abertos",
        "Número de alertas em aberto",
        new GaugeConfiguration { LabelNames = new[] { "severidade" } });

    public static readonly Gauge TalhoesMonitorados = Prometheus.Metrics.CreateGauge(
        "agrosolutions_analise_talhoes_monitorados",
        "Número de talhões sendo monitorados");

    // ========== MÉTODOS AUXILIARES ==========

    /// <summary>
    /// Atualiza o status de um talhão
    /// </summary>
    public static void AtualizarStatusTalhao(string talhaoId, string talhaoNome, string cultura, int status)
    {
        TalhaoStatus.WithLabels(talhaoId, talhaoNome, cultura).Set(status);
    }

    /// <summary>
    /// Marca um alerta como ativo ou inativo
    /// </summary>
    public static void AtualizarAlertaAtivo(string tipo, string talhaoNome, string talhaoId, bool ativo)
    {
        AlertasAtivos.WithLabels(tipo, talhaoNome, talhaoId).Set(ativo ? 1 : 0);
    }

    /// <summary>
    /// Incrementa contador de alertas gerados
    /// </summary>
    public static void IncrementarAlertaGerado(string tipo, string severidade)
    {
        AlertasGeradosPorTipo.WithLabels(tipo, severidade).Inc();
    }

    /// <summary>
    /// Calcula e atualiza o status do talhão baseado nos alertas ativos
    /// </summary>
    public static int CalcularStatusTalhao(bool temAlertaCritico, bool temAlerta)
    {
        if (temAlertaCritico) return 0; // Crítico
        if (temAlerta) return 1; // Alerta
        return 2; // Normal
    }
}

