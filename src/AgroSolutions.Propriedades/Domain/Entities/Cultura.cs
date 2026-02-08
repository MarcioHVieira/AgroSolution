using AgroSolutions.Propriedades.Domain.Enums;

namespace AgroSolutions.Propriedades.Domain.Entities;

/// <summary>
/// Representa uma cultura plantada em um talhão
/// </summary>
public class Cultura
{
    public Guid Id { get; private set; }
    public Guid TalhaoId { get; private set; }
    public TipoCultura Tipo { get; private set; }
    public string Variedade { get; private set; }
    public decimal AreaPlantada { get; private set; } // em hectares
    public DateTime DataPlantio { get; private set; }
    public DateTime? DataColheitaPrevista { get; private set; }
    public DateTime? DataColheitaRealizada { get; private set; }
    public decimal? ProducaoEstimada { get; private set; } // em toneladas
    public decimal? ProducaoReal { get; private set; } // em toneladas
    public string? Observacoes { get; private set; }
    public StatusCultura Status { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    
    // Relacionamentos
    public Talhao Talhao { get; private set; } = null!;

    // EF Core
    private Cultura() 
    { 
        Variedade = string.Empty;
    }

    public Cultura(
        Guid talhaoId,
        TipoCultura tipo,
        string variedade,
        decimal areaPlantada,
        DateTime dataPlantio,
        DateTime? dataColheitaPrevista = null,
        decimal? producaoEstimada = null,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(variedade))
            throw new ArgumentException("Variedade é obrigatória", nameof(variedade));

        if (areaPlantada <= 0)
            throw new ArgumentException("Área plantada deve ser maior que zero", nameof(areaPlantada));

        Id = Guid.NewGuid();
        TalhaoId = talhaoId;
        Tipo = tipo;
        Variedade = variedade;
        AreaPlantada = areaPlantada;
        DataPlantio = dataPlantio;
        DataColheitaPrevista = dataColheitaPrevista;
        ProducaoEstimada = producaoEstimada;
        Observacoes = observacoes;
        Status = StatusCultura.Ativa;
        DataCadastro = DateTime.UtcNow;
    }

    public void Atualizar(
        TipoCultura tipo,
        string variedade,
        decimal areaPlantada,
        DateTime dataPlantio,
        DateTime? dataColheitaPrevista = null,
        decimal? producaoEstimada = null,
        string? observacoes = null)
    {
        if (string.IsNullOrWhiteSpace(variedade))
            throw new ArgumentException("Variedade é obrigatória", nameof(variedade));

        if (areaPlantada <= 0)
            throw new ArgumentException("Área plantada deve ser maior que zero", nameof(areaPlantada));

        Tipo = tipo;
        Variedade = variedade;
        AreaPlantada = areaPlantada;
        DataPlantio = dataPlantio;
        DataColheitaPrevista = dataColheitaPrevista;
        ProducaoEstimada = producaoEstimada;
        Observacoes = observacoes;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void RegistrarColheita(DateTime dataColheita, decimal producaoReal, string? observacoes = null)
    {
        if (producaoReal < 0)
            throw new ArgumentException("Produção real não pode ser negativa", nameof(producaoReal));

        DataColheitaRealizada = dataColheita;
        ProducaoReal = producaoReal;
        if (!string.IsNullOrWhiteSpace(observacoes))
        {
            Observacoes = observacoes;
        }
        Status = StatusCultura.Colhida;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Cancelar(string motivo)
    {
        Status = StatusCultura.Cancelada;
        Observacoes = $"Cancelada: {motivo}";
        DataAtualizacao = DateTime.UtcNow;
    }

    public decimal? CalcularProdutividade()
    {
        if (ProducaoReal.HasValue && AreaPlantada > 0)
        {
            return ProducaoReal.Value / AreaPlantada; // toneladas por hectare
        }
        return null;
    }
}
