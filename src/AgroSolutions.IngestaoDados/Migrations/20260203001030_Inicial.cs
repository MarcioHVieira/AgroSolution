using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroSolutions.IngestaoDados.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sensores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropriedadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalhaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    DeviceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Fabricante = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Modelo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Latitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Longitude = table.Column<decimal>(type: "decimal(10,7)", precision: 10, scale: 7, nullable: true),
                    Altitude = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    IntervaloLeituraMinutos = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    UltimaLeitura = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UltimaCalibracao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataCadastro = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sensores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leituras",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SensorId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Valor = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Unidade = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TimestampLeitura = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimestampRecebimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Qualidade = table.Column<int>(type: "int", nullable: false),
                    NivelBateria = table.Column<int>(type: "int", nullable: true),
                    IntensidadeSinal = table.Column<int>(type: "int", nullable: true),
                    DadosAdicionais = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Observacoes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leituras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leituras_Sensores_SensorId",
                        column: x => x.SensorId,
                        principalTable: "Sensores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_Qualidade",
                table: "Leituras",
                column: "Qualidade");

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_SensorId",
                table: "Leituras",
                column: "SensorId");

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_SensorId_TimestampLeitura",
                table: "Leituras",
                columns: new[] { "SensorId", "TimestampLeitura" });

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_TimestampLeitura",
                table: "Leituras",
                column: "TimestampLeitura");

            migrationBuilder.CreateIndex(
                name: "IX_Sensores_DeviceId",
                table: "Sensores",
                column: "DeviceId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Sensores_PropriedadeId",
                table: "Sensores",
                column: "PropriedadeId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensores_Status",
                table: "Sensores",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Sensores_TalhaoId",
                table: "Sensores",
                column: "TalhaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Sensores_Tipo",
                table: "Sensores",
                column: "Tipo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leituras");

            migrationBuilder.DropTable(
                name: "Sensores");
        }
    }
}
