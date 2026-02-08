namespace AgroSolutions.Sensores.Configuration;

public class SimuladorSettings
{
    public IngestaoApiSettings IngestaoApi { get; set; } = new();
    public AutenticacaoSettings Autenticacao { get; set; } = new();
    public RegraSeca RegrasSeca { get; set; } = new();
    public RegraGeada RegrasGeada { get; set; } = new();
    public RegraCalorExcessivo RegrasCalorExcessivo { get; set; } = new();
    public RegraExcessoUmidade RegrasExcessoUmidade { get; set; } = new();
    public RegraRiscoPraga RegrasRiscoPraga { get; set; } = new();
    public SimulacaoSettings Simulacao { get; set; } = new();
}

public class IngestaoApiSettings
{
    public string BaseUrl { get; set; } = "http://localhost:5003";
    public string LeiturasEndpoint { get; set; } = "/api/leituras";
    public int TimeoutSeconds { get; set; } = 30;
}

public class AutenticacaoSettings
{
    public string IdentidadeUrl { get; set; } = "http://localhost:5001";
    public string Email { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

public class RegraSeca
{
    public decimal ThresholdNormal { get; set; } = 30.0m;
    public decimal ThresholdMedia { get; set; } = 25.0m;
    public decimal ThresholdAlta { get; set; } = 20.0m;
    public decimal ThresholdCritica { get; set; } = 15.0m;
    public int DuracaoHoras { get; set; } = 24;
    public int IntervaloMinutos { get; set; } = 60;
}

public class RegraGeada
{
    public decimal ThresholdNormal { get; set; } = 2.0m;
    public decimal ThresholdMedia { get; set; } = 1.5m;
    public decimal ThresholdAlta { get; set; } = 1.0m;
    public decimal ThresholdCritica { get; set; } = 0.0m;
    public int DuracaoHoras { get; set; } = 6;
    public int IntervaloMinutos { get; set; } = 30;
}

public class RegraCalorExcessivo
{
    public decimal ThresholdNormal { get; set; } = 35.0m;
    public decimal ThresholdMedia { get; set; } = 37.0m;
    public decimal ThresholdAlta { get; set; } = 40.0m;
    public decimal ThresholdCritica { get; set; } = 43.0m;
    public int DuracaoHoras { get; set; } = 12;
    public int IntervaloMinutos { get; set; } = 60;
}

public class RegraExcessoUmidade
{
    public decimal ThresholdNormal { get; set; } = 85.0m;
    public decimal ThresholdMedia { get; set; } = 88.0m;
    public decimal ThresholdAlta { get; set; } = 92.0m;
    public decimal ThresholdCritica { get; set; } = 95.0m;
    public int DuracaoHoras { get; set; } = 48;
    public int IntervaloMinutos { get; set; } = 120;
}

public class RegraRiscoPraga
{
    public decimal TemperaturaMin { get; set; } = 20.0m;
    public decimal TemperaturaMax { get; set; } = 30.0m;
    public decimal ThresholdUmidade { get; set; } = 70.0m;
    public int DuracaoHoras { get; set; } = 48;
    public int IntervaloMinutos { get; set; } = 120;
}

public class SimulacaoSettings
{
    public decimal VariacaoAleatoria { get; set; } = 2.0m;
    public bool AdicionarRuido { get; set; } = true;
    public int DelayEntreLeiturasMs { get; set; } = 100;
}
