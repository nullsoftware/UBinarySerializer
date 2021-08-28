using System;
using Microsoft.Win32;

#pragma warning disable CA1416 // Validate platform compatibility

namespace NullSoftware.Serialization.RegistryTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // for test was used WinRAR app
            // and it's registry-stored settings

            // [REMARK]: target app (WinRAR) should be closed
            // before run of current app, and opened after completion

            var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\WinRAR\Interface\MainWin", true); // registry key
            string valueName = "Placement"; // registry value name

            // get current saved placement of window
            var placement = WindowPlacementHelper.Deserialize(key, valueName);

            Console.WriteLine($"Original position: {placement.normalPosition}");

            // change placement to left top corner of the screen
            placement.normalPosition.Bottom = placement.normalPosition.Bottom - placement.normalPosition.Top;
            placement.normalPosition.Top = 0;

            placement.normalPosition.Right = placement.normalPosition.Right - placement.normalPosition.Left;
            placement.normalPosition.Left = 0;

            // save placement
            WindowPlacementHelper.Serialize(key, valueName, placement);

            Console.WriteLine($"New position: {placement.normalPosition}");

            Console.ReadKey(true);
        }
    }
}
