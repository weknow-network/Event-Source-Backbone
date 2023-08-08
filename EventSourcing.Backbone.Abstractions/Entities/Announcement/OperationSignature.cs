using System.Diagnostics;

namespace EventSourcing.Backbone;

[DebuggerDisplay("{Operation}_V{Version}({Parameters})")]
public readonly record struct OperationSignature(string Operation, int Version, string Parameters)
{
    public override string ToString() => $"{Operation}_V{Version}_{Parameters}";
}
