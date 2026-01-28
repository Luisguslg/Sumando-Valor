using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAdminAuditEventFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ActorEmail",
                table: "AdminAuditEvents",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityId",
                table: "AdminAuditEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntityType",
                table: "AdminAuditEvents",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "AdminAuditEvents",
                type: "nvarchar(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NewValuesJson",
                table: "AdminAuditEvents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OldValuesJson",
                table: "AdminAuditEvents",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditEvents_EntityId",
                table: "AdminAuditEvents",
                column: "EntityId");

            migrationBuilder.CreateIndex(
                name: "IX_AdminAuditEvents_EntityType",
                table: "AdminAuditEvents",
                column: "EntityType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AdminAuditEvents_EntityType",
                table: "AdminAuditEvents");

            migrationBuilder.DropIndex(
                name: "IX_AdminAuditEvents_EntityId",
                table: "AdminAuditEvents");

            migrationBuilder.DropColumn(
                name: "ActorEmail",
                table: "AdminAuditEvents");

            migrationBuilder.DropColumn(
                name: "EntityId",
                table: "AdminAuditEvents");

            migrationBuilder.DropColumn(
                name: "EntityType",
                table: "AdminAuditEvents");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "AdminAuditEvents");

            migrationBuilder.DropColumn(
                name: "NewValuesJson",
                table: "AdminAuditEvents");

            migrationBuilder.DropColumn(
                name: "OldValuesJson",
                table: "AdminAuditEvents");
        }
    }
}
