using System;
using UnityEngine;
using UnityEngine.UIElements;

namespace Zarus.UI
{
    /// <summary>
    /// Helper that instantiates and manages the shared settings panel UI.
    /// </summary>
    public class SettingsPanelView
    {
        private const string DefaultTemplateResourcePath = "UI/Layouts/Screens/SettingsPanel";
        private readonly VisualElement container;
        private readonly VisualElement root;
        private readonly Slider volumeSlider;
        private readonly Toggle fullscreenToggle;
        private readonly Button closeButton;
        private static VisualTreeAsset cachedDefaultTemplate;
        public event Action Closed;

        private SettingsPanelView(VisualElement container, VisualElement root)
        {
            this.container = container;
            this.root = root;

            volumeSlider = root.Q<Slider>("MasterVolumeSlider");
            fullscreenToggle = root.Q<Toggle>("FullscreenToggle");
            closeButton = root.Q<Button>("CloseSettingsButton");

            InitializeVolumeSlider();
            InitializeFullscreenToggle();
            InitializeCloseButton();
        }

        /// <summary>
        /// Builds a settings panel inside the provided container using the supplied template.
        /// </summary>
        public static SettingsPanelView Create(VisualElement container, VisualTreeAsset template)
        {
            if (container == null)
            {
                Debug.LogWarning("[SettingsPanelView] Cannot create settings panel without a container.");
                return null;
            }

            var resolvedTemplate = template ?? LoadDefaultTemplate();
            if (resolvedTemplate == null)
            {
                Debug.LogWarning("[SettingsPanelView] Settings panel template not assigned and no default template found.");
                return null;
            }

            container.Clear();
            var panelRoot = resolvedTemplate.Instantiate();
            container.Add(panelRoot);
            return new SettingsPanelView(container, panelRoot);
        }

        public bool IsVisible => container != null && !container.ClassListContains("hidden");

        public void Show()
        {
            container?.RemoveFromClassList("hidden");
        }

        public void Hide()
        {
            container?.AddToClassList("hidden");
        }

        public void Toggle()
        {
            if (IsVisible)
            {
                Hide();
            }
            else
            {
                Show();
            }
        }

        private void InitializeVolumeSlider()
        {
            if (volumeSlider == null)
            {
                return;
            }

            volumeSlider.lowValue = 0f;
            volumeSlider.highValue = 1f;
            volumeSlider.value = AudioListener.volume;
            volumeSlider.RegisterValueChangedCallback(evt =>
            {
                AudioListener.volume = evt.newValue;
            });
        }

        private void InitializeFullscreenToggle()
        {
            if (fullscreenToggle == null)
            {
                return;
            }

            fullscreenToggle.value = Screen.fullScreen;
            fullscreenToggle.RegisterValueChangedCallback(evt =>
            {
                Screen.fullScreen = evt.newValue;
            });
        }

        private void InitializeCloseButton()
        {
            if (closeButton == null)
            {
                return;
            }

            closeButton.clicked += () =>
            {
                Hide();
                Closed?.Invoke();
            };
        }

        private static VisualTreeAsset LoadDefaultTemplate()
        {
            if (cachedDefaultTemplate == null)
            {
                cachedDefaultTemplate = Resources.Load<VisualTreeAsset>(DefaultTemplateResourcePath);
                if (cachedDefaultTemplate == null)
                {
                    Debug.LogWarning($"[SettingsPanelView] Failed to load default template at Resources path '{DefaultTemplateResourcePath}'.");
                }
            }

            return cachedDefaultTemplate;
        }
    }
}
