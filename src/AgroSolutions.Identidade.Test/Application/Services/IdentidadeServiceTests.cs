using AgroSolutions.Identidade.Application.DTOs;
using AgroSolutions.Identidade.Application.Interfaces;
using AgroSolutions.Identidade.Application.Services;
using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Enums;
using AgroSolutions.Identidade.Domain.Interfaces;
using AgroSolutions.SharedKernel.Messaging;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace AgroSolutions.Identidade.Test.Application.Services;

public class IdentidadeServiceTests
{
    private readonly Mock<IUsuarioRepository> _usuarioRepositorioMock;
    private readonly Mock<ICodigoValidacaoRepository> _codigoValidacaoRepositorioMock;
    private readonly Mock<IRefreshTokenRepository> _refreshTokenRepositorioMock;
    private readonly Mock<ICriptografiaService> _criptografiaServicoMock;
    private readonly Mock<IEmailService> _emailServicoMock;
    private readonly Mock<ITokenService> _tokenServicoMock;
    private readonly Mock<ILogger<IdentidadeService>> _loggerMock;
    private readonly Mock<IRabbitMQPublisher> _publisherMock;
    private readonly IdentidadeService _identidadeService;

    public IdentidadeServiceTests()
    {
        _usuarioRepositorioMock = new Mock<IUsuarioRepository>();
        _codigoValidacaoRepositorioMock = new Mock<ICodigoValidacaoRepository>();
        _refreshTokenRepositorioMock = new Mock<IRefreshTokenRepository>();
        _criptografiaServicoMock = new Mock<ICriptografiaService>();
        _emailServicoMock = new Mock<IEmailService>();
        _tokenServicoMock = new Mock<ITokenService>();
        _loggerMock = new Mock<ILogger<IdentidadeService>>();
        _publisherMock = new Mock<IRabbitMQPublisher>();

        _identidadeService = new IdentidadeService(
            _usuarioRepositorioMock.Object,
            _codigoValidacaoRepositorioMock.Object,
            _refreshTokenRepositorioMock.Object,
            _criptografiaServicoMock.Object,
            _emailServicoMock.Object,
            _tokenServicoMock.Object,
            _loggerMock.Object,
            _publisherMock.Object
        );
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DeveRegistrarUsuarioComSucesso()
    {
        // Arrange
        var dto = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", "11999999999", "12345678901");
        
        _usuarioRepositorioMock.Setup(x => x.ExisteEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _usuarioRepositorioMock.Setup(x => x.ExisteCpfAsync(dto.Cpf!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _criptografiaServicoMock.Setup(x => x.GerarHash(dto.Senha))
            .Returns("hashedPassword");
        _usuarioRepositorioMock.Setup(x => x.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _codigoValidacaoRepositorioMock.Setup(x => x.AdicionarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _emailServicoMock.Setup(x => x.EnviarEmailValidacaoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _identidadeService.RegistrarUsuarioAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.UsuarioId.Should().NotBeEmpty();
        _usuarioRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()), Times.Once);
        _codigoValidacaoRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServicoMock.Verify(x => x.EnviarEmailValidacaoAsync(dto.Email, dto.NomeCompleto, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DeveLancarExcecaoQuandoEmailJaExiste()
    {
        // Arrange
        var dto = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", null, null);
        _usuarioRepositorioMock.Setup(x => x.ExisteEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _identidadeService.RegistrarUsuarioAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*e-mail*cadastrado*");
    }

    [Fact]
    public async Task RegistrarUsuarioAsync_DeveLancarExcecaoQuandoCpfJaExiste()
    {
        // Arrange
        var dto = new RegistrarUsuarioDto("Marcio Henrique", "marcio@agrosolutions.com.br", "Senha@123", null, "12345678901");
        _usuarioRepositorioMock.Setup(x => x.ExisteEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _usuarioRepositorioMock.Setup(x => x.ExisteCpfAsync(dto.Cpf!, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var act = async () => await _identidadeService.RegistrarUsuarioAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*CPF*cadastrado*");
    }

    [Fact]
    public async Task ValidarCodigoAsync_DeveValidarCodigoComSucesso()
    {
        // Arrange
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", "123456");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "hash", PerfilAcesso.Usuario);
        var codigoValidacao = new CodigoValidacao(usuario.Id, dto.Codigo, 30);

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _codigoValidacaoRepositorioMock.Setup(x => x.ObterPorCodigoAsync(dto.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(codigoValidacao);
        _usuarioRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _codigoValidacaoRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _identidadeService.ValidarCodigoAsync(dto);

        // Assert
        usuario.Status.Should().Be(StatusUsuario.Ativo);
        codigoValidacao.Utilizado.Should().BeTrue();
        _usuarioRepositorioMock.Verify(x => x.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
        _codigoValidacaoRepositorioMock.Verify(x => x.AtualizarAsync(codigoValidacao, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ValidarCodigoAsync_DeveLancarExcecaoQuandoUsuarioNaoEncontrado()
    {
        // Arrange
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", "123456");
        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var act = async () => await _identidadeService.ValidarCodigoAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*não encontrado*");
    }

    [Fact]
    public async Task ValidarCodigoAsync_DeveLancarExcecaoQuandoContaJaAtiva()
    {
        // Arrange
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", "123456");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "hash", PerfilAcesso.Usuario);
        usuario.AtivarConta();

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);

        // Act
        var act = async () => await _identidadeService.ValidarCodigoAsync(dto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*já está ativa*");
    }

    [Fact]
    public async Task ValidarCodigoAsync_DeveLancarExcecaoQuandoCodigoInvalido()
    {
        // Arrange
        var dto = new ValidarCodigoDto("marcio@agrosolutions.com.br", "123456");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "hash", PerfilAcesso.Usuario);

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _codigoValidacaoRepositorioMock.Setup(x => x.ObterPorCodigoAsync(dto.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync((CodigoValidacao?)null);

        // Act
        var act = async () => await _identidadeService.ValidarCodigoAsync(dto);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>()
            .WithMessage("*inválido*");
    }

    [Fact]
    public async Task LoginAsync_DeveFazerLoginComSucesso()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "$argon2id$hash", PerfilAcesso.Usuario);
        usuario.AtivarConta();

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _criptografiaServicoMock.Setup(x => x.VerificarSenha(dto.Senha, usuario.SenhaHash))
            .Returns(true);
        _tokenServicoMock.Setup(x => x.GerarToken(usuario.Id, usuario.Email, usuario.Perfil.ToString()))
            .Returns("jwt-token");
        _tokenServicoMock.Setup(x => x.GerarRefreshToken())
            .Returns("refresh-token");
        _usuarioRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositorioMock.Setup(x => x.RevogarTodosDoUsuarioAsync(usuario.Id, It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositorioMock.Setup(x => x.AdicionarAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _identidadeService.LoginAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.AccessToken.Should().Be("jwt-token");
        resultado.RefreshToken.Should().Be("refresh-token");
        resultado.TokenType.Should().Be("Bearer");
        resultado.Usuario.Email.Should().Be(usuario.Email);
        _refreshTokenRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecaoQuandoUsuarioNaoEncontrado()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");
        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Usuario?)null);

        // Act
        var act = async () => await _identidadeService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inválidas*");
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecaoQuandoSenhaIncorreta()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "$argon2id$hash", PerfilAcesso.Usuario);
        usuario.AtivarConta();

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _criptografiaServicoMock.Setup(x => x.VerificarSenha(dto.Senha, usuario.SenhaHash))
            .Returns(false);

        // Act
        var act = async () => await _identidadeService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inválidas*");
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecaoQuandoContaNaoValidada()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "$argon2id$hash", PerfilAcesso.Usuario);

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _criptografiaServicoMock.Setup(x => x.VerificarSenha(dto.Senha, usuario.SenhaHash))
            .Returns(true);

        // Act
        var act = async () => await _identidadeService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*não foi validada*");
    }

    [Fact]
    public async Task LoginAsync_DeveLancarExcecaoQuandoContaBloqueada()
    {
        // Arrange
        var dto = new LoginDto("marcio@agrosolutions.com.br", "Senha@123");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "$argon2id$hash", PerfilAcesso.Usuario);
        usuario.AtivarConta();
        usuario.Bloquear();

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _criptografiaServicoMock.Setup(x => x.VerificarSenha(dto.Senha, usuario.SenhaHash))
            .Returns(true);

        // Act
        var act = async () => await _identidadeService.LoginAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*bloqueada*");
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveRenovarTokenComSucesso()
    {
        // Arrange
        var dto = new RefreshTokenDto("refresh-token");
        var usuario = new Usuario("Marcio Henrique", "marcio@agrosolutions.com.br", "hash", PerfilAcesso.Usuario);
        usuario.AtivarConta();
        var refreshToken = new RefreshToken(usuario.Id, dto.RefreshToken, DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositorioMock.Setup(x => x.ObterPorTokenAsync(dto.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _usuarioRepositorioMock.Setup(x => x.ObterPorIdAsync(usuario.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _tokenServicoMock.Setup(x => x.GerarToken(usuario.Id, usuario.Email, usuario.Perfil.ToString()))
            .Returns("new-jwt-token");
        _tokenServicoMock.Setup(x => x.GerarRefreshToken())
            .Returns("new-refresh-token");
        _refreshTokenRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _refreshTokenRepositorioMock.Setup(x => x.AdicionarAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _identidadeService.RefreshTokenAsync(dto);

        // Assert
        resultado.Should().NotBeNull();
        resultado.AccessToken.Should().Be("new-jwt-token");
        resultado.RefreshToken.Should().Be("new-refresh-token");
        refreshToken.Revogado.Should().BeTrue();
    }

    [Fact]
    public async Task RefreshTokenAsync_DeveLancarExcecaoQuandoTokenInvalido()
    {
        // Arrange
        var dto = new RefreshTokenDto("refresh-token");
        _refreshTokenRepositorioMock.Setup(x => x.ObterPorTokenAsync(dto.RefreshToken, It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken?)null);

        // Act
        var act = async () => await _identidadeService.RefreshTokenAsync(dto);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("*inválido*");
    }

    [Fact]
    public async Task RevogarTokenAsync_DeveRevogarTokenComSucesso()
    {
        // Arrange
        var tokenString = "refresh-token";
        var refreshToken = new RefreshToken(Guid.NewGuid(), tokenString, DateTime.UtcNow.AddDays(7));

        _refreshTokenRepositorioMock.Setup(x => x.ObterPorTokenAsync(tokenString, It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);
        _refreshTokenRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<RefreshToken>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _identidadeService.RevogarTokenAsync(tokenString);

        // Assert
        refreshToken.Revogado.Should().BeTrue();
        _refreshTokenRepositorioMock.Verify(x => x.AtualizarAsync(refreshToken, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReenviarCodigoValidacaoAsync_DeveReenviarCodigoComSucesso()
    {
        // Arrange
        var email = "marcio@agrosolutions.com.br";
        var usuario = new Usuario("Marcio Henrique", email, "hash", PerfilAcesso.Usuario);

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _codigoValidacaoRepositorioMock.Setup(x => x.AdicionarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _emailServicoMock.Setup(x => x.EnviarEmailValidacaoAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _identidadeService.ReenviarCodigoValidacaoAsync(email);

        // Assert
        _codigoValidacaoRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServicoMock.Verify(x => x.EnviarEmailValidacaoAsync(email, usuario.NomeCompleto, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task EsqueciSenhaAsync_DeveEnviarEmailRecuperacao()
    {
        // Arrange
        var dto = new EsqueciSenhaDto("marcio@agrosolutions.com.br");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "hash", PerfilAcesso.Usuario);

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _codigoValidacaoRepositorioMock.Setup(x => x.AdicionarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _emailServicoMock.Setup(x => x.EnviarEmailRecuperacaoSenhaAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _identidadeService.EsqueciSenhaAsync(dto);

        // Assert
        _codigoValidacaoRepositorioMock.Verify(x => x.AdicionarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()), Times.Once);
        _emailServicoMock.Verify(x => x.EnviarEmailRecuperacaoSenhaAsync(dto.Email, usuario.NomeCompleto, It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RedefinirSenhaAsync_DeveRedefinirSenhaComSucesso()
    {
        // Arrange
        var dto = new RedefinirSenhaDto("marcio@agrosolutions.com.br", "123456", "NovaSenha@123");
        var usuario = new Usuario("Marcio Henrique", dto.Email, "hashAntigo", PerfilAcesso.Usuario);
        var codigoValidacao = new CodigoValidacao(usuario.Id, dto.Codigo, 30);

        _usuarioRepositorioMock.Setup(x => x.ObterPorEmailAsync(dto.Email, It.IsAny<CancellationToken>()))
            .ReturnsAsync(usuario);
        _codigoValidacaoRepositorioMock.Setup(x => x.ObterPorCodigoAsync(dto.Codigo, It.IsAny<CancellationToken>()))
            .ReturnsAsync(codigoValidacao);
        _criptografiaServicoMock.Setup(x => x.GerarHash(dto.NovaSenha))
            .Returns("novoHash");
        _usuarioRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<Usuario>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _codigoValidacaoRepositorioMock.Setup(x => x.AtualizarAsync(It.IsAny<CodigoValidacao>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _identidadeService.RedefinirSenhaAsync(dto);

        // Assert
        usuario.SenhaHash.Should().Be("novoHash");
        codigoValidacao.Utilizado.Should().BeTrue();
        _usuarioRepositorioMock.Verify(x => x.AtualizarAsync(usuario, It.IsAny<CancellationToken>()), Times.Once);
    }
}
