﻿using System;
using System.Runtime.InteropServices;

namespace TaskbarCustomizer.Helpers {

    public class Utility {
        public delegate void WinEventDelegate(IntPtr hWinEventHook, uint eventType,
            IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

        public enum ABM : uint {
            New = 0x00000000,
            Remove = 0x00000001,
            QueryPos = 0x00000002,
            SetPos = 0x00000003,
            GetState = 0x00000004,
            GetTaskbarPos = 0x00000005,
            Activate = 0x00000006,
            GetAutoHideBar = 0x00000007,
            SetAutoHideBar = 0x00000008,
            WindowPosChanged = 0x00000009,
            SetState = 0x0000000A,
        }

        public const int WM_SIZE = 0x0005;

        public enum ABE : uint {
            Left = 0,
            Top = 1,
            Right = 2,
            Bottom = 3
        }

        public static class ABS {
            public const int Autohide = 0x0000001;
            public const int AlwaysOnTop = 0x0000002;
        }

        public const short SWP_NOMOVE = 0X2;
        public const short SWP_NOSIZE = 1;
        public const short SWP_NOZORDER = 0X4;
        public const short SWP_SHOWWINDOW = 0x0040;
        public const short SWP_NOACTIVATE = 0x0010;

        public const int SW_HIDE = 0;
        public const int SW_SHOW = 1;

        [DllImport("shell32.dll", SetLastError = true)]
        public static extern IntPtr SHAppBarMessage(ABM dwMessage, [In] ref APPBARDATA pData);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string lclassName, string windowTitle);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, int hWndInsertAfter, int x, int Y, int cx, int cy, int wFlags);

        [DllImport("user32.dll")]
        public static extern int ShowWindow(IntPtr hwnd, int command);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("user32.dll")]
        public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(uint eventMin, uint eventMax, IntPtr
            hmodWinEventProc, WinEventDelegate lpfnWinEventProc, uint idProcess,
            uint idThread, uint dwFlags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hWinEventHook);

        public const uint EVENT_MIN = 0x00000001;
        public const uint EVENT_MAX = 0x7FFFFFFF;
        public const uint WINEVENT_OUTOFCONTEXT = 0;
        public const uint WINEVENT_SKIPOWNPROCESS = 0x0002;
        public const uint EVENT_OBJECT_LOCATIONCHANGE = 0x800B;

        public const uint WM_DWMCOLORIZATIONCOLORCHANGED = 0x0320;
        public const uint WM_WINDOWPOSCHANGED = 0x47;
        public const uint WM_PAINT = 0x000F;
        public const uint WM_CHANGEUISTATE = 0x127;
        public const uint WM_STYLECHANGED = 0x007D;

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct APPBARDATA {
            public uint cbSize;
            public IntPtr hWnd;
            public uint uCallbackMessage;
            public ABE uEdge;
            public RECT rc;
            public int lParam;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowCompositionAttributeData {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        public enum WindowCompositionAttribute {

            // ...
            WCA_ACCENT_POLICY = 19

            // ...
        }

        public enum AccentState {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AccentPolicy {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }

        public static IntPtr FindWindowByIndex(IntPtr hWndParent, string className, int index) {
            if (index == 0)
                return hWndParent;
            else {
                int ct = 0;
                IntPtr result = IntPtr.Zero;
                do {
                    result = Utility.FindWindowEx(hWndParent, result, className, null);
                    if (result != IntPtr.Zero)
                        ++ct;
                }
                while (ct < index && result != IntPtr.Zero);
                return result;
            }
        }
    }
}