using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Analise.Test.Domain.Entities;

public class AlertaTests
{
    [Fact]
    public void Alerta_DeveSerCriadoComPropriedadesValidas()
    {
        // Arrange & Act
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Alto,
            Status = StatusAlerta.Ativo,
            Titulo = "Alerta de Seca",
            Mensagem = "Umidade do solo abaixo do limite crítico",
            Recomendacao = "Irrigação imediata recomendada",
            DataGeracao = DateTime.UtcNow,
            ValorReferencia = 25.5m
        };

        // Assert
        alerta.Id.Should().NotBeEmpty();
        alerta.TalhaoId.Should().NotBeEmpty();
        alerta.Tipo.Should().Be(TipoAlerta.Seca);
        alerta.Severidade.Should().Be(NivelSeveridade.Alto);
        alerta.Status.Should().Be(StatusAlerta.Ativo);
        alerta.Titulo.Should().Be("Alerta de Seca");
        alerta.Mensagem.Should().NotBeEmpty();
        alerta.Recomendacao.Should().NotBeEmpty();
        alerta.DataGeracao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        alerta.ValorReferencia.Should().Be(25.5m);
        alerta.DataVisualizacao.Should().BeNull();
        alerta.DataResolucao.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoAlerta.Seca)]
    [InlineData(TipoAlerta.Geada)]
    [InlineData(TipoAlerta.CalorExcessivo)]
    [InlineData(TipoAlerta.ExcessoUmidade)]
    [InlineData(TipoAlerta.RiscoPraga)]
    [InlineData(TipoAlerta.IrrigacaoRecomendada)]
    public void Alerta_DeveAceitarTodosTiposDeAlerta(TipoAlerta tipo)
    {
        // Arrange & Act
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = tipo,
            Severidade = NivelSeveridade.Medio,
            Status = StatusAlerta.Ativo,
            Titulo = $"Alerta de {tipo}",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow
        };

        // Assert
        alerta.Tipo.Should().Be(tipo);
    }

    [Theory]
    [InlineData(NivelSeveridade.Informativo)]
    [InlineData(NivelSeveridade.Baixo)]
    [InlineData(NivelSeveridade.Medio)]
    [InlineData(NivelSeveridade.Alto)]
    [InlineData(NivelSeveridade.Critico)]
    public void Alerta_DeveAceitarTodosNiveisDeSeveridade(NivelSeveridade severidade)
    {
        // Arrange & Act
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = severidade,
            Status = StatusAlerta.Ativo,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow
        };

        // Assert
        alerta.Severidade.Should().Be(severidade);
    }

    [Theory]
    [InlineData(StatusAlerta.Ativo)]
    [InlineData(StatusAlerta.Visualizado)]
    [InlineData(StatusAlerta.EmAndamento)]
    [InlineData(StatusAlerta.Resolvido)]
    [InlineData(StatusAlerta.Ignorado)]
    public void Alerta_DeveAceitarTodosStatusPossiveis(StatusAlerta status)
    {
        // Arrange & Act
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Status = status,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow
        };

        // Assert
        alerta.Status.Should().Be(status);
    }

    [Fact]
    public void Alerta_DevePermitirPropriedadesOpcionais()
    {
        // Arrange & Act
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Status = StatusAlerta.Ativo,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow,
            Recomendacao = null,
            DataVisualizacao = null,
            DataResolucao = null,
            ValorReferencia = null,
            DadosAdicionais = null,
            UsuarioId = null
        };

        // Assert
        alerta.Recomendacao.Should().BeNull();
        alerta.DataVisualizacao.Should().BeNull();
        alerta.DataResolucao.Should().BeNull();
        alerta.ValorReferencia.Should().BeNull();
        alerta.DadosAdicionais.Should().BeNull();
        alerta.UsuarioId.Should().BeNull();
    }

    [Fact]
    public void Alerta_DevePermitirSetarDataVisualizacao()
    {
        // Arrange
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Status = StatusAlerta.Ativo,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow
        };

        var dataVisualizacao = DateTime.UtcNow;

        // Act
        alerta.Status = StatusAlerta.Visualizado;
        alerta.DataVisualizacao = dataVisualizacao;

        // Assert
        alerta.Status.Should().Be(StatusAlerta.Visualizado);
        alerta.DataVisualizacao.Should().BeCloseTo(dataVisualizacao, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Alerta_DevePermitirSetarDataResolucao()
    {
        // Arrange
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Status = StatusAlerta.Ativo,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow
        };

        var dataResolucao = DateTime.UtcNow;

        // Act
        alerta.Status = StatusAlerta.Resolvido;
        alerta.DataResolucao = dataResolucao;

        // Assert
        alerta.Status.Should().Be(StatusAlerta.Resolvido);
        alerta.DataResolucao.Should().BeCloseTo(dataResolucao, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Alerta_DevePermitirDadosAdicionaisEmJSON()
    {
        // Arrange & Act
        var dadosJson = "{\"sensor\":\"SENSOR-001\",\"valor\":25.5,\"unidade\":\"%\"}";
        var alerta = new Alerta
        {
            Id = Guid.NewGuid(),
            TalhaoId = Guid.NewGuid(),
            Tipo = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Status = StatusAlerta.Ativo,
            Titulo = "Alerta Teste",
            Mensagem = "Mensagem teste",
            DataGeracao = DateTime.UtcNow,
            DadosAdicionais = dadosJson
        };

        // Assert
        alerta.DadosAdicionais.Should().Be(dadosJson);
    }
}
