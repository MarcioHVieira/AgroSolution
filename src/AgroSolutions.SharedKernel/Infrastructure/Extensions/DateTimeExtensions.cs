namespace AgroSolutions.SharedKernel.Infrastructure.Extensions;

/// <summary>
/// Extensões para DateTime
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Verifica se é fim de semana
    /// </summary>
    public static bool IsWeekend(this DateTime date)
    {
        return date.DayOfWeek == DayOfWeek.Saturday 
            || date.DayOfWeek == DayOfWeek.Sunday;
    }

    /// <summary>
    /// Verifica se é dia útil
    /// </summary>
    public static bool IsWeekday(this DateTime date)
    {
        return !date.IsWeekend();
    }

    /// <summary>
    /// Retorna início do dia (00:00:00)
    /// </summary>
    public static DateTime StartOfDay(this DateTime date)
    {
        return date.Date;
    }

    /// <summary>
    /// Retorna fim do dia (23:59:59.999)
    /// </summary>
    public static DateTime EndOfDay(this DateTime date)
    {
        return date.Date.AddDays(1).AddTicks(-1);
    }

    /// <summary>
    /// Calcula idade em anos
    /// </summary>
    public static int CalcularIdade(this DateTime dataNascimento)
    {
        var hoje = DateTime.Today;
        var idade = hoje.Year - dataNascimento.Year;
        
        if (dataNascimento.Date > hoje.AddYears(-idade))
            idade--;

        return idade;
    }
}
