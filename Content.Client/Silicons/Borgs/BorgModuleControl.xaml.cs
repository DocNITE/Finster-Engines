using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Silicons.Borgs;

[GenerateTypedNameReferences]
public sealed partial class BorgModuleControl : PanelContainer
{
    public Action? RemoveButtonPressed;

    public BorgModuleControl(EntityUid entity, IEntityManager entityManager)
    {
        RobustXamlLoader.Load(this);

        ModuleView.SetEntity(entity);
        ModuleName.Text = entityManager.GetComponent<MetaDataComponent>(entity).EntityName;
        RemoveButton.TexturePath = "/Textures/Interface/Lora/cross.svg.png";
        RemoveButton.OnPressed += _ =>
        {
            RemoveButtonPressed?.Invoke();
        };
    }
}

