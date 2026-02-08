namespace AgroSolutions.SharedKernel.Messaging;

/// <summary>
/// Configurações do RabbitMQ
/// </summary>
public class RabbitMQSettings
{
    public string HostName { get; set; } = "localhost";
    public int Port { get; set; } = 5672;
    public string UserName { get; set; } = "guest";
    public string Password { get; set; } = "guest";
    public string ExchangeName { get; set; } = "agrosolutions";
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int NetworkRecoveryIntervalSeconds { get; set; } = 10;
}
