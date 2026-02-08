namespace AgroSolutions.Identidade.Application.DTOs;

/// <summary>
/// DTO para registro de novo usuário
/// </summary>
public record RegistrarUsuarioDto(
    string NomeCompleto,
    string Email,
    string Senha,
    string? Telefone,
    string? Cpf
);

/// <summary>
/// DTO para validação de código de e-mail
/// </summary>
public record ValidarCodigoDto(
    string Email,
    string Codigo
);

/// <summary>
/// DTO para login de usuário
/// </summary>
public record LoginDto(
    string Email,
    string Senha
);

/// <summary>
/// DTO para solicitar recuperação de senha
/// </summary>
public record EsqueciSenhaDto(
    string Email
);

/// <summary>
/// DTO para redefinir senha com código
/// </summary>
public record RedefinirSenhaDto(
    string Email,
    string Codigo,
    string NovaSenha
);

/// <summary>
/// DTO para alterar senha (usuário autenticado)
/// </summary>
public record AlterarSenhaDto(
    string SenhaAtual,
    string NovaSenha
);

/// <summary>
/// DTO de resposta com token JWT
/// </summary>
public record TokenResponseDto(
    string AccessToken,
    string RefreshToken,
    string TokenType,
    int ExpiresIn,
    UsuarioDto Usuario
);

/// <summary>
/// DTO para renovar token usando refresh token
/// </summary>
public record RefreshTokenDto(
    string RefreshToken
);

/// <summary>
/// DTO de dados do usuário
/// </summary>
public record UsuarioDto(
    Guid Id,
    string NomeCompleto,
    string Email,
    string? Telefone,
    string? Cpf,
    string Perfil,
    string Status,
    DateTime DataCriacao
);

/// <summary>
/// DTO de resposta de registro
/// </summary>
public record RegistroResponseDto(
    Guid UsuarioId
);

/// <summary>
/// DTO para confirmar senha (usado na exclusão de conta)
/// </summary>
public record ConfirmarSenhaDto(
    string Senha
);


