using System;

namespace TestPictureBrowser.Helpers
{
    /// <summary>
    /// Application path helper
    /// </summary>
    public static class AppHelper
    {
        /// <summary>
        /// Get current path of application
        /// </summary>
        public static string GetCurrentPath()
        {
            var currentPath = AppDomain.CurrentDomain.BaseDirectory;
            if (!string.IsNullOrEmpty(currentPath) && !currentPath.EndsWith("\\"))
            {
                currentPath += "\\";
            }
            return currentPath;
        }
    }
}