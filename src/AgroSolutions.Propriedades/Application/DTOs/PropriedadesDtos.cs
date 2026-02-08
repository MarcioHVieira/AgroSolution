using AgroSolutions.Propriedades.Domain.Enums;

namespace AgroSolutions.Propriedades.Application.DTOs;

public record CriarPropriedadeDto(
    string Nome,
    decimal AreaTotal,
    TipoPropriedade Tipo,
    string Cep,
    string Endereco,
    string Bairro,
    string Cidade,
    string Estado,
    string? Descricao = null,
    string? Numero = null,
    string? Complemento = null,
    decimal? Latitude = null,
    decimal? Longitude = null
);

public record AtualizarPropriedadeDto(
    string Nome,
    decimal AreaTotal,
    TipoPropriedade Tipo,
    string? Descricao = null,
    decimal? Latitude = null,
    decimal? Longitude = null
);

public record AtualizarEnderecoPropriedadeDto(
    string Cep,
    string Endereco,
    string Bairro,
    string Cidade,
    string Estado,
    string? Numero = null,
    string? Complemento = null
);

public record PropriedadeDto(
    Guid Id,
    Guid ProprietarioId,
    string Nome,
    string? Descricao,
    decimal AreaTotal,
    decimal AreaDisponivel,
    TipoPropriedade Tipo,
    string Cep,
    string Endereco,
    string? Numero,
    string? Complemento,
    string Bairro,
    string Cidade,
    string Estado,
    decimal? Latitude,
    decimal? Longitude,
    StatusPropriedade Status,
    int QuantidadeTalhoes,
    DateTime DataCadastro,
    DateTime? DataAtualizacao
);

// ===== TALHAO =====

public record CriarTalhaoDto(
    Guid PropriedadeId,
    string Nome,
    decimal Area,
    string? Descricao = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? Poligono = null
);

public record AtualizarTalhaoDto(
    string Nome,
    decimal Area,
    string? Descricao = null,
    decimal? Latitude = null,
    decimal? Longitude = null,
    string? Poligono = null
);

public record TalhaoDto(
    Guid Id,
    Guid PropriedadeId,
    string Nome,
    string? Descricao,
    decimal Area,
    decimal? Latitude,
    decimal? Longitude,
    string? Poligono,
    StatusTalhao Status,
    bool PossuiCulturaAtiva,
    int QuantidadeCulturas,
    DateTime DataCadastro,
    DateTime? DataAtualizacao
);

// ===== CULTURA =====

public record CriarCulturaDto(
    Guid TalhaoId,
    TipoCultura Tipo,
    string Variedade,
    decimal AreaPlantada,
    DateTime DataPlantio,
    DateTime? DataColheitaPrevista = null,
    decimal? ProducaoEstimada = null,
    string? Observacoes = null
);

public record AtualizarCulturaDto(
    TipoCultura Tipo,
    string Variedade,
    decimal AreaPlantada,
    DateTime DataPlantio,
    DateTime? DataColheitaPrevista = null,
    decimal? ProducaoEstimada = null,
    string? Observacoes = null
);

public record RegistrarColheitaDto(
    DateTime DataColheita,
    decimal ProducaoReal,
    string? Observacoes = null
);

public record CulturaDto(
    Guid Id,
    Guid TalhaoId,
    TipoCultura Tipo,
    string Variedade,
    decimal AreaPlantada,
    DateTime DataPlantio,
    DateTime? DataColheitaPrevista,
    DateTime? DataColheitaRealizada,
    decimal? ProducaoEstimada,
    decimal? ProducaoReal,
    decimal? Produtividade,
    string? Observacoes,
    StatusCultura Status,
    DateTime DataCadastro,
    DateTime? DataAtualizacao
);
