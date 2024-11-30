using System.Linq;
using Content.Client.Administration.Systems;
using Content.Client.UserInterface.Controls;
using Content.Client.Verbs.UI;
using Content.Shared.Administration;
using Robust.Client.AutoGenerated;
using Robust.Client.GameObjects;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared.Input;
using Robust.Shared.Utility;

namespace Content.Client.Administration.UI.CustomControls;

[GenerateTypedNameReferences]
public sealed partial class PlayerListControl : BoxContainer
{
    private readonly AdminSystem _adminSystem;

    private readonly IEntityManager _entManager;
    private readonly IUserInterfaceManager _uiManager;

    private PlayerInfo? _selectedPlayer;

    private List<PlayerInfo> _playerList = new();
    private readonly List<PlayerInfo> _sortedPlayerList = new();

    public Comparison<PlayerInfo>? Comparison;
    public Func<PlayerInfo, string, string>? OverrideText;

    public PlayerListControl()
    {
        _entManager = IoCManager.Resolve<IEntityManager>();
        _uiManager = IoCManager.Resolve<IUserInterfaceManager>();
        _adminSystem = _entManager.System<AdminSystem>();
        RobustXamlLoader.Load(this);
        // Fill the Option data
        PlayerListContainer.ItemPressed += PlayerListItemPressed;
        PlayerListContainer.ItemKeyBindDown += PlayerListItemKeyBindDown;
        PlayerListContainer.GenerateItem += GenerateButton;
        PlayerListContainer.NoItemSelected += PlayerListNoItemSelected;
        PopulateList(_adminSystem.PlayerList);
        FilterLineEdit.OnTextChanged += _ => FilterList();
        _adminSystem.PlayerListChanged += PopulateList;
        BackgroundPanel.PanelOverride = new StyleBoxFlat { BackgroundColor = new Color(20, 20, 20) };
    }

    public IReadOnlyList<PlayerInfo> PlayerInfo => _playerList;

    public event Action<PlayerInfo?>? OnSelectionChanged;

    private void PlayerListNoItemSelected()
    {
        _selectedPlayer = null;
        OnSelectionChanged?.Invoke(null);
    }

    private void PlayerListItemKeyBindDown(GUIBoundKeyEventArgs? args, ListData? data)
    {
        if (args == null || data is not PlayerListData { Info: var selectedPlayer })
            return;

        if (args.Function != EngineKeyFunctions.UIRightClick || selectedPlayer.NetEntity == null)
            return;

        _uiManager.GetUIController<VerbMenuUIController>().OpenVerbMenu(selectedPlayer.NetEntity.Value, true);
        args.Handle();
    }

    private void PlayerListItemPressed(BaseButton.ButtonEventArgs? args, ListData? data)
    {
        if (args == null || data is not PlayerListData { Info: var selectedPlayer })
            return;

        if (selectedPlayer == _selectedPlayer)
            return;

        if (args.Event.Function != EngineKeyFunctions.UIClick)
            return;

        OnSelectionChanged?.Invoke(selectedPlayer);
        _selectedPlayer = selectedPlayer;

        // update label text. Only required if there is some override (e.g. unread bwoink count).
        if (OverrideText != null && args.Button.Children.FirstOrDefault()?.Children?.FirstOrDefault() is Label label)
            label.Text = GetText(selectedPlayer);
    }

    public void StopFiltering()
    {
        FilterLineEdit.Text = string.Empty;
    }

    private void FilterList()
    {
        _sortedPlayerList.Clear();
        foreach (var info in _playerList)
        {
            var displayName = $"{info.CharacterName} ({info.Username})";
            if (info.IdentityName != info.CharacterName)
                displayName += $" [{info.IdentityName}]";
            if (!string.IsNullOrEmpty(FilterLineEdit.Text)
                && !displayName.ToLowerInvariant().Contains(FilterLineEdit.Text.Trim().ToLowerInvariant()))
                continue;
            _sortedPlayerList.Add(info);
        }

        if (Comparison != null)
            _sortedPlayerList.Sort((a, b) => Comparison(a, b));

        // Ensure pinned players are always at the top
        _sortedPlayerList.Sort((a, b) => a.IsPinned != b.IsPinned && a.IsPinned ? -1 : 1);

        PlayerListContainer.PopulateList(_sortedPlayerList.Select(info => new PlayerListData(info)).ToList());
        if (_selectedPlayer != null)
            PlayerListContainer.Select(new PlayerListData(_selectedPlayer));
    }

    public void PopulateList(IReadOnlyList<PlayerInfo>? players = null)
    {
        players ??= _adminSystem.PlayerList;

        _playerList = players.ToList();
        if (_selectedPlayer != null && !_playerList.Contains(_selectedPlayer))
            _selectedPlayer = null;

        FilterList();
    }


    private string GetText(PlayerInfo info)
    {
        var text = $"{info.CharacterName} ({info.Username})";
        if (OverrideText != null)
            text = OverrideText.Invoke(info, text);
        return text;
    }

    private void GenerateButton(ListData data, ListContainerButton button)
    {
        if (data is not PlayerListData { Info: var info })
            return;

        var entry = new PlayerListEntry();
        entry.Setup(info, OverrideText);
        entry.OnPinStatusChanged += _ =>
        {
            FilterList();
        };

        button.AddChild(entry);
        button.AddStyleClass(ListContainer.StyleClassListContainerButton);
    }
}

public record PlayerListData(PlayerInfo Info) : ListData;
