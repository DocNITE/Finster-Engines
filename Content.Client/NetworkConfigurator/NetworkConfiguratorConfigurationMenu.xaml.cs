﻿using Content.Client.Stylesheets;
using Content.Client.UserInterface.Controls;
using Content.Shared.DeviceNetwork;
using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.NetworkConfigurator;

[GenerateTypedNameReferences]
public sealed partial class NetworkConfiguratorConfigurationMenu : FancyWindow
{
    public NetworkConfiguratorConfigurationMenu()
    {
        RobustXamlLoader.Load(this);

        Clear.StyleClasses.Add(StyleBase.ButtonOpenLeft);
        Clear.StyleClasses.Add(StyleLora.StyleClassButtonColorRed);
    }

    public void UpdateState(DeviceListUserInterfaceState state)
    {
        DeviceList.UpdateState(null, state.DeviceList);

        Count.Text = Loc.GetString("network-configurator-ui-count-label", ("count", state.DeviceList.Count));
    }
}
