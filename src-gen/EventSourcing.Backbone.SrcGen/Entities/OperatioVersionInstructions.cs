using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Version: {Version}")]
internal struct OperatioVersionInstructions
{
    public int Version { get; set; }
    public string? Remark { get; set; }
    public string? Date { get; set; }
    public override string ToString() => $"v{Version}";
}
