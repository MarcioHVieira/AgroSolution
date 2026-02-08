using System.Text.RegularExpressions;

namespace AgroSolutions.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Value Object para Email
/// </summary>
public sealed record Email
{
    public string Value { get; }

    private Email(string value)
    {
        Value = value;
    }

    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email não pode ser vazio", nameof(email));

        email = email.Trim().ToLowerInvariant();

        if (!IsValid(email))
            throw new ArgumentException("Email inválido", nameof(email));

        return new Email(email);
    }

    private static bool IsValid(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return false;

        try
        {
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            return regex.IsMatch(email);
        }
        catch
        {
            return false;
        }
    }

    public override string ToString() => Value;

    public static implicit operator string(Email email) => email.Value;
}
