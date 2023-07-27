using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Deprecate Version: {Version}")]
internal struct OperatioDeprecationInstructions
{
    public int Version { get; set; }
    public string? Remark { get; set; }
    public string? Date { get; set; }
    public override string ToString() => $"v{Version}";
}
