using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zarus.Map;
using Zarus.Systems;

namespace Zarus.UI
{
    /// <summary>
    /// Controls the in-game HUD display with timer, stats, and province info.
    /// </summary>
    public class GameHUD : UIScreen
    {
        [Header("Game References")]
        [SerializeField]
        private RegionMapController mapController;

        [Header("Time Source")]
        [SerializeField]
        private DayNightCycleController dayNightController;

        // UI Elements
        private Label timerValue;
        private Label provincesValue;
        private Label provinceNameLabel;
        private Label provinceDescLabel;

        // Game State
        private HashSet<string> visitedProvinces = new HashSet<string>();
        private int totalProvinces = 9; // South Africa has 9 provinces
        private RegionEntry selectedRegion;
        private InGameTimeSnapshot latestTimeSnapshot;
        private bool hasTimeSnapshot;

        protected override void Initialize()
        {
            // Ensure we have a valid document
            if (uiDocument == null)
            {
                Debug.LogError("[GameHUD] UIDocument is null! Assign it in the Inspector.");
                return;
            }
            
            var root = uiDocument.rootVisualElement;
            if (root == null)
            {
                Debug.LogError("[GameHUD] UIDocument root element is null!");
                return;
            }

            Debug.Log($"[GameHUD] Initializing... Root element: {root.name}, childCount: {root.childCount}");

            // Query UI elements directly from root
            timerValue = root.Q<Label>("TimerValue");
            provincesValue = root.Q<Label>("ProvincesValue");
            provinceNameLabel = root.Q<Label>("ProvinceNameLabel");
            provinceDescLabel = root.Q<Label>("ProvinceDescLabel");
            
            // Verify all elements were found
            Debug.Log($"[GameHUD] Elements found - TimerValue: {timerValue != null}, ProvincesValue: {provincesValue != null}, ProvinceNameLabel: {provinceNameLabel != null}, ProvinceDescLabel: {provinceDescLabel != null}");
            
            if (timerValue == null) Debug.LogError("[GameHUD] TimerValue not found in UXML!");
            if (provincesValue == null) Debug.LogError("[GameHUD] ProvincesValue not found in UXML!");
            if (provinceNameLabel == null) Debug.LogError("[GameHUD] ProvinceNameLabel not found in UXML!");
            if (provinceDescLabel == null) Debug.LogError("[GameHUD] ProvinceDescLabel not found in UXML!");

            // Force visibility on all elements
            if (timerValue != null)
            {
                timerValue.style.display = DisplayStyle.Flex;
                timerValue.style.visibility = Visibility.Visible;
                timerValue.style.opacity = 1f;
            }
            if (provincesValue != null)
            {
                provincesValue.style.display = DisplayStyle.Flex;
                provincesValue.style.visibility = Visibility.Visible;
                provincesValue.style.opacity = 1f;
            }
            if (provinceNameLabel != null)
            {
                provinceNameLabel.style.display = DisplayStyle.Flex;
                provinceNameLabel.style.visibility = Visibility.Visible;
                provinceNameLabel.style.opacity = 1f;
            }
            if (provinceDescLabel != null)
            {
                provinceDescLabel.style.display = DisplayStyle.Flex;
                provinceDescLabel.style.visibility = Visibility.Visible;
                provinceDescLabel.style.opacity = 1f;
            }

            // Find map controller if not assigned
            if (mapController == null)
            {
                mapController = FindFirstObjectByType<RegionMapController>();
            }

            // Subscribe to map events
            if (mapController != null)
            {
                mapController.OnRegionHovered.AddListener(OnProvinceHovered);
                mapController.OnRegionSelected.AddListener(OnProvinceSelected);
                totalProvinces = mapController.Entries.Count;
            }

            if (dayNightController == null)
            {
                dayNightController = FindFirstObjectByType<DayNightCycleController>();
            }

            if (dayNightController == null)
            {
                var bootstrapGo = new GameObject("DayNightCycleAuto");
                dayNightController = bootstrapGo.AddComponent<DayNightCycleController>();
            }

            if (dayNightController != null)
            {
                dayNightController.TimeUpdated += HandleTimeUpdated;
                if (dayNightController.HasTime)
                {
                    HandleTimeUpdated(dayNightController.CurrentTime);
                }
            }
            else
            {
                Debug.LogWarning("[GameHUD] DayNightCycleController not found; timer display will not reflect in-game time.");
            }

            // Initialize displays
            UpdateTimer();
            UpdateProvincesCounter();
            
            Debug.Log($"[GameHUD] Initialization complete. Timer text: '{timerValue?.text}', Provinces text: '{provincesValue?.text}', Timer visible: {timerValue?.visible}, Timer display: {timerValue?.style.display}");
        }

