namespace AgroSolutions.ProcessamentoDados.Domain.Enums;

/// <summary>
/// Tipo de sensor IoT
/// </summary>
public enum TipoSensor
{
    Temperatura = 1,
    Umidade = 2,
    pH = 3,
    Pluviometro = 4,
    Pressao = 5,
    VelocidadeVento = 6,
    DirecaoVento = 7,
    Luz = 8,
    CO2 = 9,
    Condutividade = 10
}

/// <summary>
/// Status de processamento da leitura
/// </summary>
public enum StatusProcessamento
{
    Recebido = 1,
    Processado = 2,
    Falha = 3,
    Reprocessando = 4
}

/// <summary>
/// Qualidade da leitura
/// </summary>
public enum QualidadeLeitura
{
    Normal = 1,
    Suspeita = 2,
    Invalida = 3
}

/// <summary>
/// Tipo de agregação de dados
/// </summary>
public enum TipoAgregacao
{
    Horaria = 1,
    Diaria = 2,
    Semanal = 3,
    Mensal = 4
}
