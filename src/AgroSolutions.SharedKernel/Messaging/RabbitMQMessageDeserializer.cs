using System.Text.Json;

namespace AgroSolutions.SharedKernel.Messaging;

/// <summary>
/// Helper para desserialização de mensagens RabbitMQ usando System.Text.Json
/// Centraliza as opções de serialização para garantir consistência
/// </summary>
public static class RabbitMQMessageDeserializer
{
    private static readonly JsonSerializerOptions DefaultOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        // NÃO usa JsonStringEnumConverter - deserializa enums como números (formato padrão)
    };

    /// <summary>
    /// Desserializa uma mensagem RabbitMQ para o tipo especificado
    /// </summary>
    /// <typeparam name="T">Tipo do evento/mensagem</typeparam>
    /// <param name="body">Corpo da mensagem em bytes</param>
    /// <returns>Objeto desserializado</returns>
    /// <exception cref="JsonException">Quando a desserialização falha</exception>
    public static T Deserialize<T>(byte[] body)
    {
        return JsonSerializer.Deserialize<T>(body, DefaultOptions)
            ?? throw new JsonException($"Falha ao desserializar mensagem para o tipo {typeof(T).Name}");
    }

    /// <summary>
    /// Desserializa uma mensagem RabbitMQ para o tipo especificado (async)
    /// </summary>
    /// <typeparam name="T">Tipo do evento/mensagem</typeparam>
    /// <param name="stream">Stream da mensagem</param>
    /// <param name="cancellationToken">Token de cancelamento</param>
    /// <returns>Objeto desserializado</returns>
    /// <exception cref="JsonException">Quando a desserialização falha</exception>
    public static async Task<T> DeserializeAsync<T>(Stream stream, CancellationToken cancellationToken = default)
    {
        return await JsonSerializer.DeserializeAsync<T>(stream, DefaultOptions, cancellationToken)
            ?? throw new JsonException($"Falha ao desserializar mensagem para o tipo {typeof(T).Name}");
    }

    /// <summary>
    /// Tenta desserializar uma mensagem RabbitMQ
    /// </summary>
    /// <typeparam name="T">Tipo do evento/mensagem</typeparam>
    /// <param name="body">Corpo da mensagem em bytes</param>
    /// <param name="result">Objeto desserializado (se bem-sucedido)</param>
    /// <returns>True se a desserialização foi bem-sucedida</returns>
    public static bool TryDeserialize<T>(byte[] body, out T? result)
    {
        try
        {
            result = JsonSerializer.Deserialize<T>(body, DefaultOptions);
            return result != null;
        }
        catch
        {
            result = default;
            return false;
        }
    }
}
