using AgroSolutions.Analise.Domain.Entities;
using AgroSolutions.Analise.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Analise.Test.Domain.Entities;

public class RegraAlertaTests
{
    [Fact]
    public void RegraAlerta_DeveSerCriadaComPropriedadesValidas()
    {
        // Arrange & Act
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra de Seca",
            Descricao = "Detecta condições de seca no solo",
            TipoAlerta = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Alto,
            Ativa = true,
            Condicao = "{\"campo\":\"UmidadeSolo\",\"operador\":\"<\",\"valor\":30,\"duracao\":24}",
            TemplateMensagem = "Umidade do solo abaixo de {valor}% por {duracao} horas",
            Recomendacao = "Irrigação imediata recomendada",
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.Id.Should().NotBeEmpty();
        regra.Nome.Should().Be("Regra de Seca");
        regra.Descricao.Should().NotBeEmpty();
        regra.TipoAlerta.Should().Be(TipoAlerta.Seca);
        regra.Severidade.Should().Be(NivelSeveridade.Alto);
        regra.Ativa.Should().BeTrue();
        regra.Condicao.Should().NotBeEmpty();
        regra.TemplateMensagem.Should().NotBeEmpty();
        regra.Recomendacao.Should().NotBeEmpty();
        regra.DataCriacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        regra.DataAtualizacao.Should().BeNull();
    }

    [Theory]
    [InlineData(TipoAlerta.Seca)]
    [InlineData(TipoAlerta.Geada)]
    [InlineData(TipoAlerta.CalorExcessivo)]
    [InlineData(TipoAlerta.ExcessoUmidade)]
    [InlineData(TipoAlerta.RiscoPraga)]
    [InlineData(TipoAlerta.IrrigacaoRecomendada)]
    public void RegraAlerta_DeveAceitarTodosTiposDeAlerta(TipoAlerta tipo)
    {
        // Arrange & Act
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = $"Regra de {tipo}",
            TipoAlerta = tipo,
            Severidade = NivelSeveridade.Medio,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.TipoAlerta.Should().Be(tipo);
    }

    [Fact]
    public void RegraAlerta_DevePermitirDesativacao()
    {
        // Arrange
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra Teste",
            TipoAlerta = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow
        };

        // Act
        regra.Ativa = false;

        // Assert
        regra.Ativa.Should().BeFalse();
    }

    [Fact]
    public void RegraAlerta_DevePermitirAtualizacaoComDataRegistrada()
    {
        // Arrange
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra Teste",
            TipoAlerta = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow.AddDays(-10)
        };

        var dataAtualizacao = DateTime.UtcNow;

        // Act
        regra.Nome = "Regra Atualizada";
        regra.DataAtualizacao = dataAtualizacao;

        // Assert
        regra.Nome.Should().Be("Regra Atualizada");
        regra.DataAtualizacao.Should().BeCloseTo(dataAtualizacao, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void RegraAlerta_DevePermitirCondicaoComplexaEmJSON()
    {
        // Arrange & Act
        var condicaoCompleta = @"{
            ""campo"": ""UmidadeSolo"",
            ""operador"": ""<"",
            ""valor"": 30,
            ""duracao"": 24,
            ""condicoesAdicionais"": [
                {""campo"": ""Temperatura"", ""operador"": "">"", ""valor"": 25}
            ]
        }";

        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra Complexa",
            TipoAlerta = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Alto,
            Ativa = true,
            Condicao = condicaoCompleta,
            TemplateMensagem = "Alerta complexo",
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.Condicao.Should().Contain("campo");
        regra.Condicao.Should().Contain("operador");
        regra.Condicao.Should().Contain("valor");
        regra.Condicao.Should().Contain("duracao");
    }

    [Fact]
    public void RegraAlerta_DevePermitirTemplateComVariaveis()
    {
        // Arrange & Act
        var template = "Umidade do solo está em {valor}% (limite: {threshold}%) por {duracao} horas";
        
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra com Template",
            TipoAlerta = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = template,
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.TemplateMensagem.Should().Contain("{valor}");
        regra.TemplateMensagem.Should().Contain("{threshold}");
        regra.TemplateMensagem.Should().Contain("{duracao}");
    }

    [Theory]
    [InlineData(NivelSeveridade.Informativo)]
    [InlineData(NivelSeveridade.Baixo)]
    [InlineData(NivelSeveridade.Medio)]
    [InlineData(NivelSeveridade.Alto)]
    [InlineData(NivelSeveridade.Critico)]
    public void RegraAlerta_DeveAceitarTodosNiveisDeSeveridadePadrao(NivelSeveridade severidade)
    {
        // Arrange & Act
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra Teste",
            TipoAlerta = TipoAlerta.Seca,
            Severidade = severidade,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.Severidade.Should().Be(severidade);
    }

    [Fact]
    public void RegraAlerta_DevePermitirDescricaoNula()
    {
        // Arrange & Act
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra Teste",
            Descricao = null,
            TipoAlerta = TipoAlerta.Seca,
            Severidade = NivelSeveridade.Medio,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = "Mensagem teste",
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.Descricao.Should().BeNull();
    }

    [Fact]
    public void RegraAlerta_DevePermitirRecomendacaoNula()
    {
        // Arrange & Act
        var regra = new RegraAlerta
        {
            Id = Guid.NewGuid(),
            Nome = "Regra Teste",
            TipoAlerta = TipoAlerta.IrrigacaoRecomendada,
            Severidade = NivelSeveridade.Informativo,
            Ativa = true,
            Condicao = "{}",
            TemplateMensagem = "Condições ideais para irrigação",
            Recomendacao = null,
            DataCriacao = DateTime.UtcNow
        };

        // Assert
        regra.Recomendacao.Should().BeNull();
    }
}
