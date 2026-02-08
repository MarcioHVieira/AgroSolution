namespace AgroSolutions.Analise.Configuration.Settings;

public class RegraGeadaSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdTemperatura { get; set; } = 5;
    public int DuracaoHoras { get; set; } = 6;
    public decimal SeveridadeCritico { get; set; } = 0;
    public decimal SeveridadeAlto { get; set; } = 2;
}
