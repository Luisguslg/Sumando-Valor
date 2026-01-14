using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCursoAndUpdateTaller : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inscripciones_AspNetUsers_ApplicationUserId",
                table: "Inscripciones");

            migrationBuilder.DropIndex(
                name: "IX_Inscripciones_ApplicationUserId",
                table: "Inscripciones");

            migrationBuilder.DropColumn(
                name: "PublicoObjetivo",
                table: "Talleres");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Inscripciones");

            migrationBuilder.RenameColumn(
                name: "FechaHoraInicio",
                table: "Talleres",
                newName: "FechaInicio");

            migrationBuilder.RenameColumn(
                name: "EsPublico",
                table: "Talleres",
                newName: "RequiereEncuesta");

            migrationBuilder.RenameColumn(
                name: "DuracionMin",
                table: "Talleres",
                newName: "CursoId");

            migrationBuilder.RenameColumn(
                name: "Cupos",
                table: "Talleres",
                newName: "CuposMaximos");

            migrationBuilder.RenameIndex(
                name: "IX_Talleres_FechaHoraInicio",
                table: "Talleres",
                newName: "IX_Talleres_FechaInicio");

            migrationBuilder.AlterColumn<string>(
                name: "PlataformaDigital",
                table: "Talleres",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FacilitadorTexto",
                table: "Talleres",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CuposDisponibles",
                table: "Talleres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFin",
                table: "Talleres",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraInicio",
                table: "Talleres",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

            migrationBuilder.AddColumn<bool>(
                name: "PermiteCertificado",
                table: "Talleres",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "Asistencia",
                table: "Inscripciones",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "ScorePromedio",
                table: "EncuestasSatisfaccion",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Cursos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Titulo = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PublicoObjetivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    EsPublico = table.Column<bool>(type: "bit", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Estado = table.Column<int>(type: "int", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cursos", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Talleres_CursoId",
                table: "Talleres",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_Talleres_Estatus",
                table: "Talleres",
                column: "Estatus");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_Estado",
                table: "Cursos",
                column: "Estado");

            migrationBuilder.CreateIndex(
                name: "IX_Cursos_Orden",
                table: "Cursos",
                column: "Orden");

            migrationBuilder.AddForeignKey(
                name: "FK_Inscripciones_AspNetUsers_UserId",
                table: "Inscripciones",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Talleres_Cursos_CursoId",
                table: "Talleres",
                column: "CursoId",
                principalTable: "Cursos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Inscripciones_AspNetUsers_UserId",
                table: "Inscripciones");

            migrationBuilder.DropForeignKey(
                name: "FK_Talleres_Cursos_CursoId",
                table: "Talleres");

            migrationBuilder.DropTable(
                name: "Cursos");

            migrationBuilder.DropIndex(
                name: "IX_Talleres_CursoId",
                table: "Talleres");

            migrationBuilder.DropIndex(
                name: "IX_Talleres_Estatus",
                table: "Talleres");

            migrationBuilder.DropColumn(
                name: "CuposDisponibles",
                table: "Talleres");

            migrationBuilder.DropColumn(
                name: "FechaFin",
                table: "Talleres");

            migrationBuilder.DropColumn(
                name: "HoraInicio",
                table: "Talleres");

            migrationBuilder.DropColumn(
                name: "PermiteCertificado",
                table: "Talleres");

            migrationBuilder.DropColumn(
                name: "Asistencia",
                table: "Inscripciones");

            migrationBuilder.RenameColumn(
                name: "RequiereEncuesta",
                table: "Talleres",
                newName: "EsPublico");

            migrationBuilder.RenameColumn(
                name: "FechaInicio",
                table: "Talleres",
                newName: "FechaHoraInicio");

            migrationBuilder.RenameColumn(
                name: "CursoId",
                table: "Talleres",
                newName: "DuracionMin");

            migrationBuilder.RenameColumn(
                name: "CuposMaximos",
                table: "Talleres",
                newName: "Cupos");

            migrationBuilder.RenameIndex(
                name: "IX_Talleres_FechaInicio",
                table: "Talleres",
                newName: "IX_Talleres_FechaHoraInicio");

            migrationBuilder.AlterColumn<string>(
                name: "PlataformaDigital",
                table: "Talleres",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FacilitadorTexto",
                table: "Talleres",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicoObjetivo",
                table: "Talleres",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Inscripciones",
                type: "nvarchar(450)",
                nullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "ScorePromedio",
                table: "EncuestasSatisfaccion",
                type: "decimal(18,2)",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "decimal(5,2)",
                oldPrecision: 5,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Inscripciones_ApplicationUserId",
                table: "Inscripciones",
                column: "ApplicationUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Inscripciones_AspNetUsers_ApplicationUserId",
                table: "Inscripciones",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
