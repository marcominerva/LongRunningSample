using LongRunningSample.Models.Enums;

namespace LongRunningSample.DataAccessLayer.Entities;

public class Execution
{
    public Guid Id { get; set; }

    public string JobId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public Status Status { get; set; }

    public string Exception { get; set; }
}
