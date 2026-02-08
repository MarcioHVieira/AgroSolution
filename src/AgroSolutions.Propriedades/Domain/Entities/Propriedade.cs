using AgroSolutions.Propriedades.Domain.Enums;

namespace AgroSolutions.Propriedades.Domain.Entities;

/// <summary>
/// Representa uma propriedade rural (fazenda, sítio, etc.)
/// </summary>
public class Propriedade
{
    public Guid Id { get; private set; }
    public Guid ProprietarioId { get; private set; }
    public string Nome { get; private set; }
    public string? Descricao { get; private set; }
    public decimal AreaTotal { get; private set; } // em hectares
    public TipoPropriedade Tipo { get; private set; }
    
    // Endereço
    public string Cep { get; private set; }
    public string Endereco { get; private set; }
    public string? Numero { get; private set; }
    public string? Complemento { get; private set; }
    public string Bairro { get; private set; }
    public string Cidade { get; private set; }
    public string Estado { get; private set; }
    
    // Coordenadas geográficas (centro da propriedade)
    public decimal? Latitude { get; private set; }
    public decimal? Longitude { get; private set; }
    
    // Status
    public StatusPropriedade Status { get; private set; }
    public DateTime DataCadastro { get; private set; }
    public DateTime? DataAtualizacao { get; private set; }
    
    // Relacionamentos
    public ICollection<Talhao> Talhoes { get; private set; } = new List<Talhao>();

    // EF Core
    private Propriedade() 
    { 
        Nome = string.Empty;
        Cep = string.Empty;
        Endereco = string.Empty;
        Bairro = string.Empty;
        Cidade = string.Empty;
        Estado = string.Empty;
    }

    public Propriedade(
        Guid proprietarioId,
        string nome,
        decimal areaTotal,
        TipoPropriedade tipo,
        string cep,
        string endereco,
        string bairro,
        string cidade,
        string estado,
        string? descricao = null,
        string? numero = null,
        string? complemento = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        ValidarInvariantes(nome, areaTotal, cep, endereco, cidade, estado);

        Id = Guid.NewGuid();
        ProprietarioId = proprietarioId;
        Nome = nome;
        Descricao = descricao;
        AreaTotal = areaTotal;
        Tipo = tipo;
        Cep = cep;
        Endereco = endereco;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        Latitude = latitude;
        Longitude = longitude;
        Status = StatusPropriedade.Ativa;
        DataCadastro = DateTime.UtcNow;
    }

    public void Atualizar(
        string nome,
        decimal areaTotal,
        TipoPropriedade tipo,
        string? descricao = null,
        decimal? latitude = null,
        decimal? longitude = null)
    {
        ValidarInvariantesAtualizacao(nome, areaTotal);

        Nome = nome;
        Descricao = descricao;
        AreaTotal = areaTotal;
        Tipo = tipo;
        Latitude = latitude;
        Longitude = longitude;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void AtualizarEndereco(
        string cep,
        string endereco,
        string bairro,
        string cidade,
        string estado,
        string? numero = null,
        string? complemento = null)
    {
        ValidarInvariantesEndereco(cep, endereco, cidade, estado);

        Cep = cep;
        Endereco = endereco;
        Numero = numero;
        Complemento = complemento;
        Bairro = bairro;
        Cidade = cidade;
        Estado = estado;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Ativar()
    {
        Status = StatusPropriedade.Ativa;
        DataAtualizacao = DateTime.UtcNow;
    }

    public void Inativar()
    {
        Status = StatusPropriedade.Inativa;
        DataAtualizacao = DateTime.UtcNow;
    }

    public decimal CalcularAreaDisponivel()
    {
        var areaTalhoesUtilizada = Talhoes.Sum(t => t.Area);
        return AreaTotal - areaTalhoesUtilizada;
    }

    public bool PossuiAreaDisponivel(decimal area)
    {
        return CalcularAreaDisponivel() >= area;
    }

    private static void ValidarInvariantes(
        string nome,
        decimal areaTotal,
        string cep,
        string endereco,
        string cidade,
        string estado)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da propriedade não pode ser vazio", nameof(nome));

        if (areaTotal <= 0)
            throw new ArgumentException("Área total deve ser maior que zero", nameof(areaTotal));

        ValidarInvariantesEndereco(cep, endereco, cidade, estado);
    }

    private static void ValidarInvariantesAtualizacao(string nome, decimal areaTotal)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("Nome da propriedade não pode ser vazio", nameof(nome));

        if (areaTotal <= 0)
            throw new ArgumentException("Área total deve ser maior que zero", nameof(areaTotal));
    }

    private static void ValidarInvariantesEndereco(
        string cep,
        string endereco,
        string cidade,
        string estado)
    {
        if (string.IsNullOrWhiteSpace(cep))
            throw new ArgumentException("CEP não pode ser vazio", nameof(cep));

        if (string.IsNullOrWhiteSpace(endereco))
            throw new ArgumentException("Endereço não pode ser vazio", nameof(endereco));

        if (string.IsNullOrWhiteSpace(cidade))
            throw new ArgumentException("Cidade não pode ser vazia", nameof(cidade));

        if (string.IsNullOrWhiteSpace(estado))
            throw new ArgumentException("Estado não pode ser vazio", nameof(estado));
    }
}
