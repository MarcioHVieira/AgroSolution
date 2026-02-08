using FluentValidation;
using AgroSolutions.Propriedades.Application.DTOs;

namespace AgroSolutions.Propriedades.Application.Validators;

/// <summary>
/// Validador para criação de propriedade
/// </summary>
public class CriarPropriedadeDtoValidator : AbstractValidator<CriarPropriedadeDto>
{
    public CriarPropriedadeDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da propriedade é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.AreaTotal)
            .GreaterThan(0).WithMessage("Área total deve ser maior que zero")
            .LessThanOrEqualTo(1000000).WithMessage("Área total deve ser menor ou igual a 1.000.000 hectares");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de propriedade inválido");

        // Validação de CEP
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .Matches(@"^\d{8}$").WithMessage("CEP deve conter 8 dígitos (apenas números)");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("Endereço é obrigatório")
            .MinimumLength(5).WithMessage("Endereço deve ter no mínimo 5 caracteres")
            .MaximumLength(200).WithMessage("Endereço deve ter no máximo 200 caracteres");

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Bairro é obrigatório")
            .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres");

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MinimumLength(3).WithMessage("Cidade deve ter no mínimo 3 caracteres")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .Length(2).WithMessage("Estado deve ter 2 caracteres (sigla UF)")
            .Matches(@"^[A-Z]{2}$").WithMessage("Estado deve ser uma sigla válida (ex: SP, MG, RJ)");

        // Validações opcionais
        When(x => !string.IsNullOrEmpty(x.Descricao), () =>
        {
            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.Numero), () =>
        {
            RuleFor(x => x.Numero)
                .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.Complemento), () =>
        {
            RuleFor(x => x.Complemento)
                .MaximumLength(100).WithMessage("Complemento deve ter no máximo 100 caracteres");
        });

        // Validações de coordenadas geográficas
        When(x => x.Latitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude deve estar entre -90 e 90 graus");
        });

        When(x => x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude deve estar entre -180 e 180 graus");
        });

        // Validação lógica: se tem latitude, deve ter longitude e vice-versa
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) || (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Se informar latitude, deve informar longitude também (e vice-versa)");
    }
}

/// <summary>
/// Validador para atualização de propriedade
/// </summary>
public class AtualizarPropriedadeDtoValidator : AbstractValidator<AtualizarPropriedadeDto>
{
    public AtualizarPropriedadeDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da propriedade é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.AreaTotal)
            .GreaterThan(0).WithMessage("Área total deve ser maior que zero")
            .LessThanOrEqualTo(1000000).WithMessage("Área total deve ser menor ou igual a 1.000.000 hectares");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de propriedade inválido");

        When(x => !string.IsNullOrEmpty(x.Descricao), () =>
        {
            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
        });

        When(x => x.Latitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude deve estar entre -90 e 90 graus");
        });

        When(x => x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude deve estar entre -180 e 180 graus");
        });

        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) || (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Se informar latitude, deve informar longitude também (e vice-versa)");
    }
}

