using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Morser
{
    class InterceptKeys : IDisposable
    {
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const int WM_MOUSEMOVE = 0x200;
        private const int WM_LBUTTONDOWN = 0x201;
        private const int WM_RBUTTONDOWN = 0x204;
        private const int WM_MBUTTONDOWN = 0x207;
        private const int WM_LBUTTONUP = 0x202;
        private const int WM_RBUTTONUP = 0x205;
        private const int WM_MBUTTONUP = 0x208;
        private const int WM_LBUTTONDBLCLK = 0x203;
        private const int WM_RBUTTONDBLCLK = 0x206;
        private const int WM_MBUTTONDBLCLK = 0x209;
        private const int WM_MOUSEWHEEL = 0x020A;


        private LowLevelProc keyboardProc;
        private IntPtr keyboardHookID = IntPtr.Zero;
        private IntPtr mouseHookID = IntPtr.Zero;

        private Dictionary<string, short> registeredHotkeys = new Dictionary<string, short>();
        private System.Windows.Forms.Form owner;

        public InterceptKeys(System.Windows.Forms.Form owner)
        {
            keyboardProc = KeyboardHookCallback;
            keyboardHookID = SetHook(keyboardProc, HookType.WH_KEYBOARD_LL);

            this.owner = owner;
        }

        // register a global hot key
        public void RegisterGlobalHotKey(Keys hotkey)
        {
            // Determine the identifier of this hotkey
            string hotkeyDefinition = hotkey.ToString();

            int mappedModifiers = 0;
            if ((hotkey & Keys.Control) == Keys.Control)
            { 
                mappedModifiers |= MOD_CONTROL;
                hotkey &= (~Keys.Control);
            }
            if ((hotkey & Keys.Alt) == Keys.Alt)
            {
                mappedModifiers |= MOD_ALT;
                hotkey &= (~Keys.Alt);
            }
            if ((hotkey & Keys.Shift) == Keys.Shift)
            {
                mappedModifiers |= MOD_SHIFT;
                hotkey &= (~Keys.Shift);
            }
            if ((hotkey & Keys.LWin) == Keys.LWin)
            {
                mappedModifiers |= MOD_WIN;
                hotkey &= (~Keys.LWin);
            }
            if ((hotkey & Keys.RWin) == Keys.RWin)
            {
                mappedModifiers |= MOD_WIN;
                hotkey &= (~Keys.RWin);
            }

            // If it was previously registered, unregister it
            if (registeredHotkeys.ContainsKey(hotkeyDefinition))
            {
                short oldHotkeyId = registeredHotkeys[hotkeyDefinition];
                UnregisterGlobalHotKey(oldHotkeyId);
            }

            // use the GlobalAddAtom API to get a unique ID
            string atomName = System.Threading.Thread.CurrentThread.ManagedThreadId.ToString("X8") + hotkeyDefinition;
            short hotkeyID = GlobalAddAtom(atomName);

            if (hotkeyID == 0)
            {
                throw new Exception("Unable to generate unique hotkey ID. Error code: " + Marshal.GetLastWin32Error().ToString());
            }

            // register the hotkey, throw if any error
            if (RegisterHotKey(owner.Handle, hotkeyID, mappedModifiers, (int)hotkey) == 0)
            {
                MessageBox.Show("Unable to register hotkey. Is another instance of Morser already running?", "Cannot register hotkey");
            }
            else
            {
                registeredHotkeys[hotkeyDefinition] = hotkeyID;
            }
        }

        public bool IsMessageForHotkey(short message, Keys hotkey)
        {
            // Determine the identifier of this hotkey
            string hotkeyDefinition = hotkey.ToString();

            // See if it is registered, and maps to this message
            if (registeredHotkeys.ContainsKey(hotkeyDefinition))
            {
                return message == registeredHotkeys[hotkeyDefinition];
            }

            return false;
        }

        // EventArgs used to support key change events
        public delegate void KeyChangeEventHandler(Object owner, KeyChangeEventArgs input);
        public class KeyChangeEventArgs
        {
            public KeyChangeEventArgs(uint vkCode)
            {
                this.vkCode = vkCode;
            }

            public uint VKCode
            {
                get { return vkCode; }
                set { vkCode = value; }
            }
            private uint vkCode = 0;

            public bool Cancel
            {
                get { return cancel; }
                set { cancel = value; }
            }
            private bool cancel = false;
        }

        public event KeyChangeEventHandler KeyDown;
        public event KeyChangeEventHandler KeyUp;

        private bool OnKeyDown(uint vkCode)
        {
            if (KeyDown != null)
            {
                KeyChangeEventArgs keyEventArgs = new KeyChangeEventArgs(vkCode);
                KeyDown(owner, keyEventArgs);

                if (keyEventArgs.Cancel)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        private bool OnKeyUp(uint vkCode)
        {
            if (KeyUp != null)
            {
                KeyChangeEventArgs keyEventArgs = new KeyChangeEventArgs(vkCode);
                KeyUp(owner, keyEventArgs);

                if (keyEventArgs.Cancel)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            else
            {
                return true;
            }
        }

        // unregister a global hotkey
        private void UnregisterGlobalHotKey(short hotkeyId)
        {
            if (hotkeyId != 0)
            {
                UnregisterHotKey(owner.Handle, hotkeyId);

                // clean up the atom list
                GlobalDeleteAtom(hotkeyId);
            }
        }

        private IntPtr SetHook(LowLevelProc proc, HookType type)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(type, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private IntPtr KeyboardHookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            bool callNextHook = true;
            KBDLLHOOKSTRUCT keyState = new KBDLLHOOKSTRUCT();
            Marshal.PtrToStructure(lParam, keyState);

            if(nCode >= 0)
            {
                // If it's a key down, send that message
                if ((wParam == (IntPtr)WM_KEYDOWN) ||
                    (wParam == (IntPtr)WM_SYSKEYDOWN))
                {
                    callNextHook = OnKeyDown(keyState.vkCode);
                }

                // If it's a key up, send that.
                if ((wParam == (IntPtr)WM_KEYUP) ||
                    (wParam == (IntPtr)WM_SYSKEYUP))
                {
                    callNextHook = OnKeyUp(keyState.vkCode);
                }
            }

            // If the action wanted to cancel the keypress, then don't send
            // on the event down the chain.
            if (callNextHook)
                return CallNextHookEx(keyboardHookID, nCode, wParam, lParam);
            else
            {
                return (IntPtr) (-1);
            }
        }

        public enum HookType : int
        {
            WH_JOURNALRECORD = 0,
            WH_JOURNALPLAYBACK = 1,
            WH_KEYBOARD = 2,
            WH_GETMESSAGE = 3,
            WH_CALLWNDPROC = 4,
            WH_CBT = 5,
            WH_SYSMSGFILTER = 6,
            WH_MOUSE = 7,
            WH_HARDWARE = 8,
            WH_DEBUG = 9,
            WH_SHELL = 10,
            WH_FOREGROUNDIDLE = 11,
            WH_CALLWNDPROCRET = 12,
            WH_KEYBOARD_LL = 13,
            WH_MOUSE_LL = 14
        }

        // Methods for low level keyboard hooks
        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(HookType hookType,
            LowLevelProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        // Methods for hotkeys
        // Windows API functions and constants
        [DllImport("user32", SetLastError = true)]
        private static extern int RegisterHotKey(IntPtr hwnd, int id, int fsModifiers, int vk);
        [DllImport("user32", SetLastError = true)]
        private static extern int UnregisterHotKey(IntPtr hwnd, int id);
        [DllImport("kernel32", SetLastError = true)]
        private static extern short GlobalAddAtom(string lpString);
        [DllImport("kernel32", SetLastError = true)]
        private static extern short GlobalDeleteAtom(short nAtom);

        [DllImport("user32.dll")]
        static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        static extern int ToUnicode(uint wVirtKey, uint wScanCode, byte[] lpKeyState,
           [Out, MarshalAs(UnmanagedType.LPWStr, SizeConst = 64)] StringBuilder pwszBuff, int cchBuff,
           uint wFlags);

        [StructLayout(LayoutKind.Sequential)]
        public class POINT
        {
            public int X;
            public int Y;
        }

        [StructLayout(LayoutKind.Sequential)]
        public class KBDLLHOOKSTRUCT
        {
            public uint vkCode;
            public uint scanCode;
            public KBDLLHOOKSTRUCTFlags flags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [Flags()]
        public enum KBDLLHOOKSTRUCTFlags : uint
        {
            LLKHF_EXTENDED = 0x01,
            LLKHF_INJECTED = 0x10,
            LLKHF_ALTDOWN = 0x20,
            LLKHF_UP = 0x80,
        }

        [StructLayout(LayoutKind.Sequential)]
        public class MouseHookStruct
        {
            public POINT Point;
            public UInt32 MouseData;
            public UInt32 Flags;
            public UInt32 Time;
            public IntPtr DwExtraInfo;
        }

        private const int MOD_ALT = 1;
        private const int MOD_CONTROL = 2;
        private const int MOD_SHIFT = 4;
        private const int MOD_WIN = 8;

        private const int VK_CONTROL = 0x11;
        private const int VK_MENU = 0x12;
        private const int VK_SHIFT = 0x10;

        #region IDisposable Members

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void Dispose(bool disposing)
        {
            if (disposing)
            {
                UnhookWindowsHookEx(keyboardHookID);
                UnhookWindowsHookEx(mouseHookID);

                foreach (short oldHotKeyId in registeredHotkeys.Values)
                {
                    UnregisterGlobalHotKey(oldHotKeyId);
                }
            }
        }

        #endregion
    }
}
