using FluentValidation;
using AgroSolutions.Analise.Application.DTOs;

namespace AgroSolutions.Analise.Application.Validators;

public class CriarAlertaDtoValidator : AbstractValidator<CriarAlertaDto>
{
    public CriarAlertaDtoValidator()
    {
        RuleFor(x => x.TalhaoId)
            .NotEmpty().WithMessage("TalhaoId é obrigatório");

        RuleFor(x => x.Titulo)
            .NotEmpty().WithMessage("Título é obrigatório")
            .MaximumLength(200).WithMessage("Título deve ter no máximo 200 caracteres");

        RuleFor(x => x.Mensagem)
            .NotEmpty().WithMessage("Mensagem é obrigatória")
            .MaximumLength(1000).WithMessage("Mensagem deve ter no máximo 1000 caracteres");

        RuleFor(x => x.Recomendacao)
            .MaximumLength(1000).WithMessage("Recomendação deve ter no máximo 1000 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Recomendacao));
    }
}

public class CriarRegraAlertaDtoValidator : AbstractValidator<CriarRegraAlertaDto>
{
    public CriarRegraAlertaDtoValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres");

        RuleFor(x => x.Descricao)
            .MaximumLength(500).WithMessage("Descrição deve ter no máximo 500 caracteres")
            .When(x => !string.IsNullOrEmpty(x.Descricao));

        RuleFor(x => x.Condicao)
            .NotEmpty().WithMessage("Condição é obrigatória");

        RuleFor(x => x.TemplateMensagem)
            .NotEmpty().WithMessage("Template de mensagem é obrigatório")
            .MaximumLength(1000).WithMessage("Template deve ter no máximo 1000 caracteres");
    }
}
