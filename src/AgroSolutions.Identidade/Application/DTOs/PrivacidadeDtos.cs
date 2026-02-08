namespace AgroSolutions.Identidade.Application.DTOs;

/// <summary>
/// DTO para solicitar exclusão de conta (LGPD Art. 18, VI)
/// </summary>
public record SolicitarExclusaoDto(
    string Motivo
);

/// <summary>
/// DTO para exportação de dados (LGPD Art. 18, IV)
/// </summary>
public record DadosExportadosDto
{
    public DateTime DataExportacao { get; init; }
    public object? DadosPessoais { get; init; }
    public object? Propriedades { get; init; }
    public object? Leituras { get; init; }
    public object? Alertas { get; init; }
    public List<AuditoriaDto>? HistoricoAcessos { get; init; }
    public object? Consentimentos { get; init; }
}

/// <summary>
/// DTO de auditoria para exportação
/// </summary>
public record AuditoriaDto(
    DateTime DataHora,
    string Acao,
    string Entidade,
    string EnderecoIP,
    bool Sucesso
);

/// <summary>
/// DTO de consentimento
/// </summary>
public record ConsentimentoDto(
    string Tipo,
    bool Aceito,
    DateTime? DataAceite,
    string? Versao
);

/// <summary>
/// DTO de resposta de consentimentos
/// </summary>
public record HistoricoConsentimentosDto(
    List<ConsentimentoDto> Consentimentos
);
