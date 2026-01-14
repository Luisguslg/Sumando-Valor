using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SumandoValor.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserIdRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Migración defensiva: la base puede venir con o sin ApplicationUserId.
            // Eliminamos todo rastro de ApplicationUserId (si existe) y garantizamos FKs por UserId.

            migrationBuilder.Sql(@"
-- Inscripciones: limpiar ApplicationUserId si existe
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Inscripciones_AspNetUsers_ApplicationUserId')
    ALTER TABLE [Inscripciones] DROP CONSTRAINT [FK_Inscripciones_AspNetUsers_ApplicationUserId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Inscripciones_ApplicationUserId' AND object_id = OBJECT_ID(N'[Inscripciones]'))
    DROP INDEX [IX_Inscripciones_ApplicationUserId] ON [Inscripciones];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Inscripciones]') AND name = N'ApplicationUserId')
    ALTER TABLE [Inscripciones] DROP COLUMN [ApplicationUserId];
");

            migrationBuilder.Sql(@"
-- Certificados: limpiar ApplicationUserId si existe
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Certificados_AspNetUsers_ApplicationUserId')
    ALTER TABLE [Certificados] DROP CONSTRAINT [FK_Certificados_AspNetUsers_ApplicationUserId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Certificados_ApplicationUserId' AND object_id = OBJECT_ID(N'[Certificados]'))
    DROP INDEX [IX_Certificados_ApplicationUserId] ON [Certificados];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[Certificados]') AND name = N'ApplicationUserId')
    ALTER TABLE [Certificados] DROP COLUMN [ApplicationUserId];
");

            migrationBuilder.Sql(@"
-- EncuestasSatisfaccion: limpiar ApplicationUserId si existe
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EncuestasSatisfaccion_AspNetUsers_ApplicationUserId')
    ALTER TABLE [EncuestasSatisfaccion] DROP CONSTRAINT [FK_EncuestasSatisfaccion_AspNetUsers_ApplicationUserId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_EncuestasSatisfaccion_ApplicationUserId' AND object_id = OBJECT_ID(N'[EncuestasSatisfaccion]'))
    DROP INDEX [IX_EncuestasSatisfaccion_ApplicationUserId] ON [EncuestasSatisfaccion];
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID(N'[EncuestasSatisfaccion]') AND name = N'ApplicationUserId')
    ALTER TABLE [EncuestasSatisfaccion] DROP COLUMN [ApplicationUserId];
");

            migrationBuilder.Sql(@"
-- Asegurar FKs por UserId (Restrict/NoAction)
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Inscripciones_AspNetUsers_UserId')
    ALTER TABLE [Inscripciones]  WITH CHECK ADD CONSTRAINT [FK_Inscripciones_AspNetUsers_UserId] FOREIGN KEY([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Certificados_AspNetUsers_UserId')
    ALTER TABLE [Certificados]  WITH CHECK ADD CONSTRAINT [FK_Certificados_AspNetUsers_UserId] FOREIGN KEY([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EncuestasSatisfaccion_AspNetUsers_UserId')
    ALTER TABLE [EncuestasSatisfaccion]  WITH CHECK ADD CONSTRAINT [FK_EncuestasSatisfaccion_AspNetUsers_UserId] FOREIGN KEY([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE NO ACTION;
");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Down intencionalmente mínimo: no reintroducimos ApplicationUserId.
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Inscripciones_AspNetUsers_UserId')
    ALTER TABLE [Inscripciones] DROP CONSTRAINT [FK_Inscripciones_AspNetUsers_UserId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_Certificados_AspNetUsers_UserId')
    ALTER TABLE [Certificados] DROP CONSTRAINT [FK_Certificados_AspNetUsers_UserId];
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = N'FK_EncuestasSatisfaccion_AspNetUsers_UserId')
    ALTER TABLE [EncuestasSatisfaccion] DROP CONSTRAINT [FK_EncuestasSatisfaccion_AspNetUsers_UserId];
");
        }
    }
}
