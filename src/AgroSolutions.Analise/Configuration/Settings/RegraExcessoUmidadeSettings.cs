namespace AgroSolutions.Analise.Configuration.Settings;

public class RegraExcessoUmidadeSettings
{
    public bool Habilitada { get; set; } = true;
    public decimal ThresholdUmidade { get; set; } = 85;
    public int DuracaoHoras { get; set; } = 48;
    public decimal SeveridadeAlto { get; set; } = 95;
}
