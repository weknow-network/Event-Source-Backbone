using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Entity: {Name}")]
internal struct OperatioEntityInstructions
{
    public string? Name { get; set; }
}
