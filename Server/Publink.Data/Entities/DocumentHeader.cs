using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Publink.Data.Entities;

[Table("document_header")]
public partial class DocumentHeader
{
    [Key]
    [Column("id")]
    public Guid Id { get; set; }

    [Column("number")]
    [StringLength(255)]
    public string? Number { get; set; }

    [Column("effective_date")]
    public DateOnly? EffectiveDate { get; set; }

    [Column("execution_date")]
    public DateOnly? ExecutionDate { get; set; }

    [Column("created_date", TypeName = "timestamp(0) without time zone")]
    public DateTime CreatedDate { get; set; }

    [Column("conclusion_date")]
    public DateOnly? ConclusionDate { get; set; }

    [StringLength(255)]
    public string? ContractorId { get; set; }

    [Column("document_type")]
    public short DocumentType { get; set; }

    [Column("subject")]
    [StringLength(255)]
    public string? Subject { get; set; }

    [Column("contract_value", TypeName = "money")]
    public decimal? ContractValue { get; set; }

    [Column("parent_id")]
    public Guid? ParentId { get; set; }

    [Column("reason")]
    [StringLength(255)]
    public string? Reason { get; set; }

    [Column("deleted_date", TypeName = "timestamp(0) without time zone")]
    public DateTime? DeletedDate { get; set; }

    [Column("payment_due_date")]
    public DateOnly? PaymentDueDate { get; set; }

    [StringLength(255)]
    public string? ContractorName { get; set; }

    [Column("organization_id")]
    public Guid OrganizationId { get; set; }

    [Column("engagement_id")]
    public Guid? EngagementId { get; set; }

    [Column("is_multiyear")]
    public bool? IsMultiyear { get; set; }

    [Column("confidentiality")]
    public short Confidentiality { get; set; }

    [Column("sub_unit_id")]
    public Guid? SubUnitId { get; set; }

    [Column("contract_type")]
    public short? ContractType { get; set; }

    [Column("is_funded")]
    public bool IsFunded { get; set; }

    [Column("contractor_id")]
    public Guid? ContractorId1 { get; set; }

    [Column("contract_flags")]
    public int? ContractFlags { get; set; }

    [Column("contract_net_value", TypeName = "money")]
    public decimal? ContractNetValue { get; set; }
}
