using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Version: {Version}, Retired: {Retired}")]
internal struct OperationVersionInfo
{
    public int Version { get; set; }
    public int? Retired { get; set; }
    public VersionInfo? Parent { get; set; }
}
