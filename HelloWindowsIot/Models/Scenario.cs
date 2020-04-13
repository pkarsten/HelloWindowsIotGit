using System;

namespace HelloWindowsIot
{
    /// <summary>
    /// A Scenario Represents a ViewModel
    /// </summary>
    public class Scenario
    {
        /// <summary>
        /// Scenario Title for the Menu/Navigation Pane
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// ClassType represents a ViewModel Page
        /// </summary>
        public Type ClassType { get; set; }
        
        /// <summary>
        /// GlyphChar Icons for the Menu/Navigation Pane
        /// </summary>
        public string GlyphChar { get; set; } 
    }
}
