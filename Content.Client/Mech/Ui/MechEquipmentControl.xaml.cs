using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Mech.Ui;

[GenerateTypedNameReferences]
public sealed partial class MechEquipmentControl : Control
{
    public event Action? OnRemoveButtonPressed;

    public MechEquipmentControl(EntityUid entity, string itemName, Control? fragment)
    {
        RobustXamlLoader.Load(this);
        EquipmentName.SetMessage(itemName);
        EquipmentView.SetEntity(entity);
        RemoveButton.TexturePath = "/Textures/Interface/Lora/cross.svg.png";

        if (fragment != null)
        {
            Separator.Visible = true;
            CustomControlContainer.AddChild(fragment);
        }

        RemoveButton.OnPressed += _ => OnRemoveButtonPressed?.Invoke();
    }
}
