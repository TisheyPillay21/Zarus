using UnityEngine;
using UnityEngine.UIElements;

namespace Zarus.UI
{
    /// <summary>
    /// Base class for all UI screens in the game.
    /// Handles showing/hiding and provides common functionality.
    /// </summary>
    public abstract class UIScreen : MonoBehaviour
    {
        [Header("UI Document")]
        [SerializeField]
        protected UIDocument uiDocument;

        [Header("Screen Settings")]
        [SerializeField]
        protected bool hideOnAwake = true;

        protected VisualElement rootElement;
        protected bool isInitialized;
        protected bool isVisible;

        public bool IsVisible => isVisible;

        protected virtual void Awake()
        {
            if (uiDocument == null)
            {
                uiDocument = GetComponent<UIDocument>();
            }

            if (uiDocument != null && uiDocument.rootVisualElement != null)
            {
                rootElement = uiDocument.rootVisualElement;
                Initialize();
                isInitialized = true;

                if (hideOnAwake)
                {
                    Hide();
                }
            }
        }

        protected virtual void OnEnable()
        {
            if (!isInitialized && uiDocument != null)
            {
                rootElement = uiDocument.rootVisualElement;
                Initialize();
                isInitialized = true;
            }
        }

        /// <summary>
        /// Override this to set up UI element references and event handlers.
        /// </summary>
        protected virtual void Initialize()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Shows the screen with optional animation.
        /// </summary>
        public virtual void Show()
        {
            if (rootElement == null) return;

            rootElement.style.display = DisplayStyle.Flex;
            isVisible = true;
            OnShow();
        }

        /// <summary>
        /// Hides the screen with optional animation.
        /// </summary>
        public virtual void Hide()
        {
            if (rootElement == null) return;

            rootElement.style.display = DisplayStyle.None;
            isVisible = false;
            OnHide();
        }

        /// <summary>
        /// Called when the screen is shown. Override for custom behavior.
        /// </summary>
        protected virtual void OnShow()
        {
        }

        /// <summary>
        /// Called when the screen is hidden. Override for custom behavior.
        /// </summary>
        protected virtual void OnHide()
        {
        }

        /// <summary>
        /// Finds a UI element by name. Helper method.
        /// </summary>
        protected T Query<T>(string elementName = null) where T : VisualElement
        {
            return rootElement?.Q<T>(elementName);
        }

        /// <summary>
        /// Registers a button click handler.
        /// </summary>
        protected void RegisterButtonCallback(string buttonName, System.Action callback)
        {
            var button = Query<Button>(buttonName);
            if (button != null)
            {
                button.clicked += callback;
            }
            else
            {
                Debug.LogWarning($"[{GetType().Name}] Button '{buttonName}' not found.");
            }
        }
    }
}
