namespace AgroSolutions.Analise.Configuration.Settings;

public class RegraCalorExcessivoSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdTemperatura { get; set; } = 35;
    public int DuracaoHoras { get; set; } = 6;
    public decimal SeveridadeCritico { get; set; } = 42;
    public decimal SeveridadeAlto { get; set; } = 38;
}
