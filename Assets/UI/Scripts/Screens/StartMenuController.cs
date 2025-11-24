using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Zarus.UI
{
    /// <summary>
    /// Controls the Start scene menu.
    /// Handles button interactions for starting the game, tutorial visibility, and exiting.
    /// </summary>
    [RequireComponent(typeof(UIDocument))]
    public class StartMenuController : MonoBehaviour
    {
        private const string HiddenClassName = "hidden";

        [Header("Scenes")]
        [SerializeField]
        private string gameplaySceneName = "Main";

        [Header("Element Names")]
        [SerializeField]
        private string tutorialPanelName = "TutorialPanel";

        [SerializeField]
        private string settingsPanelHostName = "SettingsPanelHost";

        [Header("Settings Panel")]
        [SerializeField]
        private VisualTreeAsset settingsPanelAsset;

        private UIDocument uiDocument;
        private VisualElement root;
        private VisualElement menuPanel;
        private VisualElement tutorialPanel;
        private VisualElement settingsPanelHost;
        private Button startButton;
        private Button tutorialButton;
        private Button tutorialCloseButton;
        private Button settingsButton;
        private Button exitButton;
        private SettingsPanelView settingsPanelView;
        private bool isInitialized;

        private void Awake()
        {
            uiDocument = GetComponent<UIDocument>();
        }

        private void OnEnable()
        {
            TryInitialize();
        }

        private void Start()
        {
            TryInitialize();
        }

        private void OnDisable()
        {
            UnregisterCallbacks();
        }

        private void OnDestroy()
        {
            if (settingsPanelView != null)
            {
                settingsPanelView.Closed -= OnSettingsClosed;
            }
        }

        private void TryInitialize()
        {
            if (isInitialized)
            {
                return;
            }

            if (uiDocument == null || uiDocument.rootVisualElement == null)
            {
                return;
            }

            root = uiDocument.rootVisualElement;
            menuPanel = root.Q<VisualElement>("MenuPanel");
            tutorialPanel = root.Q<VisualElement>(tutorialPanelName);
            settingsPanelHost = root.Q<VisualElement>(settingsPanelHostName);

            startButton = root.Q<Button>("StartButton");
            tutorialButton = root.Q<Button>("TutorialButton");
            tutorialCloseButton = root.Q<Button>("CloseTutorialButton");
            settingsButton = root.Q<Button>("SettingsButton");
            exitButton = root.Q<Button>("ExitButton");

            EnsureSettingsPanel();
            ActivateMenuPanelAnimation();
            RegisterCallbacks();
            isInitialized = true;
        }

        private void EnsureSettingsPanel()
        {
            if (settingsPanelView != null || settingsPanelHost == null)
            {
                return;
            }

            if (settingsPanelAsset == null)
            {
                Debug.LogWarning("[StartMenu] Settings panel asset not assigned. Using default template.");
            }

            settingsPanelView = SettingsPanelView.Create(settingsPanelHost, settingsPanelAsset);
            if (settingsPanelView != null)
            {
                settingsPanelView.Closed += OnSettingsClosed;
            }
        }

        private void RegisterCallbacks()
        {
            if (startButton != null)
            {
                startButton.clicked += OnStartClicked;
            }

            if (tutorialButton != null)
            {
                tutorialButton.clicked += ShowTutorial;
            }

            if (tutorialCloseButton != null)
            {
                tutorialCloseButton.clicked += HideTutorial;
            }

            if (settingsButton != null)
            {
                settingsButton.clicked += OnSettingsClicked;
            }

            if (exitButton != null)
            {
                exitButton.clicked += QuitGame;
            }
        }

        private void UnregisterCallbacks()
        {
            if (startButton != null)
            {
                startButton.clicked -= OnStartClicked;
            }

            if (tutorialButton != null)
            {
                tutorialButton.clicked -= ShowTutorial;
            }

            if (tutorialCloseButton != null)
            {
                tutorialCloseButton.clicked -= HideTutorial;
            }

            if (settingsButton != null)
            {
                settingsButton.clicked -= OnSettingsClicked;
            }

            if (exitButton != null)
            {
                exitButton.clicked -= QuitGame;
            }
        }

        private void OnStartClicked()
        {
            if (string.IsNullOrEmpty(gameplaySceneName))
            {
                Debug.LogWarning("[StartMenu] Gameplay scene name not set.");
                return;
            }

            SceneManager.LoadScene(gameplaySceneName);
        }

        private void ShowTutorial()
        {
            if (tutorialPanel == null)
            {
                return;
            }

            HideSettingsPanel();
            tutorialPanel.RemoveFromClassList(HiddenClassName);
            UpdateMenuVisibility();
        }

        private void HideTutorial()
        {
            if (tutorialPanel == null)
            {
                return;
            }

            tutorialPanel.AddToClassList(HiddenClassName);
            UpdateMenuVisibility();
        }

        private void OnSettingsClicked()
        {
            HideTutorial();

            if (settingsPanelView != null)
            {
                settingsPanelView.Toggle();
                UpdateMenuVisibility();
                return;
            }

            Debug.LogWarning("[StartMenu] Settings panel asset missing.");
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
            UpdateMenuVisibility();
        }

        private void QuitGame()
        {
            Debug.Log("[StartMenu] Exit clicked.");
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnSettingsClosed()
        {
            UpdateMenuVisibility();
        }

        private void ActivateMenuPanelAnimation()
        {
            if (menuPanel == null)
            {
                return;
            }

            // Menu panel uses slide-up class which is hidden by default without the active modifier.
            if (!menuPanel.ClassListContains("slide-up--active"))
            {
                menuPanel.AddToClassList("slide-up--active");
            }
        }

        private bool IsTutorialVisible => tutorialPanel != null && !tutorialPanel.ClassListContains(HiddenClassName);

        private bool IsSettingsVisible
        {
            get
            {
                if (settingsPanelView != null)
                {
                    return settingsPanelView.IsVisible;
                }

                if (settingsPanelHost != null)
                {
                    return !settingsPanelHost.ClassListContains(HiddenClassName);
                }

                return false;
            }
        }

        private void UpdateMenuVisibility()
        {
            if (menuPanel == null)
            {
                return;
            }

            bool hideMenu = IsTutorialVisible || IsSettingsVisible;
            if (hideMenu)
            {
                menuPanel.AddToClassList(HiddenClassName);
            }
            else
            {
                menuPanel.RemoveFromClassList(HiddenClassName);
            }
        }

        private void HideSettingsPanel()
        {
            if (settingsPanelView != null)
            {
                if (settingsPanelView.IsVisible)
                {
                    settingsPanelView.Hide();
                }
                return;
            }

            settingsPanelHost?.AddToClassList(HiddenClassName);
        }
    }
}
