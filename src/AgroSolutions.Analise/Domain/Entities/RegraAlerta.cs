using AgroSolutions.Analise.Domain.Enums;

namespace AgroSolutions.Analise.Domain.Entities;

/// <summary>
/// Configuração de regra de alerta
/// </summary>
public class RegraAlerta
{
    /// <summary>
    /// Identificador único da regra
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nome da regra
    /// </summary>
    public string Nome { get; set; } = string.Empty;

    /// <summary>
    /// Descrição da regra
    /// </summary>
    public string? Descricao { get; set; }

    /// <summary>
    /// Tipo de alerta que a regra gera
    /// </summary>
    public TipoAlerta TipoAlerta { get; set; }

    /// <summary>
    /// Severidade padrão do alerta
    /// </summary>
    public NivelSeveridade Severidade { get; set; }

    /// <summary>
    /// Indica se a regra está ativa
    /// </summary>
    public bool Ativa { get; set; }

    /// <summary>
    /// Condição da regra em formato JSON
    /// Exemplo: {"campo":"UmidadeSolo","operador":"menor","valor":30,"duracao":24}
    /// </summary>
    public string Condicao { get; set; } = string.Empty;

    /// <summary>
    /// Template da mensagem do alerta
    /// </summary>
    public string TemplateMensagem { get; set; } = string.Empty;

    /// <summary>
    /// Recomendação padrão
    /// </summary>
    public string? Recomendacao { get; set; }

    /// <summary>
    /// Data de criação da regra
    /// </summary>
    public DateTime DataCriacao { get; set; }

    /// <summary>
    /// Data da última atualização
    /// </summary>
    public DateTime? DataAtualizacao { get; set; }
}
