using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Services;
using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using AgroSolutions.Propriedades.Domain.Interfaces;
using AgroSolutions.Propriedades.Infrastructure.Data;
using AgroSolutions.SharedKernel.Messaging;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Propriedades.Test.Application.Services;

public class TalhaoServiceTests
{
    private readonly Mock<ITalhaoRepository> _talhaoRepositoryMock;
    private readonly Mock<IPropriedadeRepository> _propriedadeRepositoryMock;
    private readonly Mock<IRabbitMQPublisher> _publisherMock;
    private readonly Mock<ILogger<TalhaoService>> _loggerMock;
    private readonly PropriedadesDbContext _context;
    private readonly TalhaoService _service;

    public TalhaoServiceTests()
    {
        _talhaoRepositoryMock = new Mock<ITalhaoRepository>();
        _propriedadeRepositoryMock = new Mock<IPropriedadeRepository>();
        _publisherMock = new Mock<IRabbitMQPublisher>();
        _loggerMock = new Mock<ILogger<TalhaoService>>();
        
        var options = new DbContextOptionsBuilder<PropriedadesDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase_" + Guid.NewGuid())
            .Options;
        _context = new PropriedadesDbContext(options);
        
        _service = new TalhaoService(
            _talhaoRepositoryMock.Object,
            _propriedadeRepositoryMock.Object,
            _publisherMock.Object,
            _loggerMock.Object,
            _context);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarTalhaoComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var dto = new CriarTalhaoDto(
            propriedade.Id,
            "Talhão A1",
            10m,
            "Descrição teste",
            -23.5505m,
            -46.6333m,
            null);

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CriarAsync(dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        resultado.Area.Should().Be(dto.Area);
        resultado.PropriedadeId.Should().Be(propriedade.Id);
        _talhaoRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoPropriedadeNaoEncontrada()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedadeId = Guid.NewGuid();
        var dto = new CriarTalhaoDto(propriedadeId, "Talhão A1", 10m);

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedadeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Propriedade?)null);

        // Act
        var act = async () => await _service.CriarAsync(dto, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{propriedadeId}*");
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);
        var dto = new CriarTalhaoDto(propriedade.Id, "Talhão A1", 10m);

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var act = async () => await _service.CriarAsync(dto, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoAreaInsuficiente()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var dto = new CriarTalhaoDto(propriedade.Id, "Talhão A1", 150m); // Maior que área total (100ha)

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        // Act
        var act = async () => await _service.CriarAsync(dto, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Área disponível insuficiente*");
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarTalhao_QuandoUsuarioProprietario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        // Act
        var resultado = await _service.ObterPorIdAsync(talhao.Id, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(talhao.Id);
        resultado.Nome.Should().Be(talhao.Nome);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoTalhaoNaoEncontrado()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Talhao?)null);

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
        var talhao = CriarTalhaoValido(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        // Act
        var act = async () => await _service.ObterPorIdAsync(talhao.Id, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    [Fact]
    public async Task ObterPorPropriedadeAsync_DeveRetornarTalhoesDaPropriedade()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhoes = new List<Talhao>
        {
            CriarTalhaoValido(propriedade),
            CriarTalhaoValido(propriedade)
        };

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorPropriedadeIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhoes);

        // Act
        var resultado = await _service.ObterPorPropriedadeAsync(propriedade.Id, usuarioId, false);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(t => t.PropriedadeId.Should().Be(propriedade.Id));
    }

    [Fact]
    public async Task ObterDisponiveisAsync_DeveRetornarTalhoesDisponiveis()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhoes = new List<Talhao>
        {
            CriarTalhaoValido(propriedade)
        };

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterDisponiveisPorPropriedadeIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhoes);

        // Act
        var resultado = await _service.ObterDisponiveisAsync(propriedade.Id, usuarioId, false);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.Should().AllSatisfy(t => t.Status.Should().Be(StatusTalhao.Disponivel));
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarTalhaoComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var dto = new AtualizarTalhaoDto(
            "Novo Nome",
            15m,
            "Nova descrição",
            -22.9068m,
            -43.1729m,
            null);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _talhaoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.AtualizarAsync(talhao.Id, dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Nome.Should().Be(dto.Nome);
        resultado.Area.Should().Be(dto.Area);
        _talhaoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarcarComoEmUsoAsync_DeveAlterarStatusComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _talhaoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoEmUsoAsync(talhao.Id, usuarioId, false);

        // Assert
        talhao.Status.Should().Be(StatusTalhao.EmUso);
        _talhaoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarcarComoDisponivelAsync_DeveAlterarStatusComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        talhao.MarcarComoEmUso();

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _talhaoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoDisponivelAsync(talhao.Id, usuarioId, false);

        // Assert
        talhao.Status.Should().Be(StatusTalhao.Disponivel);
        _talhaoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task MarcarComoEmDescansoAsync_DeveAlterarStatusComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _talhaoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.MarcarComoEmDescansoAsync(talhao.Id, usuarioId, false);

        // Assert
        talhao.Status.Should().Be(StatusTalhao.EmDescanso);
        _talhaoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemoverTalhaoComSucesso_QuandoSemCulturas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _talhaoRepositoryMock
            .Setup(x => x.RemoverAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoverAsync(talhao.Id, usuarioId, false);

        // Assert
        _talhaoRepositoryMock.Verify(x => x.RemoverAsync(talhao.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveLancarExcecao_QuandoTalhaoComCulturas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        
        // Usando reflection para adicionar uma cultura à coleção privada
        var culturasProperty = typeof(Talhao).GetProperty("Culturas");
        var culturas = new List<Cultura> { new Cultura(talhao.Id, TipoCultura.Soja, "Teste", 5m, DateTime.UtcNow) };
        culturasProperty?.SetValue(talhao, culturas);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        // Act
        var act = async () => await _service.RemoverAsync(talhao.Id, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*culturas*");
    }

    [Fact]
    public async Task RemoverAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);
        var talhao = CriarTalhaoValido(propriedade);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        // Act
        var act = async () => await _service.RemoverAsync(talhao.Id, usuarioSemPermissaoId, false);

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

    private static Talhao CriarTalhaoValido(Propriedade propriedade)
    {
        var talhao = new Talhao(propriedade.Id, "Talhão A1", 10m, "Descrição teste");
        
        // Usando reflection para definir a propriedade de navegação
        var propriedadeProperty = typeof(Talhao).GetProperty("Propriedade");
        propriedadeProperty?.SetValue(talhao, propriedade);
        
        return talhao;
    }
}
