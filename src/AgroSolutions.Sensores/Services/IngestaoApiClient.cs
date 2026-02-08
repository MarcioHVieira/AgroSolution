using AgroSolutions.Sensores.Configuration;
using AgroSolutions.Sensores.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace AgroSolutions.Sensores.Services;

public class IngestaoApiClient : IIngestaoApiClient
{
    private readonly HttpClient _httpClient;
    private readonly SimuladorSettings _settings;
    private readonly ILogger<IngestaoApiClient> _logger;
    private string? _token;
    private DateTime _tokenExpiration;

    public IngestaoApiClient(
        HttpClient httpClient,
        IOptions<SimuladorSettings> settings,
        ILogger<IngestaoApiClient> logger)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
        _logger = logger;
        _tokenExpiration = DateTime.UtcNow;
    }

    public async Task<bool> EnviarLeituraAsync(LeituraSimuladaDto leitura)
    {
        try
        {
            // Garantir que temos um token válido
            await EnsureValidTokenAsync();

            // Mapear para o DTO esperado pela API de Ingestão
            var dtoIngestao = new
            {
                deviceId = $"SIM-{leitura.TalhaoId}-{leitura.TipoSensor}", // Gerar deviceId único
                valor = leitura.Valor,
                unidade = leitura.TipoSensor switch
                {
                    "UmidadeSolo" => "%",
                    "Temperatura" => "°C",
                    "Precipitacao" => "mm",
                    "Luminosidade" => "lux",
                    "PhSolo" => "pH",
                    _ => "unidade"
                },
                timestampLeitura = leitura.DataHora
            };

            var json = JsonSerializer.Serialize(dtoIngestao, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            
            var url = $"{_settings.IngestaoApi.BaseUrl}{_settings.IngestaoApi.LeiturasEndpoint}";
            
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            var response = await _httpClient.PostAsync(url, content);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogDebug("Leitura enviada com sucesso: {TipoSensor} = {Valor} - TalhaoId: {TalhaoId}", 
                    leitura.TipoSensor, leitura.Valor, leitura.TalhaoId);
                return true;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao enviar leitura. Status: {StatusCode} - Erro: {Error}", 
                    response.StatusCode, errorContent);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao enviar leitura para API de ingestão");
            return false;
        }
    }

    public async Task<string> ObterTokenAsync()
    {
        try
        {
            var loginDto = new
            {
                email = _settings.Autenticacao.Email,
                senha = _settings.Autenticacao.Senha
            };

            var json = JsonSerializer.Serialize(loginDto, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var url = $"{_settings.Autenticacao.IdentidadeUrl}/api/autenticacao/login";

            var response = await _httpClient.PostAsync(url, content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseContent);
                
                // API retorna: { "dados": { "accessToken": "..." } }
                var token = doc.RootElement
                    .GetProperty("dados")
                    .GetProperty("accessToken")
                    .GetString();
                    
                _logger.LogInformation("Token obtido com sucesso");
                
                return token ?? throw new InvalidOperationException("Token não encontrado na resposta");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("Falha ao obter token. Status: {StatusCode} - Erro: {Error}", 
                    response.StatusCode, errorContent);
                throw new InvalidOperationException($"Falha ao autenticar: {response.StatusCode}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter token de autenticação");
            throw;
        }
    }

    private async Task EnsureValidTokenAsync()
    {
        // Se não temos token ou está prestes a expirar (com margem de 5 minutos)
        if (string.IsNullOrEmpty(_token) || DateTime.UtcNow.AddMinutes(5) >= _tokenExpiration)
        {
            _token = await ObterTokenAsync();
            _tokenExpiration = DateTime.UtcNow.AddMinutes(55); // Token geralmente expira em 60 minutos
        }
    }
}
