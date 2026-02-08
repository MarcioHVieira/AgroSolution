using System.Text.Json;
using System.Text.Json.Serialization;

namespace AgroSolutions.SharedKernel.Messaging;

/// <summary>
/// Conversor flexível de enums que aceita tanto strings quanto números
/// Isso garante compatibilidade com mensagens antigas que usam números
/// </summary>
public class FlexibleEnumConverter<TEnum> : JsonConverter<TEnum> where TEnum : struct, Enum
{
    public override TEnum Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Se é uma string, tenta converter
        if (reader.TokenType == JsonTokenType.String)
        {
            var stringValue = reader.GetString();
            if (Enum.TryParse<TEnum>(stringValue, ignoreCase: true, out var enumValue))
            {
                return enumValue;
            }
            throw new JsonException($"Unable to convert \"{stringValue}\" to enum {typeof(TEnum).Name}");
        }
        
        // Se é um número, converte diretamente
        if (reader.TokenType == JsonTokenType.Number)
        {
            var numberValue = reader.GetInt32();
            return (TEnum)Enum.ToObject(typeof(TEnum), numberValue);
        }
        
        throw new JsonException($"Unable to convert {reader.TokenType} to enum {typeof(TEnum).Name}");
    }

    public override void Write(Utf8JsonWriter writer, TEnum value, JsonSerializerOptions options)
    {
        // Sempre serializa como string
        writer.WriteStringValue(value.ToString());
    }
}
