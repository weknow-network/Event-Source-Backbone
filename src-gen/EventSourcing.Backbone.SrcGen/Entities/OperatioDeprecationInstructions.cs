using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Deprecate Version: {Date}")]
internal struct OperatioDeprecationInstructions
{
    public string? Remark { get; set; }
    public string? Date { get; set; }
}
