namespace Edrakon;

public class XRException(string? msg = null, Exception? inner = null) : Exception(msg, inner) { }
