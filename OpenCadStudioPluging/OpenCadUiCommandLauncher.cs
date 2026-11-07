using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace OpenCadStudioPluging
{
    internal static class OpenCadUiCommandLauncher
    {
        private const uint InputKeyboard = 1;
        private const uint KeyEventFKeyUp = 0x0002;
        private const uint KeyEventFUnicode = 0x0004;
        private const int SwRestore = 9;
        private const uint SwpNoSize = 0x0001;
        private const uint SwpNoMove = 0x0002;
        private const uint SwpShowWindow = 0x0040;
        private static readonly IntPtr HwndTop = IntPtr.Zero;
        private const ushort VkEscape = 0x1B;
        private const ushort VkReturn = 0x0D;

        public static void RunInActiveWindow(string command)
        {
            if (string.IsNullOrWhiteSpace(command))
                throw new InvalidOperationException("Comando CAD vacio.");

            var window = FindOpenCadMainWindow();
            if (window == IntPtr.Zero)
                throw new InvalidOperationException("No se encontro una ventana principal de OpenCADStudio.");

            EnsureForegroundWindow(window);

            Thread.Sleep(80);
            if (!TrySendViaInput(command.Trim(), out var inputError))
            {
                if (!TrySendViaPostMessage(window, command.Trim(), out var postMessageError))
                {
                    var detail = "No se pudieron enviar las teclas a OpenCADStudio.";
                    if (!string.IsNullOrWhiteSpace(inputError) || !string.IsNullOrWhiteSpace(postMessageError))
                        detail += " SendInput: " + (inputError ?? "n/a") + " | PostMessage: " + (postMessageError ?? "n/a");
                    throw new InvalidOperationException(detail);
                }
            }
        }

        private static IntPtr FindOpenCadMainWindow()
        {
            Process[] processes = Process.GetProcessesByName("OpenCADStudio");
            IntPtr fallback = IntPtr.Zero;
            foreach (var process in processes)
            {
                try
                {
                    var handle = process.MainWindowHandle;
                    if (handle == IntPtr.Zero)
                        continue;

                    if (!IsWindowVisible(handle))
                        continue;

                    var title = process.MainWindowTitle ?? string.Empty;
                    if (title.IndexOf("Open CAD Studio", StringComparison.OrdinalIgnoreCase) >= 0)
                        return handle;

                    if (fallback == IntPtr.Zero)
                        fallback = handle;
                }
                catch
                {
                }
            }
            return fallback;
        }

        private static void EnsureForegroundWindow(IntPtr window)
        {
            if (window == IntPtr.Zero)
                throw new InvalidOperationException("Ventana OpenCADStudio invalida.");

            if (IsIconic(window))
                ShowWindow(window, SwRestore);

            var currentForeground = GetForegroundWindow();
            var currentThread = GetCurrentThreadId();
            var targetThread = GetWindowThreadProcessId(window, out _);
            var foregroundThread = currentForeground == IntPtr.Zero
                ? 0u
                : GetWindowThreadProcessId(currentForeground, out _);

            try
            {
                if (foregroundThread != 0 && foregroundThread != currentThread)
                    AttachThreadInput(currentThread, foregroundThread, true);
                if (targetThread != 0 && targetThread != currentThread)
                    AttachThreadInput(currentThread, targetThread, true);

                SetWindowPos(window, HwndTop, 0, 0, 0, 0, SwpNoMove | SwpNoSize | SwpShowWindow);
                SetForegroundWindow(window);
                BringWindowToTop(window);
                SetFocus(window);
            }
            finally
            {
                if (foregroundThread != 0 && foregroundThread != currentThread)
                    AttachThreadInput(currentThread, foregroundThread, false);
                if (targetThread != 0 && targetThread != currentThread)
                    AttachThreadInput(currentThread, targetThread, false);
            }

            if (GetForegroundWindow() != window)
                throw new InvalidOperationException("No se pudo enfocar OpenCADStudio para ejecutar el comando.");
        }

        private static void SendUnicodeText(string text)
        {
            foreach (var ch in text)
            {
                SendUnicodeChar(ch);
            }
        }

        private static void SendUnicodeChar(char ch)
        {
            var down = new INPUT
            {
                type = InputKeyboard,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = 0,
                        wScan = ch,
                        dwFlags = KeyEventFUnicode,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            var up = down;
            up.U.ki.dwFlags = KeyEventFUnicode | KeyEventFKeyUp;
            if (!TrySendInputs(new[] { down, up }))
                throw new InvalidOperationException("No se pudo enviar caracter unicode.");
        }

        private static void SendVirtualKey(ushort key)
        {
            var down = new INPUT
            {
                type = InputKeyboard,
                U = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = key,
                        wScan = 0,
                        dwFlags = 0,
                        time = 0,
                        dwExtraInfo = IntPtr.Zero
                    }
                }
            };
            var up = down;
            up.U.ki.dwFlags = KeyEventFKeyUp;
            if (!TrySendInputs(new[] { down, up }))
                throw new InvalidOperationException("No se pudo enviar tecla virtual.");
        }

        private static bool TrySendInputs(INPUT[] inputs)
        {
            if (inputs == null || inputs.Length == 0)
                return false;
            var sent = SendInput((uint)inputs.Length, inputs, Marshal.SizeOf(typeof(INPUT)));
            if (sent == inputs.Length)
                return true;
            var code = Marshal.GetLastWin32Error();
            var message = new StringBuilder(256);
            if (FormatMessage(FormatMessageFromSystem | FormatMessageIgnoreInserts, IntPtr.Zero, (uint)code, 0, message, message.Capacity, IntPtr.Zero) > 0)
                throw new InvalidOperationException("SendInput fallo (Win32=" + code + "): " + message.ToString().Trim());
            throw new InvalidOperationException("SendInput fallo (Win32=" + code + ").");
        }

        private static bool TrySendViaInput(string command, out string error)
        {
            error = null;
            try
            {
                SendVirtualKey(VkEscape);
                SendVirtualKey(VkEscape);
                SendUnicodeText(command);
                SendVirtualKey(VkReturn);
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TrySendViaPostMessage(IntPtr window, string command, out string error)
        {
            error = null;
            if (window == IntPtr.Zero)
            {
                error = "ventana invalida";
                return false;
            }

            try
            {
                if (!TryPostMessage(window, WmKeyDown, (IntPtr)VkEscape, "ESC down", out error)) return false;
                if (!TryPostMessage(window, WmKeyUp, (IntPtr)VkEscape, "ESC up", out error)) return false;
                if (!TryPostMessage(window, WmKeyDown, (IntPtr)VkEscape, "ESC down", out error)) return false;
                if (!TryPostMessage(window, WmKeyUp, (IntPtr)VkEscape, "ESC up", out error)) return false;

                foreach (var ch in command)
                {
                    if (!TryPostMessage(window, WmChar, (IntPtr)ch, "CHAR " + ch, out error))
                        return false;
                }

                if (!TryPostMessage(window, WmKeyDown, (IntPtr)VkReturn, "ENTER down", out error)) return false;
                if (!TryPostMessage(window, WmKeyUp, (IntPtr)VkReturn, "ENTER up", out error)) return false;
                return true;
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }
        }

        private static bool TryPostMessage(IntPtr window, uint message, IntPtr wParam, string stage, out string error)
        {
            error = null;
            if (PostMessage(window, message, wParam, IntPtr.Zero))
                return true;
            error = stage + " fallo (Win32=" + Marshal.GetLastWin32Error() + ")";
            return false;
        }

        private const uint WmKeyDown = 0x0100;
        private const uint WmKeyUp = 0x0101;
        private const uint WmChar = 0x0102;

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern bool BringWindowToTop(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr SetFocus(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool SetWindowPos(
            IntPtr hWnd,
            IntPtr hWndInsertAfter,
            int X,
            int Y,
            int cx,
            int cy,
            uint uFlags);

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);

        [DllImport("kernel32.dll")]
        private static extern uint GetCurrentThreadId();

        [DllImport("user32.dll")]
        private static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint SendInput(uint nInputs, INPUT[] pInputs, int cbSize);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool PostMessage(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern int FormatMessage(
            uint dwFlags,
            IntPtr lpSource,
            uint dwMessageId,
            uint dwLanguageId,
            StringBuilder lpBuffer,
            int nSize,
            IntPtr Arguments);

        private const uint FormatMessageFromSystem = 0x00001000;
        private const uint FormatMessageIgnoreInserts = 0x00000200;

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion U;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }
    }
}
