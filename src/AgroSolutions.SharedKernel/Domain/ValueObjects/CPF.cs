namespace AgroSolutions.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Value Object para CPF
/// </summary>
public sealed record CPF
{
    public string Value { get; }

    private CPF(string value)
    {
        Value = value;
    }

    public static CPF Create(string cpf)
    {
        if (string.IsNullOrWhiteSpace(cpf))
            throw new ArgumentException("CPF não pode ser vazio", nameof(cpf));

        // Remove formatação
        cpf = cpf.Replace(".", "").Replace("-", "").Trim();

        if (!IsValid(cpf))
            throw new ArgumentException("CPF inválido", nameof(cpf));

        return new CPF(cpf);
    }

    private static bool IsValid(string cpf)
    {
        if (cpf.Length != 11)
            return false;

        // Verifica se todos os dígitos são iguais
        if (cpf.Distinct().Count() == 1)
            return false;

        // Calcula primeiro dígito verificador
        var soma = 0;
        for (int i = 0; i < 9; i++)
            soma += int.Parse(cpf[i].ToString()) * (10 - i);

        var resto = soma % 11;
        var digito1 = resto < 2 ? 0 : 11 - resto;

        if (int.Parse(cpf[9].ToString()) != digito1)
            return false;

        // Calcula segundo dígito verificador
        soma = 0;
        for (int i = 0; i < 10; i++)
            soma += int.Parse(cpf[i].ToString()) * (11 - i);

        resto = soma % 11;
        var digito2 = resto < 2 ? 0 : 11 - resto;

        return int.Parse(cpf[10].ToString()) == digito2;
    }

    /// <summary>
    /// Retorna CPF formatado (XXX.XXX.XXX-XX)
    /// </summary>
    public string Formatado => $"{Value.Substring(0, 3)}.{Value.Substring(3, 3)}.{Value.Substring(6, 3)}-{Value.Substring(9, 2)}";

    public override string ToString() => Value;

    public static implicit operator string(CPF cpf) => cpf.Value;
}
