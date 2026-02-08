namespace AgroSolutions.Analise.Configuration.Settings;

public class RegraRiscoPragaSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal TemperaturaMinima { get; set; } = 20;
    public decimal TemperaturaMaxima { get; set; } = 30;
    public decimal ThresholdUmidade { get; set; } = 70;
    public decimal PercentualCondicoes { get; set; } = 70;
    public int DuracaoHoras { get; set; } = 48;
}
