using Microsoft.EntityFrameworkCore;
using Publink.Data.Entities;

namespace Publink.Data.Context;

public partial class PublinkDbContext : DbContext
{
    public PublinkDbContext(DbContextOptions<PublinkDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<AuditLog> AuditLogs { get; set; }

    public virtual DbSet<DocumentHeader> DocumentHeaders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ToDo: move to separate configuration files if number of entities will grow
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_auditlog");

            entity.Property(e => e.Id).UseIdentityAlwaysColumn();

            // Relationship: AuditLog (correlation_id) -> DocumentHeader (id)
            entity.HasOne(a => a.DocumentHeader)
                .WithMany()
                .HasForeignKey(a => a.CorrelationId);
        });

        modelBuilder.Entity<DocumentHeader>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("pk_documentheader");

            entity.Property(e => e.Id).ValueGeneratedNever();
            entity.Property(e => e.Confidentiality).HasDefaultValue((short)0);
            entity.Property(e => e.ContractFlags).HasDefaultValue(0);
            entity.Property(e => e.IsFunded).HasDefaultValue(false);
            entity.Property(e => e.IsMultiyear).HasDefaultValue(false);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
