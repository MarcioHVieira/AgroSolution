namespace AgroSolutions.Analise.Configuration.Settings;

public class QoSSettings
{
    public uint PrefetchSize { get; set; } = 0;
    public ushort PrefetchCount { get; set; } = 1;
    public bool Global { get; set; } = false;
}
