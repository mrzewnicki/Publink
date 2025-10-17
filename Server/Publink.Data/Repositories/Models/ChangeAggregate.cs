namespace Publink.Data.Repositories.Models;

public class ChangeAggregate
{
    public Guid CorrelationId { get; set; }
    public int Count { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public TimeSpan Duration => End - Start;
}