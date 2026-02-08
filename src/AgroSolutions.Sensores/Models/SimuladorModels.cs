namespace AgroSolutions.Sensores.Models;

public record LeituraSimuladaDto
{
    public Guid TalhaoId { get; init; }
    public string TipoSensor { get; init; } = string.Empty;
    public decimal Valor { get; init; }
    public DateTime DataHora { get; init; }
}

public record ResultadoSimulacaoDto
{
    public string Cenario { get; init; } = string.Empty;
    public string Severidade { get; init; } = string.Empty;
    public int QuantidadeLeituras { get; init; }
    public DateTime InicioSimulacao { get; init; }
    public DateTime FimSimulacao { get; init; }
    public decimal ValorMedio { get; init; }
    public decimal ValorMinimo { get; init; }
    public decimal ValorMaximo { get; init; }
    public List<LeituraSimuladaDto> Leituras { get; init; } = new();
    public string Mensagem { get; init; } = string.Empty;
}

public record SimulacaoRequestDto
{
    public Guid TalhaoId { get; init; }
    public string Severidade { get; init; } = "Normal";
    public bool EnviarParaApi { get; init; } = true;
}

public enum TipoSensor
{
    UmidadeSolo,
    Temperatura,
    Precipitacao,
    Luminosidade,
    PhSolo
}

public enum Severidade
{
    Normal,
    Media,
    Alta,
    Critica
}
