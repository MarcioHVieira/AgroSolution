using Prometheus;

namespace AgroSolutions.Notificacoes.Infrastructure.Metrics;

public static class NotificacoesMetrics
{
    public static readonly Counter NotificacoesCriadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_criadas_total",
        "Total de notificações criadas",
        new CounterConfiguration { LabelNames = new[] { "tipo", "prioridade" } });

    public static readonly Counter NotificacoesEnviadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_enviadas_total",
        "Total de notificações enviadas com sucesso",
        new CounterConfiguration { LabelNames = new[] { "tipo", "prioridade" } });

    public static readonly Counter NotificacoesFalhadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_falhadas_total",
        "Total de notificações que falharam",
        new CounterConfiguration { LabelNames = new[] { "tipo", "motivo" } });

    public static readonly Counter EmailsEnviados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_emails_enviados_total",
        "Total de e-mails enviados com sucesso");

    public static readonly Counter EmailsFalhados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_emails_falhados_total",
        "Total de e-mails que falharam",
        new CounterConfiguration { LabelNames = new[] { "motivo" } });

    public static readonly Counter MensagensRabbitMQRecebidas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_rabbitmq_mensagens_recebidas_total",
        "Total de mensagens recebidas do RabbitMQ");

    public static readonly Counter MensagensRabbitMQProcessadas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_rabbitmq_mensagens_processadas_total",
        "Total de mensagens processadas com sucesso");

    public static readonly Counter MensagensRabbitMQComErro = Prometheus.Metrics.CreateCounter(
        "agrosolutions_notificacoes_rabbitmq_mensagens_erro_total",
        "Total de mensagens com erro");

    public static readonly Histogram TempoEnvioEmail = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_notificacoes_envio_email_duracao_segundos",
        "Tempo de envio de um e-mail",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.1, 2, 10) });

    public static readonly Histogram TempoProcessamentoNotificacao = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_notificacoes_processamento_duracao_segundos",
        "Tempo de processamento de uma notificação",
        new HistogramConfiguration
        {
            Buckets = Histogram.ExponentialBuckets(0.01, 2, 10),
            LabelNames = new[] { "tipo" }
        });

    public static readonly Gauge NotificacoesPendentes = Prometheus.Metrics.CreateGauge(
        "agrosolutions_notificacoes_pendentes",
        "Número de notificações pendentes de envio",
        new GaugeConfiguration { LabelNames = new[] { "prioridade" } });

    public static readonly Gauge NotificacoesEmProcessamento = Prometheus.Metrics.CreateGauge(
        "agrosolutions_notificacoes_em_processamento",
        "Número de notificações sendo processadas no momento");

    public static readonly Gauge TamanhoFilaDLQ = Prometheus.Metrics.CreateGauge(
        "agrosolutions_notificacoes_dlq_tamanho",
        "Tamanho da Dead Letter Queue");
}
