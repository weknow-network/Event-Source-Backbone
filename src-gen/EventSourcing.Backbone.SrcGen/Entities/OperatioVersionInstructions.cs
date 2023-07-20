using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Version: {Version}, Retired: {Retired}")]
internal struct OperatioVersionInstructions
{
    public int Version { get; set; }
    public int? Retired { get; set; }
    public override string ToString() => $"v{Version}";
}
