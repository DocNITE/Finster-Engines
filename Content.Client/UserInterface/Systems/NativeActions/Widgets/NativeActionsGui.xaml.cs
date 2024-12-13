
using System.Numerics;
using Content.Client.UserInterface.Systems.NativeActions.Controls;
using Robust.Client.AutoGenerated;
using Robust.Client.Input;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Systems.NativeActions.Widgets;

[GenerateTypedNameReferences]
public sealed partial class NativeActionsGui : UIWidget
{
    //[Dependency] private readonly IEntityManager _entity = default!;
    //[Dependency] private readonly IInputManager _input = default!;

    private readonly NativeActionsUIController _controller;

    public NativeActionButton CombatModeButton;

    public NativeActionsGui()
    {
        RobustXamlLoader.Load(this);
        IoCManager.InjectDependencies(this);

        _controller = UserInterfaceManager.GetUIController<NativeActionsUIController>();

        var combatSystem = _controller.ResolveCombatSystem();

        // Combat Mode
        //var isInCombat = combatSystem.IsInCombatMode();
        CombatModeButton = new NativeActionButton(Theme.ResolveTexture("harm_off"), Theme.ResolveTexture("harm_on"));
        CombatModeButton.Resize(new Vector2(64, 64));
        CombatModeButton.OnToggled += (args) =>
        {
            _controller.ToggleCombatMode(false);
        };
        ActionsContainer.AddChild(CombatModeButton);
    }

    protected override void OnThemeUpdated()
    {
        CombatModeButton.TextureNormal = Theme.ResolveTexture("harm_off");
        CombatModeButton.TexturePressed = Theme.ResolveTexture("harm_on");
    }
}
