using Robust.Shared.Prototypes;

namespace Content.Shared.DA.TimeCycle;

/// <summary>
///
/// </summary>
[Prototype("timeCyclePalette")]
public sealed partial class TimeCyclePalettePrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField]
    public Dictionary<int, Color> TimeColors = default!;
}
