using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;
using SumandoValor.Domain.Entities.Surveys;

namespace SumandoValor.Infrastructure.Data;

public class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Curso> Cursos { get; set; }
    public DbSet<Taller> Talleres { get; set; }
    public DbSet<Inscripcion> Inscripciones { get; set; }
    public DbSet<Certificado> Certificados { get; set; }
    public DbSet<EncuestaSatisfaccion> EncuestasSatisfaccion { get; set; }
    public DbSet<MensajeContacto> MensajesContacto { get; set; }
    public DbSet<AdminAuditEvent> AdminAuditEvents { get; set; }
    public DbSet<CarouselItem> CarouselItems { get; set; }
    public DbSet<SiteImage> SiteImages { get; set; }
    public DbSet<SurveyTemplate> SurveyTemplates { get; set; }
    public DbSet<SurveyQuestion> SurveyQuestions { get; set; }
    public DbSet<SurveyResponse> SurveyResponses { get; set; }
    public DbSet<SurveyAnswer> SurveyAnswers { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.HasIndex(e => e.Cedula).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Nombres).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Apellidos).IsRequired().HasMaxLength(80);
            entity.Property(e => e.Cedula).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Sexo).IsRequired();
            entity.Property(e => e.FechaNacimiento).IsRequired();
            entity.Property(e => e.DiscapacidadDescripcion).HasMaxLength(120);
            entity.Property(e => e.NivelEducativo).IsRequired();
            entity.Property(e => e.SituacionLaboral).IsRequired();
            entity.Property(e => e.Sector).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CanalConocio).IsRequired();
            entity.Property(e => e.Pais).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Estado).HasMaxLength(100);
            entity.Property(e => e.Municipio).HasMaxLength(100);
            entity.Property(e => e.Ciudad).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(25);
        });

        builder.Entity<Curso>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Estado);
            entity.HasIndex(e => e.Orden);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(150);
            entity.Property(e => e.Descripcion).IsRequired();
            entity.Property(e => e.PublicoObjetivo).HasMaxLength(500);
            entity.Property(e => e.ClaveAcceso).HasMaxLength(20);
            entity.Property(e => e.TokenAccesoUnico).HasMaxLength(100);
            entity.HasIndex(e => e.TokenAccesoUnico).IsUnique().HasFilter("[TokenAccesoUnico] IS NOT NULL");
        });

        builder.Entity<Taller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CursoId);
            entity.HasIndex(e => e.FechaInicio);
            entity.HasIndex(e => e.Estatus);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(2000);
            entity.Property(e => e.PlataformaDigital).HasMaxLength(200);
            entity.Property(e => e.FacilitadorTexto).HasMaxLength(200);
            entity.HasOne(e => e.Curso)
                .WithMany(c => c.Talleres)
                .HasForeignKey(e => e.CursoId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Inscripcion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TallerId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.TallerId);
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.Taller)
                .WithMany(t => t.Inscripciones)
                .HasForeignKey(e => e.TallerId)
                .OnDelete(DeleteBehavior.Restrict);
            // Relación explícita para evitar FK shadow "ApplicationUserId"
            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.Inscripciones)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<Certificado>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TallerId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.TallerId);
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.Taller)
                .WithMany(t => t.Certificados)
                .HasForeignKey(e => e.TallerId)
                .OnDelete(DeleteBehavior.Restrict);
            // Relación explícita para evitar FK shadow "ApplicationUserId"
            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.Certificados)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<EncuestaSatisfaccion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TallerId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.TallerId);
            entity.HasIndex(e => e.UserId);
            entity.Property(e => e.Rating1_5).IsRequired();
            entity.Property(e => e.Comentario).HasMaxLength(2000);
            entity.Property(e => e.ScorePromedio).HasPrecision(5, 2);
            entity.HasOne(e => e.Taller)
                .WithMany(t => t.Encuestas)
                .HasForeignKey(e => e.TallerId)
                .OnDelete(DeleteBehavior.Restrict);
            // Relación explícita para evitar FK shadow "ApplicationUserId"
            entity.HasOne<ApplicationUser>()
                .WithMany(u => u.Encuestas)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<MensajeContacto>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.Property(e => e.Nombre).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Mensaje).IsRequired().HasMaxLength(2000);
        });

        builder.Entity<AdminAuditEvent>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.CreatedAt);
            entity.HasIndex(e => e.ActorUserId);
            entity.HasIndex(e => e.TargetUserId);
            entity.Property(e => e.Action).IsRequired().HasMaxLength(80);
            entity.Property(e => e.DetailsJson).HasMaxLength(4000);
        });

        builder.Entity<CarouselItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.SortOrder);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.AltText).IsRequired().HasMaxLength(200);
        });

        builder.Entity<SiteImage>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Key).IsUnique();
            entity.Property(e => e.Key).IsRequired().HasMaxLength(80);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(260);
            entity.Property(e => e.AltText).IsRequired().HasMaxLength(200);
            entity.HasIndex(e => e.UpdatedAt);
        });

        builder.Entity<SurveyTemplate>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsActive);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(4000);
            entity.HasMany(e => e.Questions)
                .WithOne(q => q.Template)
                .HasForeignKey(q => q.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SurveyQuestion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TemplateId, e.Order });
            entity.Property(e => e.Text).IsRequired().HasMaxLength(500);
            entity.Property(e => e.OptionsJson).HasMaxLength(4000);
        });

        builder.Entity<SurveyResponse>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => new { e.TallerId, e.UserId }).IsUnique();
            entity.HasIndex(e => e.TemplateId);
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(450);
            entity.HasMany(e => e.Answers)
                .WithOne(a => a.Response)
                .HasForeignKey(a => a.ResponseId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<SurveyAnswer>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.QuestionId);
            entity.Property(e => e.Value).IsRequired().HasMaxLength(2000);
        });
    }
}
