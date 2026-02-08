using AgroSolutions.Propriedades.Domain.Enums;

namespace AgroSolutions.Propriedades.Domain.Entities;

/// <summary>
/// Representa um talhão (subdivisão de uma propriedade rural)
/// </summary>
public class Talhao
{
    public Guid Id { get; private set; }
    public Guid PropriedadeId { get; private set; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public decimal Area { get; private set; } // em hectares
    
    // Coordenadas geográficas (polígono do talhão em formato WKT ou GeoJSON simplificado)
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    public string? Poligono { get; private set; } // Coordenadas do polígono em formato JSON
    
    public StatusTalhao Status { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    
    // Relacionamentos
    public Propriedade Propriedade { get; private set; } = null!;
    public ICollection<Cultura> Culturas { get; private set; } = new List<Cultura>();

    // EF Core
    private Talhao() 
    { 
        Nome = string.Empty;
    }

    public Talhao(
        Guid propriedadeId,
        string nome,
        decimal area,
        string? descricao = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? poligono = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do talhão é obrigatório", nameof(nome));

        if (area <= 0)
            throw new ArgumentException("Área deve ser maior que zero", nameof(area));

        Id = Guid.NewGuid();
        PropriedadeId = propriedadeId;
        Nome = nome;
        Descricao = descricao;
        Area = area;
        Latitude = latitude;
        Longitude = longitude;
        Poligono = poligono;
        Status = StatusTalhao.Disponivel;
        DataCadastro = DateTime.UtcNow;
    }

    public void Atualizar(
        string nome,
        decimal area,
        string? descricao = null,
        decimal? latitude = null,
        decimal? longitude = null,
        string? poligono = null)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome do talhão é obrigatório", nameof(nome));

        if (area <= 0)
            throw new ArgumentException("Área deve ser maior que zero", nameof(area));

        Nome = nome;
        Descricao = descricao;
        Area = area;
        Latitude = latitude;
        Longitude = longitude;
        Poligono = poligono;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarComoEmUso()
    {
        Status = StatusTalhao.EmUso;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarComoDisponivel()
    {
        Status = StatusTalhao.Disponivel;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void MarcarComoEmDescanso()
    {
        Status = StatusTalhao.EmDescanso;
        DataAtualizacao = DateTime.UtcNow;
    }

    public bool PossuiCulturaAtiva()
    {
        return Culturas.Any(c => c.Status == StatusCultura.Ativa);
    }
}
