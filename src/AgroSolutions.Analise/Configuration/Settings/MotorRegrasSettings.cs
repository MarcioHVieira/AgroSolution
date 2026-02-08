namespace AgroSolutions.Analise.Configuration.Settings;

/// <summary>
/// Configurações do Motor de Regras
/// </summary>
public class MotorRegrasSettings
{
    public int StartupDelaySeconds { get; set; } = 10;
    public int IntervaloAvaliacaoMinutos { get; set; } = 5;
    public MaxLeiturasQuerySettings MaxLeiturasQuery { get; set; } = new();
    public RegraSecaSettings RegrasSeca { get; set; } = new();
    public RegraGeadaSettings RegrasGeada { get; set; } = new();
    public RegraCalorExcessivoSettings RegrasCalorExcessivo { get; set; } = new();
    public RegraExcessoUmidadeSettings RegrasExcessoUmidade { get; set; } = new();
    public RegraRiscoPragaSettings RegrasRiscoPraga { get; set; } = new();
}
