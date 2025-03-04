namespace Script
{
    using System;
    using System.Collections.Generic;

// Enum to define modes
    public enum Mode
    {
        Light,
        Dark
    }

// Class to manage the mode and colors
    public class ThemeManager
    {
        private Mode currentMode;
        private Dictionary<Mode, (string BackgroundColor, string TextColor)> colorSettings;

        public ThemeManager()
        {
            // Initialize with default mode
            currentMode = Mode.Light;

            // Define color settings for each mode
            colorSettings = new Dictionary<Mode, (string, string)>
            {
                { Mode.Light, ("#FFFFFF", "#000000") },  // White background, Black text
                { Mode.Dark, ("#000000", "#FFFFFF") }    // Black background, White text
            };
        }

        // Method to switch modes
        public void SwitchMode()
        {
            currentMode = currentMode == Mode.Light ? Mode.Dark : Mode.Light;
            Console.WriteLine($"Switched to {currentMode} Mode.");
        }

        // Get the current mode
        public Mode GetCurrentMode()
        {
            return currentMode;
        }

        // Get color settings for the current mode
        public (string BackgroundColor, string TextColor) GetCurrentColors()
        {
            return colorSettings[currentMode];
        }

        // Display current mode and color settings
        public void DisplayCurrentSettings()
        {
            var colors = GetCurrentColors();
            Console.WriteLine($"Current Mode: {currentMode}");
            Console.WriteLine($"Background Color: {colors.BackgroundColor}");
            Console.WriteLine($"Text Color: {colors.TextColor}");
        }
    }

    
}