using AgroSolutions.Analise.Domain.Enums;

namespace AgroSolutions.Analise.Domain.Entities;

/// <summary>
/// Entidade de Alerta gerado pelo motor de regras
/// </summary>
public class Alerta
{
    /// <summary>
    /// Identificador ínico do alerta
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador do talhão relacionado
    /// </summary>
    public Guid TalhaoId { get; set; }

    /// <summary>
    /// Tipo do alerta
    /// </summary>
    public TipoAlerta Tipo { get; set; }

    /// <summary>
    /// Nível de severidade
    /// </summary>
    public NivelSeveridade Severidade { get; set; }

    /// <summary>
    /// Status atual do alerta
    /// </summary>
    public StatusAlerta Status { get; set; }

    /// <summary>
    /// Título do alerta
    /// </summary>
    public string Titulo { get; set; } = string.Empty;

    /// <summary>
    /// Mensagem descritiva do alerta
    /// </summary>
    public string Mensagem { get; set; } = string.Empty;

    /// <summary>
    /// Recomendaçõo de ação
    /// </summary>
    public string? Recomendacao { get; set; }

    /// <summary>
    /// Data e hora de geração do alerta
    /// </summary>
    public DateTime DataGeracao { get; set; }

    /// <summary>
    /// Data e hora de visualização (opcional)
    /// </summary>
    public DateTime? DataVisualizacao { get; set; }

    /// <summary>
    /// Data e hora de resolução (opcional)
    /// </summary>
    public DateTime? DataResolucao { get; set; }

    /// <summary>
    /// Valor que disparou o alerta (ex: umidade 25%)
    /// </summary>
    public decimal? ValorReferencia { get; set; }

    /// <summary>
    /// Dados adicionais em JSON
    /// </summary>
    public string? DadosAdicionais { get; set; }

    /// <summary>
    /// Identificador do usuário que criou/modificou
    /// </summary>
    public Guid? UsuarioId { get; set; }
}