/// <summary>
/// Validador para atualização de endereço da propriedade
/// </summary>
public class AtualizarEnderecoPropriedadeDtoValidator : AbstractValidator<AtualizarEnderecoPropriedadeDto>
{
    public AtualizarEnderecoPropriedadeDtoValidator()
    {
        RuleFor(x => x.Cep)
            .NotEmpty().WithMessage("CEP é obrigatório")
            .Matches(@"^\d{8}$").WithMessage("CEP deve conter 8 dígitos (apenas números)");

        RuleFor(x => x.Endereco)
            .NotEmpty().WithMessage("Endereço é obrigatório")
            .MinimumLength(5).WithMessage("Endereço deve ter no mínimo 5 caracteres")
            .MaximumLength(200).WithMessage("Endereço deve ter no máximo 200 caracteres");

        RuleFor(x => x.Bairro)
            .NotEmpty().WithMessage("Bairro é obrigatório")
            .MaximumLength(100).WithMessage("Bairro deve ter no máximo 100 caracteres");

        RuleFor(x => x.Cidade)
            .NotEmpty().WithMessage("Cidade é obrigatória")
            .MinimumLength(3).WithMessage("Cidade deve ter no mínimo 3 caracteres")
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres");

        RuleFor(x => x.Estado)
            .NotEmpty().WithMessage("Estado é obrigatório")
            .Length(2).WithMessage("Estado deve ter 2 caracteres (sigla UF)")
            .Matches(@"^[A-Z]{2}$").WithMessage("Estado deve ser uma sigla válida (ex: SP, MG, RJ)");

        When(x => !string.IsNullOrEmpty(x.Numero), () =>
        {
            RuleFor(x => x.Numero)
                .MaximumLength(20).WithMessage("Número deve ter no máximo 20 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.Complemento), () =>
        {
            RuleFor(x => x.Complemento)
                .MaximumLength(100).WithMessage("Complemento deve ter no máximo 100 caracteres");
        });
    }
}

/// <summary>
/// Validador para criação de talhão
/// </summary>
public class CriarTalhaoDtoValidator : AbstractValidator<CriarTalhaoDto>
{
    public CriarTalhaoDtoValidator()
    {
        RuleFor(x => x.PropriedadeId)
            .NotEmpty().WithMessage("ID da propriedade é obrigatório");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do talhão é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Area)
            .GreaterThan(0).WithMessage("Área do talhão deve ser maior que zero")
            .LessThanOrEqualTo(50000).WithMessage("Área do talhão deve ser menor ou igual a 50.000 hectares");

        When(x => !string.IsNullOrEmpty(x.Descricao), () =>
        {
            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
        });

        When(x => x.Latitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude deve estar entre -90 e 90 graus");
        });

        When(x => x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude deve estar entre -180 e 180 graus");
        });

        When(x => !string.IsNullOrEmpty(x.Poligono), () =>
        {
            RuleFor(x => x.Poligono)
                .MaximumLength(10000).WithMessage("Polígono deve ter no máximo 10.000 caracteres");
        });
    }
}

/// <summary>
/// Validador para atualização de talhão
/// </summary>
public class AtualizarTalhaoDtoValidator : AbstractValidator<AtualizarTalhaoDto>
{
    public AtualizarTalhaoDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do talhão é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Area)
            .GreaterThan(0).WithMessage("Área do talhão deve ser maior que zero")
            .LessThanOrEqualTo(50000).WithMessage("Área do talhão deve ser menor ou igual a 50.000 hectares");

        When(x => !string.IsNullOrEmpty(x.Descricao), () =>
        {
            RuleFor(x => x.Descricao)
                .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres");
        });

        When(x => x.Latitude.HasValue, () =>
        {
            RuleFor(x => x.Latitude)
                .InclusiveBetween(-90, 90).WithMessage("Latitude deve estar entre -90 e 90 graus");
        });

        When(x => x.Longitude.HasValue, () =>
        {
            RuleFor(x => x.Longitude)
                .InclusiveBetween(-180, 180).WithMessage("Longitude deve estar entre -180 e 180 graus");
        });

        When(x => !string.IsNullOrEmpty(x.Poligono), () =>
        {
            RuleFor(x => x.Poligono)
                .MaximumLength(10000).WithMessage("Polígono deve ter no máximo 10.000 caracteres");
        });
    }
}

