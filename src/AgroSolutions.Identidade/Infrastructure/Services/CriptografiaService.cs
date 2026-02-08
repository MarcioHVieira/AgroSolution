using System.Security.Cryptography;
using System.Text;
using AgroSolutions.Identidade.Application.Interfaces;
using Konscious.Security.Cryptography;

namespace AgroSolutions.Identidade.Infrastructure.Services;

/// <summary>
/// Serviço de criptografia usando Argon2id no formato PHC String padrão
/// Suporta formato legado para migração transparente
/// </summary>
public class CriptografiaService : ICriptografiaService
{
    private const int SaltSize = 16; // 128 bits
    private const int HashSize = 32; // 256 bits
    private const int Iterations = 4;
    private const int MemorySize = 65536; // 64 MB
    private const int DegreeOfParallelism = 2;
    private const int Version = 19; // Versão do Argon2

    public string GerarHash(string senha)
    {
        // Gera um salt aleatório
        var salt = RandomNumberGenerator.GetBytes(SaltSize);

        // Gera o hash usando Argon2id
        var hash = GerarHashArgon2(senha, salt);

        // Retorna no formato PHC String padrão do Argon2id
        var saltBase64 = Convert.ToBase64String(salt).TrimEnd('='); // Remove padding
        var hashBase64 = Convert.ToBase64String(hash).TrimEnd('='); // Remove padding

        return $"$argon2id$v={Version}$m={MemorySize},t={Iterations},p={DegreeOfParallelism}${saltBase64}${hashBase64}";
    }

    public bool VerificarSenha(string senha, string hashArmazenado)
    {
        try
        {
            return VerificarSenhaFormatoPHC(senha, hashArmazenado);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Verifica senha no formato PHC String padrão do Argon2id
    /// </summary>
    private bool VerificarSenhaFormatoPHC(string senha, string hashArmazenado)
    {
        // Faz o parse do hash PHC String
        var parts = hashArmazenado.Split('$');
        if (parts.Length != 6)
        {
            return false;
        }

        // parts[0] = "" (vazio antes do primeiro $)
        // parts[1] = "argon2id"
        // parts[2] = "v=19"
        // parts[3] = "m=65536,t=4,p=2"
        // parts[4] = salt em base64
        // parts[5] = hash em base64

        // Extrai o salt (adiciona padding se necessário)
        var saltBase64 = AddBase64Padding(parts[4]);
        var salt = Convert.FromBase64String(saltBase64);

        // Extrai o hash original (adiciona padding se necessário)
        var hashOriginalBase64 = AddBase64Padding(parts[5]);
        var hashOriginal = Convert.FromBase64String(hashOriginalBase64);

        // Gera o hash da senha fornecida usando o mesmo salt
        var hashNovo = GerarHashArgon2(senha, salt);

        // Compara os hashes de forma segura (timing-attack resistant)
        return CryptographicOperations.FixedTimeEquals(hashOriginal, hashNovo);
    }

    /// <summary>
    /// Gera o hash Argon2id dados a senha e o salt
    /// </summary>
    private byte[] GerarHashArgon2(string senha, byte[] salt)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(senha))
        {
            Salt = salt,
            DegreeOfParallelism = DegreeOfParallelism,
            MemorySize = MemorySize,
            Iterations = Iterations
        };

        return argon2.GetBytes(HashSize);
    }

    /// <summary>
    /// Adiciona padding ao Base64 se necessário
    /// </summary>
    private string AddBase64Padding(string base64)
    {
        switch (base64.Length % 4)
        {
            case 2: return base64 + "==";
            case 3: return base64 + "=";
            default: return base64;
        }
    }
}


