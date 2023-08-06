using System.Diagnostics;

namespace EventSourcing.Backbone.SrcGen.Entities;

[DebuggerDisplay("Deprecate Version: {Date}")]
internal struct OperatioDeprecationInstructions
{
    public OperatioDeprecationInstructions(string kind, string? remark = null, string? date = null)
    {
        Kind = kind switch
        {
            nameof(EventsContractType.Producer) => EventsContractType.Producer,
            nameof(EventsContractType.Consumer) => EventsContractType.Consumer,
            _ => throw new NotImplementedException()
        };
        Remark = remark;
        Date = date;
    }
    public EventsContractType Kind { get; }
    public string? Remark { get;  }
    public string? Date { get;  }
}
