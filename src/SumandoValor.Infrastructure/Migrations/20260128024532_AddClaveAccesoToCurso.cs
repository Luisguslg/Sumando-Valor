using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClaveAccesoToCurso : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClaveAcceso",
                table: "Cursos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TokenAccesoUnico",
                table: "Cursos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_TokenAccesoUnico",
                table: "Cursos",
                column: "TokenAccesoUnico",
                unique: true,
                filter: "[TokenAccesoUnico] IS NOT NULL");

            migrationBuilder.AddColumn<DateTime>(
                name: "TokenExpiracion",
                table: "Cursos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Cursos_TokenAccesoUnico",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "ClaveAcceso",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "TokenAccesoUnico",
                table: "Cursos");

            migrationBuilder.DropColumn(
                name: "TokenExpiracion",
                table: "Cursos");
        }
    }
}
