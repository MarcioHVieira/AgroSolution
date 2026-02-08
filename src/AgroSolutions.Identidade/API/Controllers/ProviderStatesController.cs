using AgroSolutions.Identidade.Domain.Entities;
using AgroSolutions.Identidade.Domain.Enums;
using AgroSolutions.Identidade.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace AgroSolutions.Identidade.API.Controllers;

/// <summary>
/// Controller para configurar estados do provedor nos testes de contrato
/// </summary>
[ApiController]
[Route("provider-states")]
public class ProviderStatesController : ControllerBase
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ProviderStatesController> _logger;

    public ProviderStatesController(
        IServiceProvider serviceProvider,
        ILogger<ProviderStatesController> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> ConfigurarEstado([FromBody] ProviderStateRequest request)
    {
        _logger.LogInformation("Configurando provider state: {State}", request.State);

        using var scope = _serviceProvider.CreateScope();

        try
        {
            switch (request.State)
            {
                case "Usuário existe":
                    await ConfigurarUsuarioExistente(scope, request.Params);
                    break;

                case "Usuário não existe":
                    // Não precisa fazer nada, usuário já não existe
                    break;

                case "Token é válido":
                    await ConfigurarTokenValido(scope, request.Params);
                    break;

                default:
                    _logger.LogWarning("Estado não reconhecido: {State}", request.State);
                    return BadRequest($"Estado não reconhecido: {request.State}");
            }

            return Ok();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao configurar estado: {Message}", ex.Message);
            return StatusCode(500, ex.Message);
        }
    }

    private async Task ConfigurarUsuarioExistente(IServiceScope scope, Dictionary<string, object>? parameters)
    {
        // Criar usuário de teste no banco em memória
        var context = scope.ServiceProvider.GetRequiredService<IdentidadeDbContext>();

        var usuario = new Usuario(
            "Marcio Henrique",
            "marcio@teste.com",
            "hash_senha_teste",
            PerfilAcesso.Usuario,
            "(79) 98765-4321"
        );

        usuario.AtivarConta();

        await context.Usuarios.AddAsync(usuario);
        await context.SaveChangesAsync();

        _logger.LogInformation("Usuário de teste criado: {Email}", usuario.Email);
    }

    private Task ConfigurarTokenValido(IServiceScope scope, Dictionary<string, object>? parameters)
    {
        // Mock de token válido (configuração adicional se necessário)
        _logger.LogInformation("Token válido configurado");
        return Task.CompletedTask;
    }
}

public record ProviderStateRequest
{
    public string State { get; init; } = string.Empty;
    public Dictionary<string, object>? Params { get; init; }
}
