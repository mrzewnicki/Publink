using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Publink.Data.Entities;

[Table("audit_log")]
public partial class AuditLog
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("organization_id")]
    public Guid? OrganizationId { get; set; }

    [Column("user_id")]
    public Guid? UserId { get; set; }

    [Column("user_email")]
    [StringLength(255)]
    public string? UserEmail { get; set; }

    [Column("type")]
    public int Type { get; set; }

    [Column("entity_type")]
    public int EntityType { get; set; }

    [Column("created_date", TypeName = "timestamp(6) without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("old_values")]
    public string? OldValues { get; set; }

    [Column("new_values")]
    public string? NewValues { get; set; }

    [Column("affected_columns")]
    public string? AffectedColumns { get; set; }

    [Column("primary_key")]
    [StringLength(1024)]
    public string? PrimaryKey { get; set; }

    [Column("entity_id")]
    public Guid? EntityId { get; set; }

    [Column("parent_id")]
    public Guid? ParentId { get; set; }

    [Column("correlation_id")]
    public Guid? CorrelationId { get; set; }

    [Column("sub_unit_id")]
    public Guid? SubUnitId { get; set; }
}
