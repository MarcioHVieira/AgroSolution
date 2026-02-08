using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using FluentAssertions;

namespace AgroSolutions.Propriedades.Test.Domain.Entities;

public class PropriedadeTests
{
    [Fact]
    public void Construtor_DeveCrearPropriedadeComDadosValidos()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var nome = "Fazenda Boa Vista";
        var areaTotal = 100m;
        var tipo = TipoPropriedade.Fazenda;
        var cep = "12345-678";
        var endereco = "Estrada Rural, 123";
        var bairro = "Zona Rural";
        var cidade = "Cidade Teste";
        var estado = "SE";
        var descricao = "Fazenda produtora de grãos";
        var numero = "123";
        var complemento = "Próximo ao rio";
        var latitude = -23.5505m;
        var longitude = -46.6333m;

        // Act
        var propriedade = new Propriedade(
            proprietarioId,
            nome,
            areaTotal,
            tipo,
            cep,
            endereco,
            bairro,
            cidade,
            estado,
            descricao,
            numero,
            complemento,
            latitude,
            longitude);

        // Assert
        propriedade.Id.Should().NotBeEmpty();
        propriedade.ProprietarioId.Should().Be(proprietarioId);
        propriedade.Nome.Should().Be(nome);
        propriedade.Descricao.Should().Be(descricao);
        propriedade.AreaTotal.Should().Be(areaTotal);
        propriedade.Tipo.Should().Be(tipo);
        propriedade.Cep.Should().Be(cep);
        propriedade.Endereco.Should().Be(endereco);
        propriedade.Numero.Should().Be(numero);
        propriedade.Complemento.Should().Be(complemento);
        propriedade.Bairro.Should().Be(bairro);
        propriedade.Cidade.Should().Be(cidade);
        propriedade.Estado.Should().Be(estado);
        propriedade.Latitude.Should().Be(latitude);
        propriedade.Longitude.Should().Be(longitude);
        propriedade.Status.Should().Be(StatusPropriedade.Ativa);
        propriedade.DataCadastro.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        propriedade.DataAtualizacao.Should().BeNull();
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoNomeInvalido(string? nomeInvalido)
    {
        // Arrange & Act
        var act = () => new Propriedade(
            Guid.NewGuid(),
            nomeInvalido!,
            100m,
            TipoPropriedade.Fazenda,
            "12345-678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade",
            "SE");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Nome*");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Construtor_DeveLancarExcecao_QuandoAreaTotalInvalida(decimal areaInvalida)
    {
        // Arrange & Act
        var act = () => new Propriedade(
            Guid.NewGuid(),
            "Fazenda Teste",
            areaInvalida,
            TipoPropriedade.Fazenda,
            "12345-678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade",
            "SE");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*área total*");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoCepInvalido(string? cepInvalido)
    {
        // Arrange & Act
        var act = () => new Propriedade(
            Guid.NewGuid(),
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            cepInvalido!,
            "Estrada Rural",
            "Zona Rural",
            "Cidade",
            "SE");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*CEP*");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoEnderecoInvalido(string? enderecoInvalido)
    {
        // Arrange & Act
        var act = () => new Propriedade(
            Guid.NewGuid(),
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345-678",
            enderecoInvalido!,
            "Zona Rural",
            "Cidade",
            "SE");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Endereço*");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoCidadeInvalida(string? cidadeInvalida)
    {
        // Arrange & Act
        var act = () => new Propriedade(
            Guid.NewGuid(),
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345-678",
            "Estrada Rural",
            "Zona Rural",
            cidadeInvalida!,
            "SE");

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Cidade*");
    }

    [Theory]
    [InlineData(null!)]
    [InlineData("")]
    [InlineData("   ")]
    public void Construtor_DeveLancarExcecao_QuandoEstadoInvalido(string? estadoInvalido)
    {
        // Arrange & Act
        var act = () => new Propriedade(
            Guid.NewGuid(),
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345-678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade",
            estadoInvalido!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("*Estado*");
    }

    [Fact]
    public void Atualizar_DeveAtualizarPropriedadeComSucesso()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();
        var novoNome = "Fazenda Nova Vista";
        var novaArea = 150m;
        var novoTipo = TipoPropriedade.Sitio;
        var novaDescricao = "Nova descrição";
        var novaLatitude = -22.9068m;
        var novaLongitude = -43.1729m;

        // Act
        propriedade.Atualizar(novoNome, novaArea, novoTipo, novaDescricao, novaLatitude, novaLongitude);

        // Assert
        propriedade.Nome.Should().Be(novoNome);
        propriedade.AreaTotal.Should().Be(novaArea);
        propriedade.Tipo.Should().Be(novoTipo);
        propriedade.Descricao.Should().Be(novaDescricao);
        propriedade.Latitude.Should().Be(novaLatitude);
        propriedade.Longitude.Should().Be(novaLongitude);
        propriedade.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void AtualizarEndereco_DeveAtualizarEnderecoComSucesso()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();
        var novoCep = "98765-432";
        var novoEndereco = "Rua Nova";
        var novoBairro = "Bairro Novo";
        var novaCidade = "Cidade Nova";
        var novoEstado = "AL";
        var novoNumero = "456";
        var novoComplemento = "Ao lado da praça";

        // Act
        propriedade.AtualizarEndereco(novoCep, novoEndereco, novoBairro, novaCidade, novoEstado, novoNumero, novoComplemento);

        // Assert
        propriedade.Cep.Should().Be(novoCep);
        propriedade.Endereco.Should().Be(novoEndereco);
        propriedade.Bairro.Should().Be(novoBairro);
        propriedade.Cidade.Should().Be(novaCidade);
        propriedade.Estado.Should().Be(novoEstado);
        propriedade.Numero.Should().Be(novoNumero);
        propriedade.Complemento.Should().Be(novoComplemento);
        propriedade.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Ativar_DeveAlterarStatusParaAtiva()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();
        propriedade.Inativar();

        // Act
        propriedade.Ativar();

        // Assert
        propriedade.Status.Should().Be(StatusPropriedade.Ativa);
        propriedade.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Inativar_DeveAlterarStatusParaInativa()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();

        // Act
        propriedade.Inativar();

        // Assert
        propriedade.Status.Should().Be(StatusPropriedade.Inativa);
        propriedade.DataAtualizacao.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void CalcularAreaDisponivel_DeveRetornarAreaTotalQuandoSemTalhoes()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();

        // Act
        var areaDisponivel = propriedade.CalcularAreaDisponivel();

        // Assert
        areaDisponivel.Should().Be(propriedade.AreaTotal);
    }

    [Fact]
    public void PossuiAreaDisponivel_DeveRetornarTrue_QuandoAreaDisponivel()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();
        var areaRequerida = 50m;

        // Act
        var possui = propriedade.PossuiAreaDisponivel(areaRequerida);

        // Assert
        possui.Should().BeTrue();
    }

    [Fact]
    public void PossuiAreaDisponivel_DeveRetornarFalse_QuandoAreaInsuficiente()
    {
        // Arrange
        var propriedade = CriarPropriedadeValida();
        var areaRequerida = 150m; // Maior que área total (100ha)

        // Act
        var possui = propriedade.PossuiAreaDisponivel(areaRequerida);

        // Assert
        possui.Should().BeFalse();
    }

    private static Propriedade CriarPropriedadeValida()
    {
        return new Propriedade(
            Guid.NewGuid(),
            "Fazenda Teste",
            100m,
            TipoPropriedade.Fazenda,
            "12345-678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE");
    }
}
