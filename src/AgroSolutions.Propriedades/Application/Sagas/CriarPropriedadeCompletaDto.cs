namespace AgroSolutions.Propriedades.Application.Sagas;

/// <summary>
/// DTO com dados para criação completa de propriedade (Saga)
/// </summary>
public record CriarPropriedadeCompletaDto
{
    // Dados da propriedade
    public string Nome { get; init; } = string.Empty;
    public decimal AreaTotal { get; init; }
    public string? Descricao { get; init; }
    public Guid ProprietarioId { get; init; }

    // Dados dos talhões
    public List<CriarTalhaoDto> Talhoes { get; init; } = new();

    // IDs gerados durante a saga (para compensação)
    public Guid? PropriedadeId { get; set; }
    public List<Guid> TalhoesIds { get; set; } = new();
}

public record CriarTalhaoDto
{
    public string Nome { get; init; } = string.Empty;
    public decimal Area { get; init; }
    public string? Tipo { get; init; }
}
