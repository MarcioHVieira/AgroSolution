using AgroSolutions.Notificacoes.Application.DTOs;
using FluentValidation;

namespace AgroSolutions.Notificacoes.Application.Validators;

public class CriarNotificacaoDtoValidator : AbstractValidator<CriarNotificacaoDto>
{
    public CriarNotificacaoDtoValidator()
    {
        RuleFor(x => x.AlertaId)
            .NotEmpty().WithMessage("AlertaId é obrigatório");

        RuleFor(x => x.EmailDestinatario)
            .NotEmpty().WithMessage("E-mail do destinatário é obrigatório")
            .EmailAddress().WithMessage("E-mail inválido")
            .MaximumLength(255).WithMessage("E-mail deve ter no máximo 255 caracteres");

        RuleFor(x => x.NomeDestinatario)
            .NotEmpty().WithMessage("Nome do destinatário é obrigatório")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres");

        RuleFor(x => x.Assunto)
            .NotEmpty().WithMessage("Assunto é obrigatório")
            .MaximumLength(500).WithMessage("Assunto deve ter no máximo 500 caracteres");

        RuleFor(x => x.Mensagem)
            .NotEmpty().WithMessage("Mensagem é obrigatória");
    }
}