/// <summary>
/// Validador para criação de cultura
/// </summary>
public class CriarCulturaDtoValidator : AbstractValidator<CriarCulturaDto>
{
    public CriarCulturaDtoValidator()
    {
        RuleFor(x => x.TalhaoId)
            .NotEmpty().WithMessage("ID do talhão é obrigatório");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de cultura inválido");

        RuleFor(x => x.Variedade)
            .NotEmpty().WithMessage("Variedade da cultura é obrigatória")
            .MinimumLength(2).WithMessage("Variedade deve ter no mínimo 2 caracteres")
            .MaximumLength(100).WithMessage("Variedade deve ter no máximo 100 caracteres");

        RuleFor(x => x.AreaPlantada)
            .GreaterThan(0).WithMessage("Área plantada deve ser maior que zero")
            .LessThanOrEqualTo(50000).WithMessage("Área plantada deve ser menor ou igual a 50.000 hectares");

        RuleFor(x => x.DataPlantio)
            .NotEmpty().WithMessage("Data de plantio é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de plantio não pode ser futura");

        When(x => x.DataColheitaPrevista.HasValue, () =>
        {
            RuleFor(x => x.DataColheitaPrevista)
                .GreaterThan(x => x.DataPlantio).WithMessage("Data de colheita prevista deve ser posterior à data de plantio");
        });

        When(x => x.ProducaoEstimada.HasValue, () =>
        {
            RuleFor(x => x.ProducaoEstimada)
                .GreaterThan(0).WithMessage("Produção estimada deve ser maior que zero")
                .LessThanOrEqualTo(1000000).WithMessage("Produção estimada deve ser menor ou igual a 1.000.000 toneladas");
        });

        When(x => !string.IsNullOrEmpty(x.Observacoes), () =>
        {
            RuleFor(x => x.Observacoes)
                .MaximumLength(1000).WithMessage("Observações devem ter no máximo 1.000 caracteres");
        });
    }
}

/// <summary>
/// Validador para atualização de cultura
/// </summary>
public class AtualizarCulturaDtoValidator : AbstractValidator<AtualizarCulturaDto>
{
    public AtualizarCulturaDtoValidator()
    {
        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de cultura inválido");

        RuleFor(x => x.Variedade)
            .NotEmpty().WithMessage("Variedade da cultura é obrigatória")
            .MinimumLength(2).WithMessage("Variedade deve ter no mínimo 2 caracteres")
            .MaximumLength(100).WithMessage("Variedade deve ter no máximo 100 caracteres");

        RuleFor(x => x.AreaPlantada)
            .GreaterThan(0).WithMessage("Área plantada deve ser maior que zero")
            .LessThanOrEqualTo(50000).WithMessage("Área plantada deve ser menor ou igual a 50.000 hectares");

        RuleFor(x => x.DataPlantio)
            .NotEmpty().WithMessage("Data de plantio é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de plantio não pode ser futura");

        When(x => x.DataColheitaPrevista.HasValue, () =>
        {
            RuleFor(x => x.DataColheitaPrevista)
                .GreaterThan(x => x.DataPlantio).WithMessage("Data de colheita prevista deve ser posterior à data de plantio");
        });

        When(x => x.ProducaoEstimada.HasValue, () =>
        {
            RuleFor(x => x.ProducaoEstimada)
                .GreaterThan(0).WithMessage("Produção estimada deve ser maior que zero")
                .LessThanOrEqualTo(1000000).WithMessage("Produção estimada deve ser menor ou igual a 1.000.000 toneladas");
        });

        When(x => !string.IsNullOrEmpty(x.Observacoes), () =>
        {
            RuleFor(x => x.Observacoes)
                .MaximumLength(1000).WithMessage("Observações devem ter no máximo 1.000 caracteres");
        });
    }
}

/// <summary>
/// Validador para registro de colheita
/// </summary>
public class RegistrarColheitaDtoValidator : AbstractValidator<RegistrarColheitaDto>
{
    public RegistrarColheitaDtoValidator()
    {
        RuleFor(x => x.DataColheita)
            .NotEmpty().WithMessage("Data de colheita é obrigatória")
            .LessThanOrEqualTo(DateTime.UtcNow).WithMessage("Data de colheita não pode ser futura");

        RuleFor(x => x.ProducaoReal)
            .GreaterThan(0).WithMessage("Produção real deve ser maior que zero")
            .LessThanOrEqualTo(1000000).WithMessage("Produção real deve ser menor ou igual a 1.000.000 toneladas");

        When(x => !string.IsNullOrEmpty(x.Observacoes), () =>
        {
            RuleFor(x => x.Observacoes)
                .MaximumLength(1000).WithMessage("Observações devem ter no máximo 1.000 caracteres");
        });
    }
}
