namespace Publink.Data.Entities.Enums;

// ToDo: "Type" name is too generic in C#, I would change it even if it makes sense in the context of the entity. I would change it to AuditLogType
public enum Type
{
    Added = 1,
    Deleted = 2,
    Modified = 3,
}