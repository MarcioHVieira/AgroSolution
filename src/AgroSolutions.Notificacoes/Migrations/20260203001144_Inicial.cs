using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AgroSolutions.Notificacoes.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AlertaId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalhaoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DestinatarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailDestinatario = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    NomeDestinatario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Prioridade = table.Column<int>(type: "int", nullable: false),
                    Assunto = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Mensagem = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DataEnvio = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TentativasEnvio = table.Column<int>(type: "int", nullable: false),
                    MensagemErro = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DadosAdicionais = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PropriedadesInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProprietarioId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmailProprietario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    NomeProprietario = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DataSincronizacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PropriedadesInfo", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_DataCriacao",
                table: "Notificacoes",
                column: "DataCriacao");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_DestinatarioId",
                table: "Notificacoes",
                column: "DestinatarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Notificacoes_Status",
                table: "Notificacoes",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PropriedadesInfo_ProprietarioId",
                table: "PropriedadesInfo",
                column: "ProprietarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "PropriedadesInfo");
        }
    }
}
