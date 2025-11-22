using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using Zarus.Map;

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

        // UI Elements
        private Label timerValue;
        private Label provincesValue;
        private Label gameSpeedValue;
        private Label provinceNameLabel;
        private Label provinceDescLabel;

        // Game State
        private float gameTime;
        private HashSet<string> visitedProvinces = new HashSet<string>();
        private int totalProvinces = 9; // South Africa has 9 provinces

        protected override void Initialize()
        {
            // Query UI elements
            timerValue = Query<Label>("TimerValue");
            provincesValue = Query<Label>("ProvincesValue");
            gameSpeedValue = Query<Label>("GameSpeedValue");
            provinceNameLabel = Query<Label>("ProvinceNameLabel");
            provinceDescLabel = Query<Label>("ProvinceDescLabel");

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

            // Initialize displays
            UpdateTimer();
            UpdateProvincesCounter();
        }

        private void Update()
        {
            // Update timer only when game is not paused
            if (Time.timeScale > 0)
            {
                gameTime += Time.deltaTime;
                UpdateTimer();
            }
        }

        private void UpdateTimer()
        {
            if (timerValue == null) return;

            int minutes = Mathf.FloorToInt(gameTime / 60f);
            int seconds = Mathf.FloorToInt(gameTime % 60f);
            timerValue.text = $"{minutes:00}:{seconds:00}";
        }

        private void UpdateProvincesCounter()
        {
            if (provincesValue == null) return;

            provincesValue.text = $"{visitedProvinces.Count} / {totalProvinces}";
        }

        private void OnProvinceHovered(RegionEntry region)
        {
            if (region == null) return;

            // Update bottom info bar with hovered province
            if (provinceNameLabel != null)
            {
                provinceNameLabel.text = region.DisplayName.ToUpper();
            }

            if (provinceDescLabel != null)
            {
                string desc = !string.IsNullOrEmpty(region.Description)
                    ? region.Description
                    : "Hover over to explore";
                provinceDescLabel.text = desc;
            }
        }

        private void OnProvinceSelected(RegionEntry region)
        {
            if (region == null) return;

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
            gameTime = 0f;
            UpdateTimer();
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
            return gameTime;
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
        }
    }
}
