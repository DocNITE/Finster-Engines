using Content.Server.NPC.HTN;
using Content.Shared.CombatMode;

namespace Content.Server.CombatMode;

public sealed class CombatModeSystem : SharedCombatModeSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeNetworkEvent<ToggleCombatModeEvent>(OnToggleCombatMode);
    }

    public void OnToggleCombatMode(ToggleCombatModeEvent ev, EntitySessionEventArgs args)
    {
        if (!args.SenderSession.AttachedEntity.HasValue)
            return;

        var uid = args.SenderSession.AttachedEntity.Value;
        if (!TryComp(uid, out CombatModeComponent? comp))
            return;

        PerformAction(uid, comp, uid);
    }

    protected override bool IsNpc(EntityUid uid)
    {
        return HasComp<HTNComponent>(uid);
    }
}
