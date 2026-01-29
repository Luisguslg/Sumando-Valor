using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CreateAuditLogTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                IF EXISTS (SELECT * FROM sys.tables WHERE name = 'AdminAuditEvents')
                    DROP TABLE [AdminAuditEvents];
            ");

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    TableName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecordId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UserEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    OldValues = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValues = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_TableName",
                table: "AuditLogs",
                column: "TableName");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_CreatedAt",
                table: "AuditLogs",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.Sql(@"
                CREATE OR ALTER TRIGGER trg_AuditLog_Insert
                ON AuditLogs
                AFTER INSERT
                AS
                BEGIN
                    SET NOCOUNT ON;
                END;
            ");

            var tables = new[] { "Cursos", "Talleres", "Inscripciones", "Certificados", "EncuestasSatisfaccion", 
                                 "MensajesContacto", "CarouselItems", "SiteImages", "SurveyTemplates", 
                                 "SurveyQuestions", "SurveyResponses", "SurveyAnswers" };

            foreach (var tableName in tables)
            {
                migrationBuilder.Sql($@"
                    CREATE OR ALTER TRIGGER trg_{tableName}_Insert
                    ON [{tableName}]
                    AFTER INSERT
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        DECLARE @UserId NVARCHAR(450) = CAST(SESSION_CONTEXT(N'UserId') AS NVARCHAR(450));
                        DECLARE @UserEmail NVARCHAR(256) = CAST(SESSION_CONTEXT(N'UserEmail') AS NVARCHAR(256));
                        
                        INSERT INTO AuditLogs (TableName, Action, RecordId, UserId, UserEmail, CreatedAt, NewValues)
                        SELECT 
                            '{tableName}',
                            'INSERT',
                            CAST(Id AS NVARCHAR(100)),
                            @UserId,
                            @UserEmail,
                            GETUTCDATE(),
                            (SELECT * FROM inserted FOR JSON AUTO)
                        FROM inserted;
                    END;
                ");

                migrationBuilder.Sql($@"
                    CREATE OR ALTER TRIGGER trg_{tableName}_Update
                    ON [{tableName}]
                    AFTER UPDATE
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        DECLARE @UserId NVARCHAR(450) = CAST(SESSION_CONTEXT(N'UserId') AS NVARCHAR(450));
                        DECLARE @UserEmail NVARCHAR(256) = CAST(SESSION_CONTEXT(N'UserEmail') AS NVARCHAR(256));
                        
                        INSERT INTO AuditLogs (TableName, Action, RecordId, UserId, UserEmail, CreatedAt, OldValues, NewValues)
                        SELECT 
                            '{tableName}',
                            'UPDATE',
                            CAST(d.Id AS NVARCHAR(100)),
                            @UserId,
                            @UserEmail,
                            GETUTCDATE(),
                            (SELECT * FROM deleted WHERE deleted.Id = d.Id FOR JSON AUTO),
                            (SELECT * FROM inserted WHERE inserted.Id = d.Id FOR JSON AUTO)
                        FROM deleted d
                        INNER JOIN inserted i ON d.Id = i.Id;
                    END;
                ");

                migrationBuilder.Sql($@"
                    CREATE OR ALTER TRIGGER trg_{tableName}_Delete
                    ON [{tableName}]
                    AFTER DELETE
                    AS
                    BEGIN
                        SET NOCOUNT ON;
                        DECLARE @UserId NVARCHAR(450) = CAST(SESSION_CONTEXT(N'UserId') AS NVARCHAR(450));
                        DECLARE @UserEmail NVARCHAR(256) = CAST(SESSION_CONTEXT(N'UserEmail') AS NVARCHAR(256));
                        
                        INSERT INTO AuditLogs (TableName, Action, RecordId, UserId, UserEmail, CreatedAt, OldValues)
                        SELECT 
                            '{tableName}',
                            'DELETE',
                            CAST(Id AS NVARCHAR(100)),
                            @UserId,
                            @UserEmail,
                            GETUTCDATE(),
                            (SELECT * FROM deleted FOR JSON AUTO)
                        FROM deleted;
                    END;
                ");
            }
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var tables = new[] { "Cursos", "Talleres", "Inscripciones", "Certificados", "EncuestasSatisfaccion", 
                                 "MensajesContacto", "CarouselItems", "SiteImages", "SurveyTemplates", 
                                 "SurveyQuestions", "SurveyResponses", "SurveyAnswers" };

            foreach (var tableName in tables)
            {
                migrationBuilder.Sql($@"DROP TRIGGER IF EXISTS trg_{tableName}_Insert;");
                migrationBuilder.Sql($@"DROP TRIGGER IF EXISTS trg_{tableName}_Update;");
                migrationBuilder.Sql($@"DROP TRIGGER IF EXISTS trg_{tableName}_Delete;");
            }

            migrationBuilder.DropTable(name: "AuditLogs");
        }
    }
}
