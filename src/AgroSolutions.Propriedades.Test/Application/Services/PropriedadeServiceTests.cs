using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Services;
using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Propriedades.Test.Application.Services;

public class PropriedadeServiceTests
{
    private readonly Mock<IPropriedadeRepository> _repositoryMock;
    private readonly Mock<IUsuarioInfoRepository> _usuarioInfoRepositoryMock;
    private readonly Mock<IRabbitMQPublisher> _publisherMock;
    private readonly Mock<ILogger<PropriedadeService>> _loggerMock;
    private readonly PropriedadeService _service;

    public PropriedadeServiceTests()
    {
        _repositoryMock = new Mock<IPropriedadeRepository>();
        _usuarioInfoRepositoryMock = new Mock<IUsuarioInfoRepository>();
        _publisherMock = new Mock<IRabbitMQPublisher>();
        _loggerMock = new Mock<ILogger<PropriedadeService>>();
        
        // Configurar mock do UsuarioInfoRepository para retornar dados padrão
        _usuarioInfoRepositoryMock
            .Setup(x => x.ObterDadosUsuarioAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(("teste@agrosolutions.com", "Usuário Teste"));
        
        _service = new PropriedadeService(
            _repositoryMock.Object, 
            _usuarioInfoRepositoryMock.Object,
            _publisherMock.Object, 
            _loggerMock.Object);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarPropriedadeComSucesso()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var dto = new CriarPropriedadeDto(
            "Fazenda Boa Vista",
            100m,
            TipoPropriedade.Fazenda,
            "12345-678",
            "Estrada Rural",
            "Zona Rural",
            "Cidade Teste",
            "SE",
            "Descrição teste",
            "123",
            "Complemento",
            -23.5505m,
            -46.6333m);

        _repositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CriarAsync(proprietarioId, dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        resultado.AreaTotal.Should().Be(dto.AreaTotal);
        resultado.Tipo.Should().Be(dto.Tipo);
        resultado.ProprietarioId.Should().Be(proprietarioId);
        _repositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarPropriedade_QuandoUsuarioProprietario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var resultado = await _service.ObterPorIdAsync(propriedade.Id, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(propriedade.Id);
        resultado.Nome.Should().Be(propriedade.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarPropriedade_QuandoUsuarioAdmin()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var resultado = await _service.ObterPorIdAsync(propriedade.Id, adminId, true);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(propriedade.Id);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoPropriedadeNaoEncontrada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Propriedade?)null);

        // Act
        var act = async () => await _service.ObterPorIdAsync(id, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{id}*");
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var act = async () => await _service.ObterPorIdAsync(propriedade.Id, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    [Fact]
    public async Task ObterPorProprietarioAsync_DeveRetornarPropriedadesDoProprietario()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var propriedades = new List<Propriedade>
        {
            CriarPropriedadeValida(proprietarioId),
            CriarPropriedadeValida(proprietarioId)
        };

        _repositoryMock
            .Setup(x => x.ObterPorProprietarioIdAsync(proprietarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedades);

        // Act
        var resultado = await _service.ObterPorProprietarioAsync(proprietarioId);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(p => p.ProprietarioId.Should().Be(proprietarioId));
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarTodasPropriedades_QuandoUsuarioAdmin()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var propriedades = new List<Propriedade>
        {
            CriarPropriedadeValida(Guid.NewGuid()),
            CriarPropriedadeValida(Guid.NewGuid())
        };

        _repositoryMock
            .Setup(x => x.ObterTodasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedades);

        // Act
        var resultado = await _service.ObterTodasAsync(adminId, true);

        // Assert
        resultado.Should().HaveCount(2);
        _repositoryMock.Verify(x => x.ObterTodasAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ObterTodasAsync_DeveRetornarApenasPropriedadesDoUsuario_QuandoNaoAdmin()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedades = new List<Propriedade>
        {
            CriarPropriedadeValida(usuarioId)
        };

        _repositoryMock
            .Setup(x => x.ObterPorProprietarioIdAsync(usuarioId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedades);

        // Act
        var resultado = await _service.ObterTodasAsync(usuarioId, false);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().ProprietarioId.Should().Be(usuarioId);
        _repositoryMock.Verify(x => x.ObterPorProprietarioIdAsync(usuarioId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarPropriedadeComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var dto = new AtualizarPropriedadeDto(
            "Novo Nome",
            150m,
            TipoPropriedade.Sitio,
            "Nova descrição",
            -22.9068m,
            -43.1729m);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.AtualizarAsync(propriedade.Id, dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        resultado.AreaTotal.Should().Be(dto.AreaTotal);
        resultado.Tipo.Should().Be(dto.Tipo);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtualizarAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);
        var dto = new AtualizarPropriedadeDto("Novo Nome", 150m, TipoPropriedade.Sitio);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var act = async () => await _service.AtualizarAsync(propriedade.Id, dto, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    [Fact]
    public async Task AtualizarEnderecoAsync_DeveAtualizarEnderecoComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var dto = new AtualizarEnderecoPropriedadeDto(
            "12345-678",
            "Rua Nova",
            "Bairro Novo",
            "Cidade Nova",
            "AL",
            "456",
            "Complemento novo");

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.AtualizarEnderecoAsync(propriedade.Id, dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Cep.Should().Be(dto.Cep);
        resultado.Endereco.Should().Be(dto.Endereco);
        resultado.Cidade.Should().Be(dto.Cidade);
        resultado.Estado.Should().Be(dto.Estado);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AtivarAsync_DeveAtivarPropriedadeComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        propriedade.Inativar();

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.AtivarAsync(propriedade.Id, usuarioId, false);

        // Assert
        propriedade.Status.Should().Be(StatusPropriedade.Ativa);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task InativarAsync_DeveInativarPropriedadeComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _repositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.InativarAsync(propriedade.Id, usuarioId, false);

        // Assert
        propriedade.Status.Should().Be(StatusPropriedade.Inativa);
        _repositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Propriedade>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemoverPropriedadeComSucesso_QuandoSemTalhoes()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _repositoryMock
            .Setup(x => x.RemoverAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoverAsync(propriedade.Id, usuarioId, false);

        // Assert
        _repositoryMock.Verify(x => x.RemoverAsync(propriedade.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveLancarExcecao_QuandoPropriedadeComTalhoes()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        
        // Usando reflection para adicionar um talhão à coleção privada
        var talhoesProperty = typeof(Propriedade).GetProperty("Talhoes");
        var talhoes = new List<Talhao> { new Talhao(propriedade.Id, "Talhão 1", 10m) };
        talhoesProperty?.SetValue(propriedade, talhoes);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var act = async () => await _service.RemoverAsync(propriedade.Id, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*talhões*");
    }

    [Fact]
    public async Task RemoverAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);

        _repositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var act = async () => await _service.RemoverAsync(propriedade.Id, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    private static Propriedade CriarPropriedadeValida(Guid proprietarioId)
    {
        return new Propriedade(
            proprietarioId,
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
