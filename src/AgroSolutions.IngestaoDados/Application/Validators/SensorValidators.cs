using AgroSolutions.IngestaoDados.Application.DTOs;
using FluentValidation;

namespace AgroSolutions.IngestaoDados.Application.Validators;

/// <summary>
/// Validador para criação de sensor
/// </summary>
public class CriarSensorDtoValidator : AbstractValidator<CriarSensorDto>
{
    public CriarSensorDtoValidator()
    {
        RuleFor(x => x.PropriedadeId)
            .NotEmpty().WithMessage("ID da propriedade é obrigatório");

        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID é obrigatório")
            .MinimumLength(5).WithMessage("Device ID deve ter no mínimo 5 caracteres")
            .MaximumLength(100).WithMessage("Device ID deve ter no máximo 100 caracteres")
            .Matches(@"^[A-Z0-9\-_]+$").WithMessage("Device ID deve conter apenas letras maiúsculas, números, hífens e underscores");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do sensor é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo de sensor inválido");

        RuleFor(x => x.IntervaloLeituraMinutos)
            .GreaterThan(0).WithMessage("Intervalo de leitura deve ser maior que zero")
            .LessThanOrEqualTo(1440).WithMessage("Intervalo de leitura não pode exceder 24 horas (1440 minutos)");

        When(x => !string.IsNullOrEmpty(x.Fabricante), () =>
        {
            RuleFor(x => x.Fabricante)
                .MaximumLength(100).WithMessage("Fabricante deve ter no máximo 100 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.Modelo), () =>
        {
            RuleFor(x => x.Modelo)
                .MaximumLength(100).WithMessage("Modelo deve ter no máximo 100 caracteres");
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

        When(x => x.Altitude.HasValue, () =>
        {
            RuleFor(x => x.Altitude)
                .GreaterThanOrEqualTo(-500).WithMessage("Altitude deve ser maior ou igual a -500 metros")
                .LessThanOrEqualTo(9000).WithMessage("Altitude não pode exceder 9.000 metros");
        });

        When(x => !string.IsNullOrEmpty(x.Observacoes), () =>
        {
            RuleFor(x => x.Observacoes)
                .MaximumLength(500).WithMessage("Observações devem ter no máximo 500 caracteres");
        });

        // Validação: se tem latitude, deve ter longitude
        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) || (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Se informar latitude, deve informar longitude também (e vice-versa)");
    }
}

/// <summary>
/// Validador para atualização de sensor
/// </summary>
public class AtualizarSensorDtoValidator : AbstractValidator<AtualizarSensorDto>
{
    public AtualizarSensorDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome do sensor é obrigatório")
            .MinimumLength(3).WithMessage("Nome deve ter no mínimo 3 caracteres")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.IntervaloLeituraMinutos)
            .GreaterThan(0).WithMessage("Intervalo de leitura deve ser maior que zero")
            .LessThanOrEqualTo(1440).WithMessage("Intervalo de leitura não pode exceder 24 horas (1440 minutos)");

        When(x => !string.IsNullOrEmpty(x.Fabricante), () =>
        {
            RuleFor(x => x.Fabricante)
                .MaximumLength(100).WithMessage("Fabricante deve ter no máximo 100 caracteres");
        });

        When(x => !string.IsNullOrEmpty(x.Modelo), () =>
        {
            RuleFor(x => x.Modelo)
                .MaximumLength(100).WithMessage("Modelo deve ter no máximo 100 caracteres");
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

        When(x => x.Altitude.HasValue, () =>
        {
            RuleFor(x => x.Altitude)
                .GreaterThanOrEqualTo(-500).WithMessage("Altitude deve ser maior ou igual a -500 metros")
                .LessThanOrEqualTo(9000).WithMessage("Altitude não pode exceder 9.000 metros");
        });

        When(x => !string.IsNullOrEmpty(x.Observacoes), () =>
        {
            RuleFor(x => x.Observacoes)
                .MaximumLength(500).WithMessage("Observações devem ter no máximo 500 caracteres");
        });

        RuleFor(x => x)
            .Must(x => (x.Latitude.HasValue && x.Longitude.HasValue) || (!x.Latitude.HasValue && !x.Longitude.HasValue))
            .WithMessage("Se informar latitude, deve informar longitude também (e vice-versa)");
    }
}

/// <summary>
/// Validador para registro de leitura de sensor
/// </summary>
public class RegistrarLeituraDtoValidator : AbstractValidator<RegistrarLeituraDto>
{
    // Limites razoáveis para diferentes tipos de sensores
    private static readonly Dictionary<string, (decimal Min, decimal Max)> LimitesValores = new()
    {
        { "temperatura", (-50, 60) },      // °C (agricultura)
        { "umidade", (0, 100) },           // %
        { "ph", (0, 14) },                 // pH
        { "pluviometro", (0, 500) },       // mm/h
        { "pressao", (800, 1100) },        // hPa
        { "vento", (0, 200) },             // km/h
        { "luz", (0, 200000) },            // lux
        { "co2", (0, 5000) },              // ppm
        { "condutividade", (0, 10000) }    // µS/cm
    };

    public RegistrarLeituraDtoValidator()
    {
        RuleFor(x => x.DeviceId)
            .NotEmpty().WithMessage("Device ID é obrigatório")
            .MinimumLength(5).WithMessage("Device ID deve ter no mínimo 5 caracteres")
            .MaximumLength(100).WithMessage("Device ID deve ter no máximo 100 caracteres");

        RuleFor(x => x.Valor)
            .NotNull().WithMessage("Valor da leitura é obrigatório");

        RuleFor(x => x.Unidade)
            .NotEmpty().WithMessage("Unidade de medida é obrigatória")
            .MaximumLength(20).WithMessage("Unidade deve ter no máximo 20 caracteres");

        RuleFor(x => x.TimestampLeitura)
            .NotEmpty().WithMessage("Timestamp da leitura é obrigatório")
            .GreaterThan(DateTime.UtcNow.AddDays(-7))
            .WithMessage("Timestamp da leitura não pode ser anterior a 7 dias")
            .LessThanOrEqualTo(DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Timestamp da leitura não pode ser mais de 5 minutos no futuro");

        When(x => x.NivelBateria.HasValue, () =>
        {
            RuleFor(x => x.NivelBateria)
                .InclusiveBetween(0, 100).WithMessage("Nível de bateria deve estar entre 0 e 100%");
        });

        When(x => x.IntensidadeSinal.HasValue, () =>
        {
            RuleFor(x => x.IntensidadeSinal)
                .InclusiveBetween(-120, 0).WithMessage("Intensidade do sinal (RSSI) deve estar entre -120 e 0 dBm");
        });

        When(x => !string.IsNullOrEmpty(x.DadosAdicionais), () =>
        {
            RuleFor(x => x.DadosAdicionais)
                .MaximumLength(5000).WithMessage("Dados adicionais devem ter no máximo 5.000 caracteres");
        });
    }
}

/// <summary>
/// Validador para registro de lote de leituras
/// </summary>
public class RegistrarLeituraLoteDtoValidator : AbstractValidator<RegistrarLeituraLoteDto>
{
    public RegistrarLeituraLoteDtoValidator()
    {
        RuleFor(x => x.Leituras)
            .NotNull().WithMessage("Lista de leituras é obrigatória")
            .NotEmpty().WithMessage("Deve conter pelo menos uma leitura")
            .Must(x => x.Count <= 1000).WithMessage("Não é permitido enviar mais de 1.000 leituras por vez");

        RuleForEach(x => x.Leituras)
            .SetValidator(new RegistrarLeituraDtoValidator());
    }
}


