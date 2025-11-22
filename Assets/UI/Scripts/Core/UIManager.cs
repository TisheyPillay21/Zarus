using UnityEngine;
using UnityEngine.InputSystem;

namespace Zarus.UI
{
    /// <summary>
    /// Central manager for all UI screens in the game.
    /// Singleton pattern ensures only one instance exists.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager instance;
        public static UIManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<UIManager>();
                    if (instance == null)
                    {
                        Debug.LogError("[UIManager] No UIManager found in scene. Please add one.");
                    }
                }
                return instance;
            }
        }

        [Header("UI Screens")]
        [SerializeField]
        private PauseMenu pauseMenu;

        [SerializeField]
        private GameHUD gameHUD;

        [Header("Input")]
        [SerializeField]
        private InputActionAsset inputActions;

        private InputAction pauseAction;
        private InputActionMap playerActionMap;
        private InputActionMap uiActionMap;

        private bool isPaused;
        public bool IsPaused => isPaused;

        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            // Initialize input system
            InitializeInput();
        }

        private void OnEnable()
        {
            if (pauseAction != null)
            {
                pauseAction.Enable();
                pauseAction.performed += OnPausePerformed;
            }
        }

        private void OnDisable()
        {
            if (pauseAction != null)
            {
                pauseAction.performed -= OnPausePerformed;
                pauseAction.Disable();
            }
        }

        private void Start()
        {
            // Show HUD on game start
            if (gameHUD != null)
            {
                gameHUD.Show();
            }
        }

        private void InitializeInput()
        {
            if (inputActions == null)
            {
                Debug.LogWarning("[UIManager] InputActionAsset not assigned. Looking for default...");
                inputActions = Resources.Load<InputActionAsset>("InputSystem_Actions");
            }

            if (inputActions != null)
            {
                playerActionMap = inputActions.FindActionMap("Player");
                uiActionMap = inputActions.FindActionMap("UI");

                // Listen for pause input (ESC key)
                pauseAction = inputActions.FindAction("UI/Cancel");
                
                if (pauseAction == null)
                {
                    Debug.LogWarning("[UIManager] Pause action not found in Input System.");
                }

                // Enable player controls by default
                playerActionMap?.Enable();
            }
        }

        private void OnPausePerformed(InputAction.CallbackContext context)
        {
            TogglePause();
        }

        /// <summary>
        /// Toggles the pause state of the game.
        /// </summary>
        public void TogglePause()
        {
            if (isPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        /// <summary>
        /// Pauses the game and shows the pause menu.
        /// </summary>
        public void Pause()
        {
            if (isPaused) return;

            isPaused = true;
            Time.timeScale = 0f;

            // Switch input to UI mode
            playerActionMap?.Disable();
            uiActionMap?.Enable();

            // Show pause menu
            if (pauseMenu != null)
            {
                pauseMenu.Show();
            }

            Debug.Log("[UIManager] Game paused.");
        }

        /// <summary>
        /// Resumes the game and hides the pause menu.
        /// </summary>
        public void Resume()
        {
            if (!isPaused) return;

            isPaused = false;
            Time.timeScale = 1f;

            // Switch input back to player mode
            uiActionMap?.Disable();
            playerActionMap?.Enable();

            // Hide pause menu
            if (pauseMenu != null)
            {
                pauseMenu.Hide();
            }

            Debug.Log("[UIManager] Game resumed.");
        }

        /// <summary>
        /// Quits the game application.
        /// </summary>
        public void QuitGame()
        {
            Debug.Log("[UIManager] Quitting game...");

#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        /// <summary>
        /// Shows the game HUD.
        /// </summary>
        public void ShowHUD()
        {
            gameHUD?.Show();
        }

        /// <summary>
        /// Hides the game HUD.
        /// </summary>
        public void HideHUD()
        {
            gameHUD?.Hide();
        }
    }
}
