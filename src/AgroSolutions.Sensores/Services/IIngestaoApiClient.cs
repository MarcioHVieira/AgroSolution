using AgroSolutions.Sensores.Models;

namespace AgroSolutions.Sensores.Services;

public interface IIngestaoApiClient
{
    Task<bool> EnviarLeituraAsync(LeituraSimuladaDto leitura);
    Task<string> ObterTokenAsync();
}