        private void UpdateTimer()
        {
            if (timerValue == null) return;

            if (hasTimeSnapshot)
            {
                var indicator = latestTimeSnapshot.GetIndicatorLabel();
                var formatted = latestTimeSnapshot.DateTime.ToString("MMM d, HH:mm");
                timerValue.text = $"{indicator} Day {latestTimeSnapshot.DayIndex} — {formatted}";
            }
            else
            {
                timerValue.text = "[SYNC] --:--";
            }
        }

        private void HandleTimeUpdated(InGameTimeSnapshot snapshot)
        {
            latestTimeSnapshot = snapshot;
            hasTimeSnapshot = true;
            UpdateTimer();
        }

        private void UpdateProvincesCounter()
        {
            if (provincesValue == null) return;

            provincesValue.text = $"{visitedProvinces.Count} / {totalProvinces}";
        }

        private void OnProvinceHovered(RegionEntry region)
        {
            if (region == null) return;

            // Only show hover when nothing is selected
            if (selectedRegion != null) return;

            if (provinceNameLabel != null)
                provinceNameLabel.text = region.DisplayName.ToUpper();

            if (provinceDescLabel != null)
                provinceDescLabel.text = !string.IsNullOrEmpty(region.Description)
                    ? region.Description
                    : "Hover over to explore";
        }

        private void OnProvinceSelected(RegionEntry region)
        {
            if (region == null) return;

            selectedRegion = region;

            // Mark province as visited
            if (!visitedProvinces.Contains(region.RegionId))
            {
                visitedProvinces.Add(region.RegionId);
                UpdateProvincesCounter();
                Debug.Log($"[GameHUD] Province visited: {region.DisplayName}");
            }

            // Update info display
            if (provinceNameLabel != null)
            {
                provinceNameLabel.text = $"★ {region.DisplayName.ToUpper()} ★";
            }

            if (provinceDescLabel != null)
            {
                string desc = !string.IsNullOrEmpty(region.Description)
                    ? region.Description
                    : "Selected province";
                provinceDescLabel.text = desc;
            }
        }

        /// <summary>
        /// Resets the game timer.
        /// </summary>
        public void ResetTimer()
        {
            dayNightController?.RestartCycle();
        }

        /// <summary>
        /// Resets the visited provinces counter.
        /// </summary>
        public void ResetVisitedProvinces()
        {
            visitedProvinces.Clear();
            UpdateProvincesCounter();
        }

        /// <summary>
        /// Gets the current game time in seconds.
        /// </summary>
        public float GetGameTime()
        {
            if (!hasTimeSnapshot)
            {
                return 0f;
            }

            return latestTimeSnapshot.TimeOfDayMinutes * 60f;
        }

        /// <summary>
        /// Gets the number of provinces visited.
        /// </summary>
        public int GetVisitedProvincesCount()
        {
            return visitedProvinces.Count;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (mapController != null)
            {
                mapController.OnRegionHovered.RemoveListener(OnProvinceHovered);
                mapController.OnRegionSelected.RemoveListener(OnProvinceSelected);
            }

            if (dayNightController != null)
            {
                dayNightController.TimeUpdated -= HandleTimeUpdated;
            }
        }
    }
}
