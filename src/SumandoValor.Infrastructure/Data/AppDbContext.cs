using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SumandoValor.Domain.Entities;

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
            entity.Property(e => e.CanalConocio).IsRequired();
            entity.Property(e => e.Estado).IsRequired();
            entity.Property(e => e.Ciudad).IsRequired();
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
            entity.HasIndex(e => e.TallerId);
            entity.HasIndex(e => e.UserId);
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
    }
}
