using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSurveyFieldsAndIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Comentario",
                table: "EncuestasSatisfaccion",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Rating1_5",
                table: "EncuestasSatisfaccion",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_EncuestasSatisfaccion_TallerId_UserId",
                table: "EncuestasSatisfaccion",
                columns: new[] { "TallerId", "UserId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EncuestasSatisfaccion_TallerId_UserId",
                table: "EncuestasSatisfaccion");

            migrationBuilder.DropColumn(
                name: "Comentario",
                table: "EncuestasSatisfaccion");

            migrationBuilder.DropColumn(
                name: "Rating1_5",
                table: "EncuestasSatisfaccion");
        }
    }
}
