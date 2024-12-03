using Silk.NET.OpenXR;
using Action = Silk.NET.OpenXR.Action;

namespace Edrakon.Structs;

public readonly struct XRActionSuggestedBinding(IXRAction action, string binding)
{
    public readonly IXRAction Action = action;
    public readonly string Binding = binding;
}



public readonly struct BooleanAction : IXRAction
{
    public static ActionType Type => ActionType.BooleanInput;
    public readonly Action Action { get; }
}

public readonly struct FloatAction : IXRAction
{
    public static ActionType Type => ActionType.FloatInput;
    public readonly Action Action { get; }
}

public readonly struct Vector2fAction : IXRAction
{
    public static ActionType Type => ActionType.Vector2fInput;
    public readonly Action Action { get; }
}

public readonly struct PosefAction : IXRAction
{
    public static ActionType Type => ActionType.PoseInput;
    public readonly Action Action { get; }
}

public readonly struct VibrationAction : IXRAction
{
    public static ActionType Type => ActionType.VibrationOutput;
    public readonly Action Action { get; }
}

public interface IXRAction
{
    static abstract ActionType Type { get; }
    Action Action { get; }
}