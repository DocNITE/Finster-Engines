using System.Numerics;
using Content.Client.CombatMode;
using Content.Client.Gameplay;
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.NativeActions.Controls;
using Content.Client.UserInterface.Systems.NativeActions.Widgets;
using Content.Shared.CombatMode;
using Content.Shared.Input;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;

namespace Content.Client.UserInterface.Systems;

public sealed class NativeActionsUIController : UIController, IOnStateEntered<GameplayState>, IOnStateExited<GameplayState>, IOnSystemChanged<CombatModeSystem>
{
    [UISystemDependency] private readonly CombatModeSystem _combatSystem = default!;

    private NativeActionsGui? _nativeActionsGui;

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

        ReloadActions();
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

    public void OnPlayerAttached(EntityUid uid)
    {
        _playerUid = uid;

        ReloadButtons();
    }

    public void ReloadActions()
    {
        if (UIManager.ActiveScreen == null)
            return;

        if (UIManager.ActiveScreen.GetWidget<NativeActionsGui>() is { } nativeActions)
        {
            if (_nativeActionsGui == null)
                _nativeActionsGui = nativeActions;
        }
    }

    public void ToggleCombatMode(bool ignoreButton = true)
    {
        if (_nativeActionsGui != null && !ignoreButton)
            _nativeActionsGui.CombatModeButton.Toggle();

        _combatSystem.LocalToggleCombatMode();
    }

    // TODO: It might be moved into prototype definition
    public void ReloadButtons()
    {
        if (UIManager.ActiveScreen == null)
            return;

        if (UIManager.ActiveScreen.GetWidget<NativeActionsGui>() is { } nativeActions)
        {

        }
    }

    public CombatModeSystem ResolveCombatSystem()
    {
        return _combatSystem;
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
