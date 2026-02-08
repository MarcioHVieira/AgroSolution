namespace AgroSolutions.Analise.Configuration.Settings;

public class RegraSecaSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdUmidade { get; set; } = 30;
    public int DuracaoHoras { get; set; } = 24;
    public decimal SeveridadeCritico { get; set; } = 15;
    public decimal SeveridadeAlto { get; set; } = 20;
}
