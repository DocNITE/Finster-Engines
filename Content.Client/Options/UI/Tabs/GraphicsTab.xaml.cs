using Content.Shared.CCVar;
using Robust.Client.AutoGenerated;
using Robust.Client.Graphics;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;
using Robust.Shared;
using Robust.Shared.Configuration;

namespace Content.Client.Options.UI.Tabs
{
    [GenerateTypedNameReferences]
    public sealed partial class GraphicsTab : Control
    {
        private static readonly float[] UIScaleOptions =
        {
            0f,
            0.75f,
            1f,
            1.25f,
            1.50f,
            1.75f,
            2f
        };

        [Dependency] private readonly IConfigurationManager _cfg = default!;

        public GraphicsTab()
        {
            IoCManager.InjectDependencies(this);
            RobustXamlLoader.Load(this);

            VSyncCheckBox.OnToggled += OnCheckBoxToggled;
            FullscreenCheckBox.OnToggled += OnCheckBoxToggled;

            LightingPresetOption.AddItem(Loc.GetString("ui-options-lighting-very-low"));
            LightingPresetOption.AddItem(Loc.GetString("ui-options-lighting-low"));
            LightingPresetOption.AddItem(Loc.GetString("ui-options-lighting-medium"));
            LightingPresetOption.AddItem(Loc.GetString("ui-options-lighting-high"));
            LightingPresetOption.OnItemSelected += OnLightingQualityChanged;

            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-auto",
                                                ("scale", UserInterfaceManager.DefaultUIScale)));
            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-75"));
            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-100"));
            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-125"));
            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-150"));
            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-175"));
            UIScaleOption.AddItem(Loc.GetString("ui-options-scale-200"));
            UIScaleOption.OnItemSelected += OnUIScaleChanged;

            ViewportStretchCheckBox.OnToggled += _ =>
            {
                UpdateViewportScale();
                UpdateApplyButton();
            };

            ViewportScaleSlider.OnValueChanged += _ =>
            {
                UpdateApplyButton();
                UpdateViewportScale();
            };

            ViewportVerticalFitCheckBox.OnToggled += _ =>
            {
                UpdateViewportScale();
                UpdateApplyButton();
            };

            IntegerScalingCheckBox.OnToggled += OnCheckBoxToggled;
            ViewportLowResCheckBox.OnToggled += OnCheckBoxToggled;
            ParallaxLowQualityCheckBox.OnToggled += OnCheckBoxToggled;
            FpsCounterCheckBox.OnToggled += OnCheckBoxToggled;
            ApplyButton.OnPressed += OnApplyButtonPressed;
            VSyncCheckBox.Pressed = _cfg.GetCVar(CVars.DisplayVSync);
            FullscreenCheckBox.Pressed = ConfigIsFullscreen;
            LightingPresetOption.SelectId(GetConfigLightingQuality());
            UIScaleOption.SelectId(GetConfigUIScalePreset(ConfigUIScale));
            ViewportScaleSlider.Value = _cfg.GetCVar(CCVars.ViewportFixedScaleFactor);
            ViewportStretchCheckBox.Pressed = _cfg.GetCVar(CCVars.ViewportStretch);
            IntegerScalingCheckBox.Pressed = _cfg.GetCVar(CCVars.ViewportSnapToleranceMargin) != 0;
            ViewportVerticalFitCheckBox.Pressed = _cfg.GetCVar(CCVars.ViewportVerticalFit);
            ViewportLowResCheckBox.Pressed = !_cfg.GetCVar(CCVars.ViewportScaleRender);
            ParallaxLowQualityCheckBox.Pressed = _cfg.GetCVar(CCVars.ParallaxLowQuality);
            FpsCounterCheckBox.Pressed = _cfg.GetCVar(CCVars.HudFpsCounterVisible);
            //ViewportWidthSlider.Value = _cfg.GetCVar(CCVars.ViewportWidth);

            _cfg.OnValueChanged(CCVars.ViewportMinimumWidth, _ => UpdateViewportWidthRange());
            _cfg.OnValueChanged(CCVars.ViewportMaximumWidth, _ => UpdateViewportWidthRange());

