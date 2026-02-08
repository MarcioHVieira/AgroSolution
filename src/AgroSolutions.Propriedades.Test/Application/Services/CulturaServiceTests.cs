using AgroSolutions.Propriedades.Application.DTOs;
using AgroSolutions.Propriedades.Application.Services;
using AgroSolutions.Propriedades.Domain.Entities;
using AgroSolutions.Propriedades.Domain.Enums;
using AgroSolutions.Propriedades.Domain.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace AgroSolutions.Propriedades.Test.Application.Services;

public class CulturaServiceTests
{
    private readonly Mock<ICulturaRepository> _culturaRepositoryMock;
    private readonly Mock<ITalhaoRepository> _talhaoRepositoryMock;
    private readonly Mock<IPropriedadeRepository> _propriedadeRepositoryMock;
    private readonly Mock<ILogger<CulturaService>> _loggerMock;
    private readonly CulturaService _service;

    public CulturaServiceTests()
    {
        _culturaRepositoryMock = new Mock<ICulturaRepository>();
        _talhaoRepositoryMock = new Mock<ITalhaoRepository>();
        _propriedadeRepositoryMock = new Mock<IPropriedadeRepository>();
        _loggerMock = new Mock<ILogger<CulturaService>>();
        _service = new CulturaService(
            _culturaRepositoryMock.Object,
            _talhaoRepositoryMock.Object,
            _propriedadeRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task CriarAsync_DeveCriarCulturaComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var dto = new CriarCulturaDto(
            talhao.Id,
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(90),
            15m,
            "Cultura teste");

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _culturaRepositoryMock
            .Setup(x => x.AdicionarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _talhaoRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CriarAsync(dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Tipo.Should().Be(dto.Tipo);
        resultado.Variedade.Should().Be(dto.Variedade);
        resultado.AreaPlantada.Should().Be(dto.AreaPlantada);
        _culturaRepositoryMock.Verify(x => x.AdicionarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()), Times.Once);
        _talhaoRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Talhao>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoTalhaoNaoEncontrado()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var talhaoId = Guid.NewGuid();
        var dto = new CriarCulturaDto(
            talhaoId,
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhaoId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Talhao?)null);

        // Act
        var act = async () => await _service.CriarAsync(dto, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<KeyNotFoundException>()
            .WithMessage($"*{talhaoId}*");
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var dto = new CriarCulturaDto(
            talhao.Id,
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        // Act
        var act = async () => await _service.CriarAsync(dto, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    [Fact]
    public async Task CriarAsync_DeveLancarExcecao_QuandoAreaPlantadaExcedeAreaTalhao()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var dto = new CriarCulturaDto(
            talhao.Id,
            TipoCultura.Soja,
            "Monsoy 6410",
            15m, // Maior que área do talhão (10ha)
            DateTime.UtcNow);

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        // Act
        var act = async () => await _service.CriarAsync(dto, usuarioId, false);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*excede*");
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveRetornarCultura_QuandoUsuarioProprietario()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var cultura = CriarCulturaValida(talhao);

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        // Act
        var resultado = await _service.ObterPorIdAsync(cultura.Id, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Id.Should().Be(cultura.Id);
        resultado.Variedade.Should().Be(cultura.Variedade);
    }

    [Fact]
    public async Task ObterPorIdAsync_DeveLancarExcecao_QuandoCulturaNaoEncontrada()
    {
        // Arrange
        var id = Guid.NewGuid();
        var usuarioId = Guid.NewGuid();

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Cultura?)null);

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
        var cultura = CriarCulturaValida(talhao);

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        // Act
        var act = async () => await _service.ObterPorIdAsync(cultura.Id, usuarioSemPermissaoId, false);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*permissão*");
    }

    [Fact]
    public async Task ObterPorTalhaoAsync_DeveRetornarCulturas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var culturas = new List<Cultura>
        {
            CriarCulturaValida(talhao),
            CriarCulturaValida(talhao)
        };

        _talhaoRepositoryMock
            .Setup(x => x.ObterPorIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(talhao);

        _culturaRepositoryMock
            .Setup(x => x.ObterPorTalhaoIdAsync(talhao.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(culturas);

        // Act
        var resultado = await _service.ObterPorTalhaoAsync(talhao.Id, usuarioId, false);

        // Assert
        resultado.Should().HaveCount(2);
        resultado.Should().AllSatisfy(c => c.TalhaoId.Should().Be(talhao.Id));
    }

    [Fact]
    public async Task ObterPorPropriedadeAsync_DeveRetornarCulturas()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var culturas = new List<Cultura>
        {
            CriarCulturaValida(talhao)
        };

        _propriedadeRepositoryMock
            .Setup(x => x.ObterPorIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(propriedade);

        _culturaRepositoryMock
            .Setup(x => x.ObterPorPropriedadeIdAsync(propriedade.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(culturas);

        // Act
        var resultado = await _service.ObterPorPropriedadeAsync(propriedade.Id, usuarioId, false);

        // Assert
        resultado.Should().HaveCount(1);
    }

    [Fact]
    public async Task ObterAtivasAsync_DeveRetornarTodasCulturasAtivas_QuandoAdmin()
    {
        // Arrange
        var adminId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(Guid.NewGuid());
        var talhao = CriarTalhaoValido(propriedade);
        var culturas = new List<Cultura>
        {
            CriarCulturaValida(talhao),
            CriarCulturaValida(talhao)
        };

        _culturaRepositoryMock
            .Setup(x => x.ObterAtivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(culturas);

        // Act
        var resultado = await _service.ObterAtivasAsync(adminId, true);

        // Assert
        resultado.Should().HaveCount(2);
    }

    [Fact]
    public async Task ObterAtivasAsync_DeveRetornarApenasCulturasDoUsuario_QuandoNaoAdmin()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var culturaUsuario = CriarCulturaValida(talhao);

        var outraPropriedade = CriarPropriedadeValida(Guid.NewGuid());
        var outroTalhao = CriarTalhaoValido(outraPropriedade);
        var culturaOutroUsuario = CriarCulturaValida(outroTalhao);

        var culturas = new List<Cultura> { culturaUsuario, culturaOutroUsuario };

        _culturaRepositoryMock
            .Setup(x => x.ObterAtivasAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(culturas);

        // Act
        var resultado = await _service.ObterAtivasAsync(usuarioId, false);

        // Assert
        resultado.Should().HaveCount(1);
        resultado.First().Id.Should().Be(culturaUsuario.Id);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarCulturaComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var cultura = CriarCulturaValida(talhao);
        var dto = new AtualizarCulturaDto(
            TipoCultura.Milho,
            "Pioneer 30F53",
            7m,
            DateTime.UtcNow.AddDays(-20),
            DateTime.UtcNow.AddDays(100),
            20m,
            "Nova observação");

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        _culturaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.AtualizarAsync(cultura.Id, dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.Tipo.Should().Be(dto.Tipo);
        resultado.Variedade.Should().Be(dto.Variedade);
        resultado.AreaPlantada.Should().Be(dto.AreaPlantada);
        _culturaRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarColheitaAsync_DeveRegistrarColheitaComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var cultura = CriarCulturaValida(talhao);
        var dto = new RegistrarColheitaDto(
            DateTime.UtcNow,
            18m,
            "Colheita realizada com sucesso");

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        _culturaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.RegistrarColheitaAsync(cultura.Id, dto, usuarioId, false);

        // Assert
        resultado.Should().NotBeNull();
        resultado.DataColheitaRealizada.Should().Be(dto.DataColheita);
        resultado.ProducaoReal.Should().Be(dto.ProducaoReal);
        resultado.Status.Should().Be(StatusCultura.Colhida);
        _culturaRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CancelarAsync_DeveCancelarCulturaComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var cultura = CriarCulturaValida(talhao);
        var motivo = "Praga severa";

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        _culturaRepositoryMock
            .Setup(x => x.AtualizarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.CancelarAsync(cultura.Id, motivo, usuarioId, false);

        // Assert
        cultura.Status.Should().Be(StatusCultura.Cancelada);
        cultura.Observacoes.Should().Contain(motivo);
        _culturaRepositoryMock.Verify(x => x.AtualizarAsync(It.IsAny<Cultura>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveRemoverCulturaComSucesso()
    {
        // Arrange
        var usuarioId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(usuarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var cultura = CriarCulturaValida(talhao);

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        _culturaRepositoryMock
            .Setup(x => x.RemoverAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _service.RemoverAsync(cultura.Id, usuarioId, false);

        // Assert
        _culturaRepositoryMock.Verify(x => x.RemoverAsync(cultura.Id, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoverAsync_DeveLancarExcecao_QuandoUsuarioSemPermissao()
    {
        // Arrange
        var proprietarioId = Guid.NewGuid();
        var usuarioSemPermissaoId = Guid.NewGuid();
        var propriedade = CriarPropriedadeValida(proprietarioId);
        var talhao = CriarTalhaoValido(propriedade);
        var cultura = CriarCulturaValida(talhao);

        _culturaRepositoryMock
            .Setup(x => x.ObterPorIdAsync(cultura.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cultura);

        // Act
        var act = async () => await _service.RemoverAsync(cultura.Id, usuarioSemPermissaoId, false);

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
            "SP");
    }

    private static Talhao CriarTalhaoValido(Propriedade propriedade)
    {
        var talhao = new Talhao(propriedade.Id, "Talhão A1", 10m, "Descrição teste");
        
        var propriedadeProperty = typeof(Talhao).GetProperty("Propriedade");
        propriedadeProperty?.SetValue(talhao, propriedade);
        
        return talhao;
    }

    private static Cultura CriarCulturaValida(Talhao talhao)
    {
        var cultura = new Cultura(
            talhao.Id,
            TipoCultura.Soja,
            "Monsoy 6410",
            5m,
            DateTime.UtcNow.AddDays(-30),
            DateTime.UtcNow.AddDays(90),
            15m,
            "Cultura teste");
        
        var talhaoProperty = typeof(Cultura).GetProperty("Talhao");
        talhaoProperty?.SetValue(cultura, talhao);
        
        return cultura;
    }
}
