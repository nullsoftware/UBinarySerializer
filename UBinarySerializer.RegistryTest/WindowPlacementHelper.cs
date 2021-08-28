using System;
using System.Runtime.InteropServices;
using Microsoft.Win32;

#pragma warning disable CA1416 // Validate platform compatibility

namespace NullSoftware.Serialization.RegistryTest
{
    public static class WindowPlacementHelper
    {
        // RECT structure required by WINDOWPLACEMENT structure
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public RECT(int left, int top, int right, int bottom)
            {
                this.Left = left;
                this.Top = top;
                this.Right = right;
                this.Bottom = bottom;
            }

            public override string ToString()
            {
                return $"{Left}, {Top}, {Right}, {Bottom}";
            }
        }

        // POINT structure required by WINDOWPLACEMENT structure
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public override string ToString()
            {
                return $"{X}, {Y}";
            }
        }

        // WINDOWPLACEMENT stores the position, size, and state of a window
        [Serializable]
        [StructLayout(LayoutKind.Sequential)]
        public struct WINDOWPLACEMENT
        {
            public int length;
            public int flags;
            public int showCmd;
            public POINT minPosition;
            public POINT maxPosition;
            public RECT normalPosition;
        }

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

        private const int SW_SHOWNORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;

        private static readonly BinarySerializer<WINDOWPLACEMENT> _serializer 
            = new BinarySerializer<WINDOWPLACEMENT>();

        private static readonly int _size = Marshal.SizeOf<WINDOWPLACEMENT>();

        public static void SetPlacement(IntPtr windowHandle, WINDOWPLACEMENT placement)
        {
            placement.length = _size;
            placement.flags = 0;
            placement.showCmd = (placement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : placement.showCmd);
            SetWindowPlacement(windowHandle, ref placement);
        }

        public static WINDOWPLACEMENT GetPlacement(IntPtr windowHandle)
        {
            WINDOWPLACEMENT wp;
            GetWindowPlacement(windowHandle, out wp);

            return wp;
        }

        public static void Serialize(RegistryKey key, string valueName, WINDOWPLACEMENT placement)
        {
            byte[] data = _serializer.SerializeUnsafe(placement);

            key.SetValue(valueName, data);
        }

        public static WINDOWPLACEMENT Deserialize(RegistryKey key, string valueName)
        {
            byte[] value = (byte[])key.GetValue(valueName);

            if (value.Length != _size)
                throw new InvalidOperationException($"Bytes count should equals {_size}.");

            return _serializer.DeserializeUnsafe(value);
        }
    }
}
