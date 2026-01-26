using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SimplifySurveysGlobal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_SurveyTemplates_CursoId",
                table: "SurveyTemplates");

            migrationBuilder.DropIndex(
                name: "IX_SurveyTemplates_TallerId",
                table: "SurveyTemplates");

            migrationBuilder.DropColumn(
                name: "CursoId",
                table: "SurveyTemplates");

            migrationBuilder.DropColumn(
                name: "TallerId",
                table: "SurveyTemplates");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "SurveyTemplates",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "SurveyTemplates");

            migrationBuilder.AddColumn<int>(
                name: "CursoId",
                table: "SurveyTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TallerId",
                table: "SurveyTemplates",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SurveyTemplates_CursoId",
                table: "SurveyTemplates",
                column: "CursoId");

            migrationBuilder.CreateIndex(
                name: "IX_SurveyTemplates_TallerId",
                table: "SurveyTemplates",
                column: "TallerId");
        }
    }
}
