namespace AgroSolutions.Identidade.Configuration.Settings;

/// <summary>
/// Configurações para geração e validação de tokens JWT
/// Utiliza assinatura RSA (RS256) com chave pública/privada
/// </summary>
public class JwtSettings
{
    public string Issuer { get; set; } = "AgroSolutions.Identidade";
    public string Audience { get; set; } = "AgroSolutions";
    public int ExpiracaoMinutos { get; set; } = 60;
}

