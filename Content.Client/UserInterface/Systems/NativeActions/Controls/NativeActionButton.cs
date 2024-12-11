using System.Numerics;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;

namespace Content.Client.UserInterface.Systems.NativeActions.Controls;

[Virtual]
public class NativeActionButton : TextureButton
{
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    private TextureRect _textureRect;

    private Texture _textureNormal;
    private Texture _texturePressed;

    public Texture TextureNormal
    {
        get => _textureNormal;
        set
        {
            _textureNormal = value;
            UpdateTexture();
        }
    }

    public bool Toggled => this.Pressed;

    public Texture TexturePressed
    {
        get => _texturePressed;
        set
        {
            _texturePressed = value;
            UpdateTexture();
        }
    }

    public NativeActionButton(Texture normalTex, Texture toggledTex, bool isPressed = false, bool isToggleable = true)
    {
        IoCManager.InjectDependencies(this);

        _textureNormal = normalTex;
        _texturePressed = toggledTex;

        _textureRect = new TextureRect();
        AddChild(_textureRect);

        Pressed = isPressed;
        ToggleMode = isToggleable;

        UpdateTexture();
    }

    public void Resize(Vector2 size)
    {
        MinSize = size;
        SetSize = size;
        _textureRect.MinSize = size;
        _textureRect.SetSize = size;
    }

    public void UpdateTexture()
    {
        if (Pressed)
            base.TextureNormal = _texturePressed;
        else
            base.TextureNormal = _textureNormal;
    }

    protected override void KeyBindUp(GUIBoundKeyEventArgs args)
    {
        base.KeyBindUp(args);

        if (args.Function == EngineKeyFunctions.UIClick && ToggleMode)
            UpdateTexture();
    }

    public void Toggle()
    {
        SetClickPressed(!Pressed);
    }
}
