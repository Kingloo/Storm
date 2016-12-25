using System;
using System.Windows;

namespace Storm.Extensions
{
    public static class WindowExt
    {
        public static void SetToMiddleOfScreen(this Window window)
        {
            if (window == null) { throw new ArgumentNullException(nameof(window)); }

            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowHeight = window.Height;
            window.Top = (screenHeight / 2) - (windowHeight / 2);

            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double windowWidth = window.Width;
            window.Left = (screenWidth / 2) - (windowWidth / 2);
        }
    }
}
