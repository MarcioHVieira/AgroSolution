using AgroSolutions.IngestaoDados.Domain.Entities;
using AgroSolutions.IngestaoDados.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.IngestaoDados.Infrastructure.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(IngestaoDbContext context, ILogger logger)
    {
        try
        {
            logger.LogInformation("Verificando se há sensores cadastrados...");

            // Verifica se já existem sensores
            if (await context.Sensores.AnyAsync())
            {
                logger.LogInformation("Sensores já cadastrados. Seed não necessário.");
                return;
            }

            logger.LogInformation("Iniciando seed de sensores de demonstração...");

            var sensores = new List<Sensor>
            {
                // === FAZENDA SANTA RITA (Usuário A - Propriedade A1) ===
                
                new Sensor(
                    propriedadeId: Guid.Parse("2A3B08A4-3FCF-4ED4-B559-B2A82AF6E5C1"),
                    deviceId: "TEMP-FSR-001",
                    nome: "Sensor Temperatura - Talhão Norte",
                    tipo: TipoSensor.Temperatura,
                    intervaloLeituraMinutos: 5,
                    talhaoId: Guid.Parse("54932999-A7E8-4748-B039-FFCB8F6BA42B"),
                    fabricante: "AgroTech",
                    modelo: "AT-TEMP-500",
                    observacoes: "Sensor de temperatura para monitoramento de talhão de soja"
                ),
                
                new Sensor(
                    propriedadeId: Guid.Parse("2A3B08A4-3FCF-4ED4-B559-B2A82AF6E5C1"),
                    deviceId: "HUMID-FSR-001",
                    nome: "Sensor Umidade Solo - Talhão Norte",
                    tipo: TipoSensor.UmidadeSolo,
                    intervaloLeituraMinutos: 10,
                    talhaoId: Guid.Parse("54932999-A7E8-4748-B039-FFCB8F6BA42B"),
                    fabricante: "AgroTech",
                    modelo: "AT-HUMID-300",
                    observacoes: "Sensor de umidade do solo para irrigação inteligente"
                ),
                
                new Sensor(
                    propriedadeId: Guid.Parse("2A3B08A4-3FCF-4ED4-B559-B2A82AF6E5C1"),
                    deviceId: "PLUV-FSR-001",
                    nome: "Pluviômetro - Fazenda Santa Rita",
                    tipo: TipoSensor.Precipitacao,
                    intervaloLeituraMinutos: 15,
                    talhaoId: null,
                    fabricante: "WeatherPro",
                    modelo: "WP-PLUV-100",
                    observacoes: "Pluviômetro central da propriedade"
                ),

                // === FAZENDA VALE VERDE (Usuário B - Propriedade B1) ===
                
                new Sensor(
                    propriedadeId: Guid.Parse("5C20A6F1-61B3-441C-BA9D-1E27F429F259"),
                    deviceId: "TEMP-FVV-001",
                    nome: "Sensor Temperatura - Cafezal Alto",
                    tipo: TipoSensor.Temperatura,
                    intervaloLeituraMinutos: 5,
                    talhaoId: Guid.Parse("DE1E2CD6-212B-45A0-B8C1-E4B638B6D37C"),
                    fabricante: "AgroTech",
                    modelo: "AT-TEMP-500",
                    observacoes: "Sensor de temperatura para cafezal de arábica premium"
                ),
                
                new Sensor(
                    propriedadeId: Guid.Parse("5C20A6F1-61B3-441C-BA9D-1E27F429F259"),
                    deviceId: "HUMID-FVV-001",
                    nome: "Sensor Umidade Solo - Cafezal Alto",
                    tipo: TipoSensor.UmidadeSolo,
                    intervaloLeituraMinutos: 10,
                    talhaoId: Guid.Parse("DE1E2CD6-212B-45A0-B8C1-E4B638B6D37C"),
                    fabricante: "AgroTech",
                    modelo: "AT-HUMID-300",
                    observacoes: "Sensor de umidade do solo para cafezal"
                ),
                
                new Sensor(
                    propriedadeId: Guid.Parse("5C20A6F1-61B3-441C-BA9D-1E27F429F259"),
                    deviceId: "PH-FVV-001",
                    nome: "Sensor pH Solo - Cafezal Alto",
                    tipo: TipoSensor.PHSolo,
                    intervaloLeituraMinutos: 30,
                    talhaoId: Guid.Parse("DE1E2CD6-212B-45A0-B8C1-E4B638B6D37C"),
                    fabricante: "SoilLab",
                    modelo: "SL-PH-200",
                    observacoes: "Sensor de pH para monitoramento da acidez do solo"
                )
            };

            await context.Sensores.AddRangeAsync(sensores);
            await context.SaveChangesAsync();

            logger.LogInformation("? Seed concluído com sucesso! {Count} sensores cadastrados.", sensores.Count);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "? Erro ao executar seed de sensores");
            throw;
        }
    }
}

