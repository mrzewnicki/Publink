namespace Publink.Shared.Dtos;

/*
public class AuditLogDto
{
    public int Id { get; set; }
    public Guid? OrganizationId { get; set; }
    public Guid? UserId { get; set; }
    public string? UserEmail { get; set; }
    public int Type { get; set; }
    public int EntityType { get; set; }
    public DateTime CreatedDate { get; set; }
    public string? OldValues { get; set; }
    public string? NewValues { get; set; }
    public string? AffectedColumns { get; set; }
    public string? PrimaryKey { get; set; }
    public Guid? EntityId { get; set; }
    public Guid? ParentId { get; set; }
    public Guid? CorrelationId { get; set; }
    public Guid? SubUnitId { get; set; }
}*/

public class AuditLogDto
{
    public int Id { get; set; }
    public string ChangedBy { get; set; }
    public string ContractNumber { get; set; }
    public int Type { get; set; }
    public int EntityType { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime ProcessTookTime { get; set; }
    public int EntitiesAffectCount { get; set; }
}