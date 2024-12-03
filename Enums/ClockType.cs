namespace Edrakon.Helpers;

public enum ClockType : int
{
    Realtime,
    Monotonic,
    ProcessCpuTimeId,
    ThreadCpuTimeId
}