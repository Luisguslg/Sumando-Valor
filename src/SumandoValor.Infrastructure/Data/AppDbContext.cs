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
        });

        builder.Entity<Taller>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.FechaHoraInicio);
            entity.Property(e => e.Titulo).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Descripcion).HasMaxLength(2000);
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
        });

        builder.Entity<EncuestaSatisfaccion>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.TallerId);
            entity.HasIndex(e => e.UserId);
            entity.HasOne(e => e.Taller)
                .WithMany(t => t.Encuestas)
                .HasForeignKey(e => e.TallerId)
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
