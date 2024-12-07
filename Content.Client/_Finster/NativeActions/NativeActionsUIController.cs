using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Shared.CombatMode;
using Content.Shared.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Client._Finster.NativeActions;

public sealed class NativeActionsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CombatModeSystem>
{
    [UISystemDependency] private readonly CombatModeSystem _combatSystem = default!;

    private EntityUid? _playerUid;

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
    }

    private void OnScreenLoad()
    {
        if (UIManager.ActiveScreen == null)
            return;
    }

    public void OnStateEntered(GameplayState state)
    {
        //bind enable/disable combat mode key to ToggleCombatMenu;
        CommandBinds.Builder
           .Bind(ContentKeyFunctions.ToggleCombatMode, InputCmdHandler.FromDelegate(_ => ToggleCombatMode()))
           .Register<CombatModeSystem>();
    }

    public void OnStateExited(GameplayState state)
    {
        CommandBinds.Unregister<CombatModeSystem>();
    }

    public void ToggleCombatMode()
    {
        _combatSystem.LocalToggleCombatMode();
    }

    public void OnPlayerAttached(EntityUid uid)
    {
        _playerUid = uid;
    }

    public void OnPlayerDetached(EntityUid uid)
    {
        if (_playerUid == uid)
            _playerUid = null;
    }
    public void OnSystemLoaded(CombatModeSystem system)
    {
        system.LocalPlayerAttached += OnPlayerAttached;
        system.LocalPlayerDetached += OnPlayerDetached;
    }

    public void OnSystemUnloaded(CombatModeSystem system)
    {
        system.LocalPlayerAttached -= OnPlayerAttached;
        system.LocalPlayerDetached -= OnPlayerDetached;
    }
}
