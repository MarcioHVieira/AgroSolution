namespace AgroSolutions.Identidade.Application.Interfaces;

/// <summary>
/// Interface para criptografia de senhas com Argon2
/// </summary>
public interface ICriptografiaService
{
    string GerarHash(string senha);
    bool VerificarSenha(string senha, string hash);
}
