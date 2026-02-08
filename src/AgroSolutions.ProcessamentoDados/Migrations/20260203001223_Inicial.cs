using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroSolutions.ProcessamentoDados.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AgregacoesDados",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropriedadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalhaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoSensor = table.Column<int>(type: "int", nullable: false),
                    TipoAgregacao = table.Column<int>(type: "int", nullable: false),
                    PeriodoInicio = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodoFim = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalLeituras = table.Column<int>(type: "int", nullable: false),
                    ValorMinimo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValorMaximo = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    ValorMedio = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    DesvioPadrao = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LeiturasNormais = table.Column<int>(type: "int", nullable: false),
                    LeiturasSuspeitas = table.Column<int>(type: "int", nullable: false),
                    LeiturasInvalidas = table.Column<int>(type: "int", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgregacoesDados", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeiturasProcessadas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LeituraOrigemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PropriedadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalhaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TipoSensor = table.Column<int>(type: "int", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimestampLeitura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampRecebimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampProcessamento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qualidade = table.Column<int>(type: "int", nullable: false),
                    NivelBateria = table.Column<int>(type: "int", nullable: true),
                    IntensidadeSinal = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    DadosAdicionais = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MensagemErro = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeiturasProcessadas", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_DeviceId",
                table: "AgregacoesDados",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_PeriodoInicio",
                table: "AgregacoesDados",
                column: "PeriodoInicio");

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_PropriedadeId",
                table: "AgregacoesDados",
                column: "PropriedadeId");

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_SensorId",
                table: "AgregacoesDados",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_SensorId_TipoAgregacao_PeriodoInicio",
                table: "AgregacoesDados",
                columns: new[] { "SensorId", "TipoAgregacao", "PeriodoInicio" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_TalhaoId",
                table: "AgregacoesDados",
                column: "TalhaoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgregacoesDados_TipoAgregacao",
                table: "AgregacoesDados",
                column: "TipoAgregacao");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_DeviceId",
                table: "LeiturasProcessadas",
                column: "DeviceId");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_LeituraOrigemId",
                table: "LeiturasProcessadas",
                column: "LeituraOrigemId");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_PropriedadeId",
                table: "LeiturasProcessadas",
                column: "PropriedadeId");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_SensorId",
                table: "LeiturasProcessadas",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_SensorId_TimestampLeitura",
                table: "LeiturasProcessadas",
                columns: new[] { "SensorId", "TimestampLeitura" });

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_Status",
                table: "LeiturasProcessadas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_TalhaoId",
                table: "LeiturasProcessadas",
                column: "TalhaoId");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasProcessadas_TimestampLeitura",
                table: "LeiturasProcessadas",
                column: "TimestampLeitura");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgregacoesDados");

            migrationBuilder.DropTable(
                name: "LeiturasProcessadas");
        }
    }
}
