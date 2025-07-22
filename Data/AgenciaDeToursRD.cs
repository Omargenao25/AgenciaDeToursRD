
using Microsoft.EntityFrameworkCore;
using AgenciaDeToursRD.Models;


namespace AgenciaDeToursRD.Data
{
    public class AgenciaDeToursDbContext : DbContext
    {
        public AgenciaDeToursDbContext(DbContextOptions<AgenciaDeToursDbContext> options)
            : base(options) { }

        public DbSet<Pais> Paises { get; set; }
        public DbSet<Destino> Destinos { get; set; }
        public DbSet<Tour> Tours { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Tabla Pais
            modelBuilder.Entity<Pais>(entity =>
            {
                entity.HasKey(p => p.ID);

                entity.Property(p => p.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.HasMany(p => p.Destinos)
                      .WithOne(d => d.Pais)
                      .HasForeignKey(d => d.PaisId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Tabla Destino
            modelBuilder.Entity<Destino>(entity =>
            {
                entity.HasKey(d => d.ID);

                entity.Property(d => d.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(d => d.DuracionTexto)
                      .IsRequired()
                      .HasMaxLength(50);

                entity.HasOne(d => d.Pais)
                      .WithMany(p => p.Destinos)
                      .HasForeignKey(d => d.PaisId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // Tabla Tour
            modelBuilder.Entity<Tour>(entity =>
            {
                entity.HasKey(t => t.ID);

                entity.Property(t => t.Nombre)
                      .IsRequired()
                      .HasMaxLength(100);

                entity.Property(t => t.Precio)
                      .HasColumnType("decimal(18,2)");

                entity.HasOne(t => t.Destino)
                      .WithMany()
                      .HasForeignKey(t => t.DestinoID)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(t => t.Pais)
                      .WithMany()
                      .HasForeignKey(t => t.PaisID)
                      .OnDelete(DeleteBehavior.Restrict);
            });
        }


    }
}