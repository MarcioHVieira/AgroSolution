using System.Security.Cryptography;

namespace AgroSolutions.Identidade.Infrastructure.Security;

/// <summary>
/// Gerenciador de chaves RSA para assinatura de tokens JWT
/// </summary>
public class RsaKeyManager
{
    private const string PrivateKeyFileName = "rsa_private_key.xml";
    private const string PublicKeyFileName = "rsa_public_key.xml";
    private readonly string _keysDirectory;
    private RSA? _rsa;

    public RsaKeyManager(IConfiguration configuration)
    {
        // Diretório de armazenamento das chaves
        _keysDirectory = configuration["RsaKeys:Directory"] 
            ?? Path.Combine(AppContext.BaseDirectory, "keys");
        
        EnsureKeysDirectoryExists();
        InitializeKeys();
    }

    private void EnsureKeysDirectoryExists()
    {
        if (!Directory.Exists(_keysDirectory))
        {
            Directory.CreateDirectory(_keysDirectory);
        }
    }

    private void InitializeKeys()
    {
        var privateKeyPath = Path.Combine(_keysDirectory, PrivateKeyFileName);
        var publicKeyPath = Path.Combine(_keysDirectory, PublicKeyFileName);

        if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath))
        {
            GenerateAndSaveKeys();
        }
        else
        {
            LoadKeys();
        }
    }

    /// <summary>
    /// Gera um novo par de chaves RSA e salva em arquivos XML
    /// </summary>
    public void GenerateAndSaveKeys()
    {
        _rsa = RSA.Create(2048);

        var privateKeyPath = Path.Combine(_keysDirectory, PrivateKeyFileName);
        var publicKeyPath = Path.Combine(_keysDirectory, PublicKeyFileName);

        // Salva chave privada (incluindo parâmetros públicos e privados)
        var privateKeyXml = _rsa.ToXmlString(includePrivateParameters: true);
        File.WriteAllText(privateKeyPath, privateKeyXml);

        // Salva chave pública (apenas parâmetros públicos)
        var publicKeyXml = _rsa.ToXmlString(includePrivateParameters: false);
        File.WriteAllText(publicKeyPath, publicKeyXml);

        // Define permissões restritas no arquivo da chave privada
        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(privateKeyPath, 
                UnixFileMode.UserRead | UnixFileMode.UserWrite);
        }
    }

    /// <summary>
    /// Carrega as chaves RSA dos arquivos XML
    /// </summary>
    private void LoadKeys()
    {
        var privateKeyPath = Path.Combine(_keysDirectory, PrivateKeyFileName);
        
        if (!File.Exists(privateKeyPath))
        {
            throw new FileNotFoundException("Chave privada RSA não encontrada.", privateKeyPath);
        }

        _rsa = RSA.Create();
        var privateKeyXml = File.ReadAllText(privateKeyPath);
        _rsa.FromXmlString(privateKeyXml);
    }

    /// <summary>
    /// Obtém a instância RSA com chave privada (para assinar tokens)
    /// </summary>
    public RSA GetRsa()
    {
        if (_rsa == null)
        {
            throw new InvalidOperationException("RSA não foi inicializado.");
        }
        return _rsa;
    }

    /// <summary>
    /// Obtém a chave pública em formato XML
    /// </summary>
    public string GetPublicKeyXml()
    {
        if (_rsa == null)
        {
            throw new InvalidOperationException("RSA não foi inicializado.");
        }
        return _rsa.ToXmlString(includePrivateParameters: false);
    }

    /// <summary>
    /// Obtém a chave pública em formato PEM (compatível com OpenSSL/outros sistemas)
    /// </summary>
    public string GetPublicKeyPem()
    {
        if (_rsa == null)
        {
            throw new InvalidOperationException("RSA não foi inicializado.");
        }

        var publicKeyBytes = _rsa.ExportSubjectPublicKeyInfo();
        return "-----BEGIN PUBLIC KEY-----\n" +
               Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks) +
               "\n-----END PUBLIC KEY-----";
    }

    /// <summary>
    /// Obtém os parâmetros RSA para uso com Microsoft.IdentityModel.Tokens
    /// </summary>
    public RSAParameters GetRsaParameters()
    {
        if (_rsa == null)
        {
            throw new InvalidOperationException("RSA não foi inicializado.");
        }
        return _rsa.ExportParameters(includePrivateParameters: true);
    }

    /// <summary>
    /// Obtém a chave pública em formato JWK (JSON Web Key) para JWKS endpoint
    /// </summary>
    public Dictionary<string, string> GetPublicKeyJwk()
    {
        if (_rsa == null)
        {
            throw new InvalidOperationException("RSA não foi inicializado.");
        }

        var parameters = _rsa.ExportParameters(includePrivateParameters: false);
        
        // Gera um ID único e estável para a chave baseado no modulus
        var keyId = Convert.ToBase64String(
            System.Security.Cryptography.SHA256.HashData(parameters.Modulus!)
        ).Substring(0, 16);

        return new Dictionary<string, string>
        {
            { "kty", "RSA" },
            { "use", "sig" },
            { "alg", "RS256" },
            { "kid", keyId },
            { "n", Base64UrlEncode(parameters.Modulus!) },
            { "e", Base64UrlEncode(parameters.Exponent!) }
        };
    }

    private static string Base64UrlEncode(byte[] input)
    {
        return Convert.ToBase64String(input)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
