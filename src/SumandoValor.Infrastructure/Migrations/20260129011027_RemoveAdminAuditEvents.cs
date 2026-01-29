using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAdminAuditEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Eliminar la tabla si existe (SQL Server eliminará automáticamente los índices)
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminAuditEvents')
                    DROP TABLE [AdminAuditEvents];
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Recrear la tabla (versión básica original)
            migrationBuilder.CreateTable(
                name: "AdminAuditEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ActorUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    TargetUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    DetailsJson = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdminAuditEvents", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditEvents_ActorUserId",
                table: "AdminAuditEvents",
                column: "ActorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditEvents_CreatedAt",
                table: "AdminAuditEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditEvents_TargetUserId",
                table: "AdminAuditEvents",
                column: "TargetUserId");
        }
    }
}
