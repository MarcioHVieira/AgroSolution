using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroSolutions.Analise.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalhaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Severidade = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Titulo = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Recomendacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataGeracao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataVisualizacao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DataResolucao = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ValorReferencia = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    DadosAdicionais = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UsuarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RegrasAlertas",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TipoAlerta = table.Column<int>(type: "int", nullable: false),
                    Severidade = table.Column<int>(type: "int", nullable: false),
                    Ativa = table.Column<bool>(type: "bit", nullable: false),
                    Condicao = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    TemplateMensagem = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Recomendacao = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataAtualizacao = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegrasAlertas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TalhoesInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    PropriedadeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProprietarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailProprietario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeProprietario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataSincronizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TalhoesInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_DataGeracao",
                table: "Alertas",
                column: "DataGeracao");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_Status",
                table: "Alertas",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_TalhaoId",
                table: "Alertas",
                column: "TalhaoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegrasAlertas_Ativa",
                table: "RegrasAlertas",
                column: "Ativa");

            migrationBuilder.CreateIndex(
                name: "IX_RegrasAlertas_TipoAlerta",
                table: "RegrasAlertas",
                column: "TipoAlerta");

            migrationBuilder.CreateIndex(
                name: "IX_TalhoesInfo_ProprietarioId",
                table: "TalhoesInfo",
                column: "ProprietarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "RegrasAlertas");

            migrationBuilder.DropTable(
                name: "TalhoesInfo");
        }
    }
}
