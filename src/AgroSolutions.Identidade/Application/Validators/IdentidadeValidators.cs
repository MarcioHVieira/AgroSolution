using FluentValidation;
using AgroSolutions.Identidade.Application.DTOs;

namespace AgroSolutions.Identidade.Application.Validators;

public class RegistrarUsuarioDtoValidator : AbstractValidator<RegistrarUsuarioDto>
{
    public RegistrarUsuarioDtoValidator()
    {
        RuleFor(x => x.NomeCompleto)
            .NotEmpty().WithMessage("Nome completo é obrigatório")
            .MinimumLength(3).WithMessage("Nome completo deve ter no mínimo 3 caracteres")
            .MaximumLength(200).WithMessage("Nome completo deve ter no máximo 200 caracteres");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(100).WithMessage("E-mail deve ter no máximo 100 caracteres");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
            .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
            .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
            .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número")
            .Matches("[^a-zA-Z0-9]").WithMessage("Senha deve conter pelo menos um caractere especial");

        When(x => !string.IsNullOrEmpty(x.Telefone), () =>
        {
            RuleFor(x => x.Telefone)
                .Matches(@"^\d{10,11}$").WithMessage("Telefone deve conter 10 ou 11 dígitos");
        });

        When(x => !string.IsNullOrEmpty(x.Cpf), () =>
        {
            RuleFor(x => x.Cpf)
                .Matches(@"^\d{11}$").WithMessage("CPF deve conter 11 dígitos")
                .Must(ValidarCpf).WithMessage("CPF inválido");
        });
    }

    private bool ValidarCpf(string? cpf)
    {
        if (string.IsNullOrEmpty(cpf) || cpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cpf.Distinct().Count() == 1)
            return false;

        var multiplicador1 = new[] { 10, 9, 8, 7, 6, 5, 4, 3, 2 };
        var multiplicador2 = new[] { 11, 10, 9, 8, 7, 6, 5, 4, 3, 2 };

        var tempCpf = cpf[..9];
        var soma = 0;

        for (int i = 0; i < 9; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador1[i];

        var resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;

        var digito = resto.ToString();
        tempCpf += digito;
        soma = 0;

        for (int i = 0; i < 10; i++)
            soma += int.Parse(tempCpf[i].ToString()) * multiplicador2[i];

        resto = soma % 11;
        resto = resto < 2 ? 0 : 11 - resto;
        digito += resto.ToString();

        return cpf.EndsWith(digito);
    }
}

public class ValidarCodigoDtoValidator : AbstractValidator<ValidarCodigoDto>
{
    public ValidarCodigoDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.Codigo)
            .NotEmpty().WithMessage("Código é obrigatório")
            .Length(6).WithMessage("Código deve ter 6 caracteres");
    }
}

public class LoginDtoValidator : AbstractValidator<LoginDto>
{
    public LoginDtoValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("E-mail é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória");
    }
}
