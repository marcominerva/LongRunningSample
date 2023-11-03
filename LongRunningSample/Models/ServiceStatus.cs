using LongRunningSample.Models.Enums;

namespace LongRunningSample.Models;

public class ServiceStatus
{
    public Status Status { get; set; }

    public string Exception { get; set; }
}
