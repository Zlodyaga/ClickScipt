using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CapsLockClicker
{
    public partial class MainForm : Form
    {
        private Random random = new Random();
        private System.Windows.Forms.Timer timer;
        private bool isPaused = false;
        private IntPtr _keyboardHookID = IntPtr.Zero;
        private IntPtr _mouseHookID = IntPtr.Zero;
        private LowLevelKeyboardProc _keyboardProc;
        private LowLevelMouseProc _mouseProc;

        public MainForm()
        {
            InitializeComponent();

            // ������������� �����
            _keyboardProc = HookCallback;
            _mouseProc = MouseHookCallback;
            _keyboardHookID = SetHook(_keyboardProc);
            _mouseHookID = SetMouseHook(_mouseProc);

            // ��������� �������
            timer = new System.Windows.Forms.Timer();
            timer.Tick += Timer_Tick;
            SetNextInterval();
            timer.Start();

            LogDebug("���������� ��������.");
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (isPaused) return; // ���� ����� ������� � �� ��������� �����

            ClickCapsLock();
            Task.Delay(1000).Wait();
            ClickCapsLock();

            LogDebug("Caps Lock ��� ������ �����.");

            SetNextInterval(); // ������������� ��������� ��������
        }

        private void SetNextInterval()
        {
            int interval = random.Next(3 * 60 * 1000, 8 * 60 * 1000); // �� 3 �� 8 ����� � �������������
            timer.Interval = interval;
            LogDebug($"��������� ���� ����� {interval / (60 * 1000)} �����. ({interval / 1000} ������)");
        }

        private void ClickCapsLock()
        {
            keybd_event((byte)Keys.CapsLock, 0, 0, 0);
            keybd_event((byte)Keys.CapsLock, 0, 2, 0);
        }

        // ��������� �����
        private IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private IntPtr SetMouseHook(LowLevelMouseProc proc)
        {
            using (var curProcess = System.Diagnostics.Process.GetCurrentProcess())
            using (var curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        // ������� ��� ����������
        private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                PauseAndResetTimer();
            }
            return CallNextHookEx(_keyboardHookID, nCode, wParam, lParam);
        }

        // ������� ��� ���� (������ �����!)
        private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && (wParam == (IntPtr)0x201 || wParam == (IntPtr)0x204)) // WM_LBUTTONDOWN (0x201) ��� WM_RBUTTONDOWN (0x204)
            {
                PauseAndResetTimer();
            }
            return CallNextHookEx(_mouseHookID, nCode, wParam, lParam);
        }
        
        private async void PauseAndResetTimer()
        {
            if (!isPaused)
            {
                isPaused = true;
                timer.Stop();
                LogDebug("���������� ���������� (������� ��� ����). ������ ������������� �� 1,5 ������...");

                await Task.Delay(90 * 1000); // ����������� ����� � 90 ������

                isPaused = false;
                timer.Start();
                SetNextInterval();
                LogDebug("������ ����������.");
            }
        }


        // ����� ��� ����������� � Debug
        private void LogDebug(string message)
        {
            Debug.WriteLine($"{DateTime.Now:HH:mm:ss} - {message}");
        }

        // ������� WinAPI
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
    }
}
