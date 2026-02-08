namespace AgroSolutions.Analise.Configuration.Settings;

/// <summary>
/// Configurações do RabbitMQ
/// </summary>
public class RabbitMQSettings
{
    public string HostName { get; set; } = string.Empty;
    public string Port { get; set; } = "5672";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string ExchangeType { get; set; } = "topic";
    public string QueueName { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public bool Durable { get; set; } = true;
    public bool AutoDelete { get; set; } = false;
    public bool Exclusive { get; set; } = false;
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    public int NetworkRecoveryIntervalSeconds { get; set; } = 10;
    public QoSSettings QoS { get; set; } = new();
}
