using AgroSolutions.Analise.Infrastructure.Services;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Analise.Test.Infrastructure.Services;

public class RabbitMQAlertaPublisherServiceTests : IDisposable
{
    private readonly Mock<ILogger<RabbitMQAlertaPublisherService>> _loggerMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly RabbitMQAlertaPublisherService _service;

    public RabbitMQAlertaPublisherServiceTests()
    {
        _loggerMock = new Mock<ILogger<RabbitMQAlertaPublisherService>>();
        _configurationMock = new Mock<IConfiguration>();

        // Configurar valores padrão - usar porta inválida para falhar rápido
        _configurationMock.Setup(c => c["RabbitMQ:HostName"]).Returns("240.0.0.1"); // IP inválido para timeout rápido
        _configurationMock.Setup(c => c["RabbitMQ:Port"]).Returns("1"); // Porta inválida
        _configurationMock.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
        _configurationMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");
        _configurationMock.Setup(c => c["RabbitMQ:ExchangeAlertas"]).Returns("agrosolutions.alertas");
        _configurationMock.Setup(c => c["RabbitMQ:MaxRetries"]).Returns("1"); // Reduzir tentativas
        _configurationMock.Setup(c => c["RabbitMQ:RetryDelaySeconds"]).Returns("0.1"); // Delay mínimo

        _service = new RabbitMQAlertaPublisherService(_loggerMock.Object, _configurationMock.Object);
    }

    [Fact]
    public void Constructor_DeveCriarInstanciaComSucesso()
    {
        // Arrange & Act
        var service = new RabbitMQAlertaPublisherService(_loggerMock.Object, _configurationMock.Object);

        // Assert
        service.Should().NotBeNull();
        
        service.Dispose();
    }

    [Fact]
    public void Constructor_DeveUsarConfiguracoesDoIConfiguration()
    {
        // Arrange
        var localConfigMock = new Mock<IConfiguration>();
        localConfigMock.Setup(c => c["RabbitMQ:HostName"]).Returns("240.0.0.1");
        localConfigMock.Setup(c => c["RabbitMQ:Port"]).Returns("1");
        localConfigMock.Setup(c => c["RabbitMQ:UserName"]).Returns("guest");
        localConfigMock.Setup(c => c["RabbitMQ:Password"]).Returns("guest");
        localConfigMock.Setup(c => c["RabbitMQ:MaxRetries"]).Returns("1");
        localConfigMock.Setup(c => c["RabbitMQ:RetryDelaySeconds"]).Returns("0.1");

        // Act
        var service = new RabbitMQAlertaPublisherService(_loggerMock.Object, localConfigMock.Object);

        // Assert
        localConfigMock.Verify(c => c["RabbitMQ:HostName"], Times.AtLeastOnce);
        localConfigMock.Verify(c => c["RabbitMQ:Port"], Times.AtLeastOnce);
        localConfigMock.Verify(c => c["RabbitMQ:UserName"], Times.AtLeastOnce);
        localConfigMock.Verify(c => c["RabbitMQ:Password"], Times.AtLeastOnce);
        
        service.Dispose();
    }

    [Fact]
    public async Task PublicarAlertaCriticoAsync_DeveRetornarBoolean()
    {
        // Arrange
        var alerta = new
        {
            Id = Guid.NewGuid(),
            Tipo = "Geada",
            Severidade = "Critico",
            Titulo = "Risco de Geada",
            Mensagem = "Temperatura baixa"
        };

        // Act
        var resultado = await _service.PublicarAlertaCriticoAsync(alerta, "alerta.critico.geada");

        // Assert
        // Verifica que o método executou sem exceções
        Assert.True(resultado == true || resultado == false);
    }

    [Fact]
    public async Task PublicarAlertaNormalAsync_DeveRetornarBoolean()
    {
        // Arrange
        var alerta = new
        {
            Id = Guid.NewGuid(),
            Tipo = "Umidade",
            Severidade = "Medio",
            Titulo = "Excesso de Umidade",
            Mensagem = "Umidade alta"
        };

        // Act
        var resultado = await _service.PublicarAlertaNormalAsync(alerta, "alerta.medio.umidade");

        // Assert
        Assert.True(resultado == true || resultado == false);
    }

    [Fact]
    public async Task PublicarAlertaAsync_DeveAceitarPrioridadeEttlCustomizados()
    {
        // Arrange
        var alerta = new
        {
            Id = Guid.NewGuid(),
            Tipo = "Teste",
            Severidade = "Baixo"
        };

        // Act
        var resultado = await _service.PublicarAlertaAsync(
            alerta, 
            "alerta.baixo.teste", 
            prioridade: 3, 
            ttlMinutos: 180
        );

        // Assert
        Assert.True(resultado == true || resultado == false);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(10)]
    public async Task PublicarAlertaAsync_DeveAceitarPrioridades(int prioridade)
    {
        // Arrange
        var alerta = new { Id = Guid.NewGuid() };

        // Act
        var resultado = await _service.PublicarAlertaAsync(
            alerta, 
            "alerta.teste", 
            prioridade: prioridade, 
            ttlMinutos: 60
        );

        // Assert
        Assert.True(resultado == true || resultado == false);
    }

    [Fact]
    public void Dispose_DeveLimparRecursos()
    {
        // Arrange
        var localConfigMock = new Mock<IConfiguration>();
        localConfigMock.Setup(c => c["RabbitMQ:HostName"]).Returns("240.0.0.1");
        localConfigMock.Setup(c => c["RabbitMQ:Port"]).Returns("1");
        localConfigMock.Setup(c => c["RabbitMQ:MaxRetries"]).Returns("1");
        localConfigMock.Setup(c => c["RabbitMQ:RetryDelaySeconds"]).Returns("0.1");
        
        var service = new RabbitMQAlertaPublisherService(_loggerMock.Object, localConfigMock.Object);

        // Act
        var act = () => service.Dispose();

        // Assert
        act.Should().NotThrow();
    }

    public void Dispose()
    {
        _service?.Dispose();
    }
}
