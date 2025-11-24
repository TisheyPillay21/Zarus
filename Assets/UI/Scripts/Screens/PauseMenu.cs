using UnityEngine;
using UnityEngine.UIElements;

namespace Zarus.UI
{
    /// <summary>
    /// Controls the pause menu screen.
    /// </summary>
    public class PauseMenu : UIScreen
    {
        private const string HiddenClassName = "hidden";

        [Header("Settings Panel")]
        [SerializeField]
        private VisualTreeAsset settingsPanelAsset;

        private Button resumeButton;
        private Button settingsButton;
        private Button quitButton;
        private VisualElement menuPanel;
        private VisualElement settingsPanelHost;
        private SettingsPanelView settingsPanel;

        protected override void Initialize()
        {
            // Query UI elements
            resumeButton = Query<Button>("ResumeButton");
            settingsButton = Query<Button>("SettingsButton");
            quitButton = Query<Button>("QuitButton");
            menuPanel = Query<VisualElement>("MenuPanel");
            settingsPanelHost = Query<VisualElement>("SettingsPanelHost");

            if (settingsPanelHost != null)
            {
                if (settingsPanelAsset == null)
                {
                    Debug.LogWarning("[PauseMenu] Settings panel asset not assigned. Using default template.");
                }

                settingsPanel = SettingsPanelView.Create(settingsPanelHost, settingsPanelAsset);
                if (settingsPanel != null)
                {
                    settingsPanel.Closed += OnSettingsClosed;
                }
            }
            else
            {
                Debug.LogWarning("[PauseMenu] Settings panel host missing.");
            }

            // Register button callbacks
            if (resumeButton != null)
            {
                resumeButton.clicked += OnResumeClicked;
            }

            if (settingsButton != null)
            {
                settingsButton.clicked += OnSettingsClicked;
            }

            if (quitButton != null)
            {
                quitButton.clicked += OnQuitClicked;
            }
        }

        protected override void OnShow()
        {
            base.OnShow();

            // Animate panel in
            if (menuPanel != null)
            {
                menuPanel.AddToClassList("slide-up--active");
            }

            UpdateSettingsVisibility();
        }

        protected override void OnHide()
        {
            base.OnHide();

            // Remove animation class
            if (menuPanel != null)
            {
                menuPanel.RemoveFromClassList("slide-up--active");
                menuPanel.RemoveFromClassList(HiddenClassName);
            }

            EnsureSettingsPanelHidden();
            UIManager.Instance?.ShowHUD();
        }

        private void OnResumeClicked()
        {
            Debug.Log("[PauseMenu] Resume clicked.");
            UIManager.Instance?.Resume();
        }

        private void OnSettingsClicked()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Toggle();
                UpdateSettingsVisibility();
            }
            else
            {
                Debug.LogWarning("[PauseMenu] Settings panel unavailable.");
                ToggleFallbackSettingsHost();
            }
        }

        private void OnQuitClicked()
        {
            Debug.Log("[PauseMenu] Quit to menu clicked.");
            UIManager.Instance?.ReturnToMenu();
        }

        private void OnDestroy()
        {
            if (settingsPanel != null)
            {
                settingsPanel.Closed -= OnSettingsClosed;
            }
        }

        private void OnSettingsClosed()
        {
            UpdateSettingsVisibility();
        }

        private void UpdateSettingsVisibility()
        {
            bool settingsVisible = IsSettingsVisible();

            if (menuPanel != null)
            {
                if (settingsVisible)
                {
                    menuPanel.AddToClassList(HiddenClassName);
                }
                else
                {
                    menuPanel.RemoveFromClassList(HiddenClassName);
                }
            }

            if (settingsVisible)
            {
                UIManager.Instance?.HideHUD();
            }
            else
            {
                UIManager.Instance?.ShowHUD();
            }
        }

        private bool IsSettingsVisible()
        {
            if (settingsPanel != null)
            {
                return settingsPanel.IsVisible;
            }

            if (settingsPanelHost != null)
            {
                return !settingsPanelHost.ClassListContains(HiddenClassName);
            }

            return false;
        }

        private void ToggleFallbackSettingsHost()
        {
            if (settingsPanelHost == null)
            {
                return;
            }

            bool isHidden = settingsPanelHost.ClassListContains(HiddenClassName);
            if (isHidden)
            {
                settingsPanelHost.RemoveFromClassList(HiddenClassName);
            }
            else
            {
                settingsPanelHost.AddToClassList(HiddenClassName);
            }

            UpdateSettingsVisibility();
        }

        private void EnsureSettingsPanelHidden()
        {
            if (settingsPanel != null && settingsPanel.IsVisible)
            {
                settingsPanel.Hide();
            }

            if (settingsPanelHost != null)
            {
                settingsPanelHost.AddToClassList(HiddenClassName);
            }
        }
    }
}
