namespace Publink.Shared.Dtos;

public class QueryDto
{
    public int PageSize { get; set; }
    public int PageNumber { get; set; }
    // Sorting format: "Property:asc|desc" e.g. "CreatedDate:desc"
    public string? Sort { get; set; }
}