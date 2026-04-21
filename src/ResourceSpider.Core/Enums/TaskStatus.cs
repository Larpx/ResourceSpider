namespace ResourceSpider.Core.Enums;

public enum TaskStatus
{
    Pending = 0,
    Running = 1,
    Completed = 2,
    Failed = 3,
    Paused = 4,
    WaitingRecovery = 5,
    Cancelled = 6
}
