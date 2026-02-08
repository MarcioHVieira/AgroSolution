namespace AgroSolutions.SharedKernel.Infrastructure.Extensions;

/// <summary>
/// Extensões para strings
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Converte string para slug (URL-friendly)
    /// </summary>
    public static string ToSlug(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        return text
            .ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("á", "a")
            .Replace("ã", "a")
            .Replace("â", "a")
            .Replace("à", "a")
            .Replace("é", "e")
            .Replace("ê", "e")
            .Replace("í", "i")
            .Replace("ó", "o")
            .Replace("ô", "o")
            .Replace("ú", "u")
            .Replace("ü", "u")
            .Replace("ç", "c");
    }

    /// <summary>
    /// Verifica se string está vazia ou nula
    /// </summary>
    public static bool IsNullOrEmpty(this string? text)
    {
        return string.IsNullOrWhiteSpace(text);
    }

    /// <summary>
    /// Trunca string para tamanho máximo
    /// </summary>
    public static string Truncate(this string text, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= maxLength)
            return text;

        return text.Substring(0, maxLength) + "...";
    }

    /// <summary>
    /// Mascara parte da string (útil para emails, CPFs, etc.)
    /// </summary>
    public static string Mask(this string text, int visibleChars = 4)
    {
        if (string.IsNullOrWhiteSpace(text) || text.Length <= visibleChars)
            return text;

        var visible = text.Substring(0, visibleChars);
        var masked = new string('*', text.Length - visibleChars);
        return visible + masked;
    }
}
