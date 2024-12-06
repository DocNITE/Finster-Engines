﻿using Content.Client.UserInterface.Screens;
using Content.Shared.Hands.Components;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.UserInterface.Systems.Hotbar.Widgets;

[GenerateTypedNameReferences]
public sealed partial class HotbarGui : UIWidget
{
    public BoxContainer StorageContainer;

    public HotbarGui()
    {
        RobustXamlLoader.Load(this);
        var hotbarController = UserInterfaceManager.GetUIController<HotbarUIController>();

        StorageContainer = _StorageContainer;

        hotbarController.Setup(HandContainer, StoragePanel);
        LayoutContainer.SetGrowVertical(this, LayoutContainer.GrowDirection.Begin);
    }

    public void UpdatePanelEntityLeft(EntityUid? entity)
    {
    }

    public void UpdatePanelEntityRight(EntityUid? entity)
    {
    }

    public void SetHighlightHand(HandUILocation? hand)
    {
    }

    public void UpdateStatusVisibility(bool left, bool right)
    {
    }
}