            UpdateViewportWidthRange();
            UpdateViewportWidthDisplay();
            UpdateViewportScale();
            UpdateApplyButton();
        }

        private void OnUIScaleChanged(OptionButton.ItemSelectedEventArgs args)
        {
            UIScaleOption.SelectId(args.Id);
            UpdateApplyButton();
        }

        private void OnApplyButtonPressed(BaseButton.ButtonEventArgs args)
        {
            _cfg.SetCVar(CVars.DisplayVSync, VSyncCheckBox.Pressed);
            SetConfigLightingQuality(LightingPresetOption.SelectedId);

            _cfg.SetCVar(CVars.DisplayWindowMode,
                         (int) (FullscreenCheckBox.Pressed ? WindowMode.Fullscreen : WindowMode.Windowed));
            _cfg.SetCVar(CVars.DisplayUIScale, UIScaleOptions[UIScaleOption.SelectedId]);
            _cfg.SetCVar(CCVars.ViewportStretch, ViewportStretchCheckBox.Pressed);
            _cfg.SetCVar(CCVars.ViewportFixedScaleFactor, (int) ViewportScaleSlider.Value);
            _cfg.SetCVar(CCVars.ViewportSnapToleranceMargin,
                         IntegerScalingCheckBox.Pressed ? CCVars.ViewportSnapToleranceMargin.DefaultValue : 0);
            _cfg.SetCVar(CCVars.ViewportVerticalFit, ViewportVerticalFitCheckBox.Pressed);
            _cfg.SetCVar(CCVars.ViewportScaleRender, !ViewportLowResCheckBox.Pressed);
            _cfg.SetCVar(CCVars.ParallaxLowQuality, ParallaxLowQualityCheckBox.Pressed);
            _cfg.SetCVar(CCVars.HudFpsCounterVisible, FpsCounterCheckBox.Pressed);
            //_cfg.SetCVar(CCVars.ViewportWidth, (int) ViewportWidthSlider.Value);

            _cfg.SaveToFile();
            UpdateApplyButton();
        }

        private void OnCheckBoxToggled(BaseButton.ButtonToggledEventArgs args)
        {
            UpdateApplyButton();
        }

        private void OnLightingQualityChanged(OptionButton.ItemSelectedEventArgs args)
        {
            LightingPresetOption.SelectId(args.Id);
            UpdateApplyButton();
        }

        private void UpdateApplyButton()
        {
            var isVSyncSame = VSyncCheckBox.Pressed == _cfg.GetCVar(CVars.DisplayVSync);
            var isFullscreenSame = FullscreenCheckBox.Pressed == ConfigIsFullscreen;
            var isLightingQualitySame = LightingPresetOption.SelectedId == GetConfigLightingQuality();
            var isUIScaleSame = MathHelper.CloseToPercent(UIScaleOptions[UIScaleOption.SelectedId], ConfigUIScale);
            var isVPStretchSame = ViewportStretchCheckBox.Pressed == _cfg.GetCVar(CCVars.ViewportStretch);
            var isVPScaleSame = (int) ViewportScaleSlider.Value == _cfg.GetCVar(CCVars.ViewportFixedScaleFactor);
            var isIntegerScalingSame = IntegerScalingCheckBox.Pressed == (_cfg.GetCVar(CCVars.ViewportSnapToleranceMargin) != 0);
            var isVPVerticalFitSame = ViewportVerticalFitCheckBox.Pressed == _cfg.GetCVar(CCVars.ViewportVerticalFit);
            var isVPResSame = ViewportLowResCheckBox.Pressed == !_cfg.GetCVar(CCVars.ViewportScaleRender);
            var isPLQSame = ParallaxLowQualityCheckBox.Pressed == _cfg.GetCVar(CCVars.ParallaxLowQuality);
            var isFpsCounterVisibleSame = FpsCounterCheckBox.Pressed == _cfg.GetCVar(CCVars.HudFpsCounterVisible);
            //var isWidthSame = (int) ViewportWidthSlider.Value == _cfg.GetCVar(CCVars.ViewportWidth);

            ApplyButton.Disabled = isVSyncSame &&
                                   isFullscreenSame &&
                                   isLightingQualitySame &&
                                   isUIScaleSame &&
                                   isVPStretchSame &&
                                   isVPScaleSame &&
                                   isIntegerScalingSame &&
                                   isVPVerticalFitSame &&
                                   isVPResSame &&
                                   isPLQSame &&
                                   isFpsCounterVisibleSame;
                                   //isWidthSame;
        }

        private bool ConfigIsFullscreen =>
            _cfg.GetCVar(CVars.DisplayWindowMode) == (int) WindowMode.Fullscreen;

        public void UpdateProperties()
        {
            FullscreenCheckBox.Pressed = ConfigIsFullscreen;
        }


        private float ConfigUIScale => _cfg.GetCVar(CVars.DisplayUIScale);

        private int GetConfigLightingQuality()
        {
            var val = _cfg.GetCVar(CVars.LightResolutionScale);
            var soft = _cfg.GetCVar(CVars.LightSoftShadows);
            if (val <= 0.125)
            {
                return 0;
            }
            else if ((val <= 0.5) && !soft)
            {
                return 1;
            }
            else if (val <= 0.5)
            {
                return 2;
            }
            else
            {
                return 3;
            }
        }

        private void SetConfigLightingQuality(int value)
        {
            switch (value)
            {
                case 0:
                    _cfg.SetCVar(CVars.LightResolutionScale, 0.125f);
                    _cfg.SetCVar(CVars.LightSoftShadows, false);
                    _cfg.SetCVar(CVars.LightBlur, false);
                    break;
                case 1:
                    _cfg.SetCVar(CVars.LightResolutionScale, 0.5f);
                    _cfg.SetCVar(CVars.LightSoftShadows, false);
                    _cfg.SetCVar(CVars.LightBlur, true);
                    break;
                case 2:
                    _cfg.SetCVar(CVars.LightResolutionScale, 0.5f);
                    _cfg.SetCVar(CVars.LightSoftShadows, true);
                    _cfg.SetCVar(CVars.LightBlur, true);
                    break;
                case 3:
                    _cfg.SetCVar(CVars.LightResolutionScale, 1);
                    _cfg.SetCVar(CVars.LightSoftShadows, true);
                    _cfg.SetCVar(CVars.LightBlur, true);
                    break;
            }
        }

        private static int GetConfigUIScalePreset(float value)
        {
            for (var i = 0; i < UIScaleOptions.Length; i++)
            {
                if (MathHelper.CloseToPercent(UIScaleOptions[i], value))
                {
                    return i;
                }
            }

            return 0;
        }

        private void UpdateViewportScale()
        {
            ViewportScaleBox.Visible = !ViewportStretchCheckBox.Pressed;
            IntegerScalingCheckBox.Visible = ViewportStretchCheckBox.Pressed;
            ViewportVerticalFitCheckBox.Visible = ViewportStretchCheckBox.Pressed;
            //ViewportWidthSlider.Visible = ViewportWidthSliderDisplay.Visible = !ViewportStretchCheckBox.Pressed || ViewportStretchCheckBox.Pressed && !ViewportVerticalFitCheckBox.Pressed;
            ViewportScaleText.Text = Loc.GetString("ui-options-vp-scale", ("scale", ViewportScaleSlider.Value));
        }

        private void UpdateViewportWidthRange()
        {
            var min = _cfg.GetCVar(CCVars.ViewportMinimumWidth);
            var max = _cfg.GetCVar(CCVars.ViewportMaximumWidth);

            //ViewportWidthSlider.MinValue = min;
            //ViewportWidthSlider.MaxValue = max;
        }

        private void UpdateViewportWidthDisplay()
        {
            //ViewportWidthSliderDisplay.Text = Loc.GetString("ui-options-vp-width", ("width", (int) ViewportWidthSlider.Value));
        }
    }
}
