using Prometheus;

namespace AgroSolutions.Identidade.Infrastructure.Metrics;

public static class IdentidadeMetrics
{
    public static readonly Counter UsuariosCriados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_usuarios_criados_total",
        "Total de usuários criados");

    public static readonly Counter LoginsSucesso = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_logins_sucesso_total",
        "Total de logins bem-sucedidos",
        new CounterConfiguration { LabelNames = new[] { "tipo_usuario" } });

    public static readonly Counter LoginsFalhados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_logins_falhados_total",
        "Total de logins que falharam",
        new CounterConfiguration { LabelNames = new[] { "motivo" } });

    public static readonly Counter TokensGerados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_tokens_gerados_total",
        "Total de tokens JWT gerados");

    public static readonly Counter TokensRefresh = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_tokens_refresh_total",
        "Total de tokens refresh gerados");

    public static readonly Counter TokensInvalidos = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_tokens_invalidos_total",
        "Total de tokens inválidos rejeitados",
        new CounterConfiguration { LabelNames = new[] { "motivo" } });

    public static readonly Counter SenhasRedefinidas = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_senhas_redefinidas_total",
        "Total de senhas redefinidas");

    public static readonly Counter EmailsVerificados = Prometheus.Metrics.CreateCounter(
        "agrosolutions_identidade_emails_verificados_total",
        "Total de e-mails verificados");

    public static readonly Histogram TempoLogin = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_identidade_login_duracao_segundos",
        "Tempo de processamento de um login",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.01, 2, 10) });

    public static readonly Histogram TempoGeracaoToken = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_identidade_geracao_token_duracao_segundos",
        "Tempo de geração de um token JWT",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Histogram TempoValidacaoToken = Prometheus.Metrics.CreateHistogram(
        "agrosolutions_identidade_validacao_token_duracao_segundos",
        "Tempo de validação de um token",
        new HistogramConfiguration { Buckets = Histogram.ExponentialBuckets(0.001, 2, 10) });

    public static readonly Gauge UsuariosAtivos = Prometheus.Metrics.CreateGauge(
        "agrosolutions_identidade_usuarios_ativos",
        "Número de usuários ativos no sistema",
        new GaugeConfiguration { LabelNames = new[] { "tipo_usuario" } });

    public static readonly Gauge SessoesAtivas = Prometheus.Metrics.CreateGauge(
        "agrosolutions_identidade_sessoes_ativas",
        "Número de sessões ativas no momento");

    public static readonly Gauge TokensEmCache = Prometheus.Metrics.CreateGauge(
        "agrosolutions_identidade_tokens_em_cache",
        "Número de tokens em cache");
}
