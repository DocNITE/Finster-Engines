using Content.Client.Info;
using Content.Client.Info.PlaytimeStats;
using Content.Client.Resources;
using Content.Shared.Preferences;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Prototypes;

namespace Content.Client.Lobby.UI
{
    /// <summary>
    /// Holds the entire character setup GUI, from character picks to individual character editing.
    /// </summary>
    [GenerateTypedNameReferences]
    public sealed partial class CharacterSetupGui : DefaultWindow
    {
        private readonly IClientPreferencesManager _preferencesManager;
        private readonly IEntityManager _entManager;
        private readonly IPrototypeManager _protomanager;

        private readonly Button _createNewCharacterButton;

        public event Action<int>? SelectCharacter;
        public event Action<int>? DeleteCharacter;

        public CharacterSetupGui(
            IEntityManager entManager,
            IPrototypeManager protoManager,
            IResourceCache resourceCache,
            IClientPreferencesManager preferencesManager,
            HumanoidProfileEditor profileEditor)
        {
            RobustXamlLoader.Load(this);
            _preferencesManager = preferencesManager;
            _entManager = entManager;
            _protomanager = protoManager;

            var panelTex = resourceCache.GetTexture("/Textures/Interface/Lora/button.svg.96dpi.png");
            var back = new StyleBoxTexture
            {
                Texture = panelTex,
                Modulate = new Color(15, 15, 15)
            };
            back.SetPatchMargin(StyleBox.Margin.All, 10);

            BackgroundPanel.PanelOverride = back;

            Title = Loc.GetString("character-setup-gui-character-setup-label");

            _createNewCharacterButton = new Button
            {
                Text = Loc.GetString("character-setup-gui-create-new-character-button"),
            };

            _createNewCharacterButton.OnPressed += args =>
            {
                preferencesManager.CreateCharacter(HumanoidCharacterProfile.Random());
                ReloadCharacterPickers();
                args.Event.Handle();
            };

            CharEditor.AddChild(profileEditor);
        }

        /// <summary>
        /// Disposes and reloads all character picker buttons from the preferences data.
        /// </summary>
        public void ReloadCharacterPickers()
        {
            _createNewCharacterButton.Orphan();
            Characters.DisposeAllChildren();

            var numberOfFullSlots = 0;
            var characterButtonsGroup = new ButtonGroup();

            if (!_preferencesManager.ServerDataLoaded)
            {
                return;
            }

            _createNewCharacterButton.ToolTip =
                Loc.GetString("character-setup-gui-create-new-character-button-tooltip",
                    ("maxCharacters", _preferencesManager.Settings!.MaxCharacterSlots));

            var selectedSlot = _preferencesManager.Preferences?.SelectedCharacterIndex;

            foreach (var (slot, character) in _preferencesManager.Preferences!.Characters)
            {
                numberOfFullSlots++;
                var characterPickerButton = new CharacterPickerButton(_entManager,
                    _protomanager,
                    characterButtonsGroup,
                    character,
                    slot == selectedSlot);

                Characters.AddChild(characterPickerButton);

                characterPickerButton.OnPressed += args =>
                {
                    SelectCharacter?.Invoke(slot);
                };

                characterPickerButton.OnDeletePressed += () =>
                {
                    DeleteCharacter?.Invoke(slot);
                };
            }

            _createNewCharacterButton.Disabled = numberOfFullSlots >= _preferencesManager.Settings.MaxCharacterSlots;
            Characters.AddChild(_createNewCharacterButton);
        }
    }
}
