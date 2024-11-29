using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace CapsLockClicker
{
    public partial class MainForm : Form
    {
        private Random random = new Random();
        private System.Windows.Forms.Timer timer;

        public MainForm()
        {
            InitializeComponent();
            timer = new System.Windows.Forms.Timer();
            timer.Tick += Timer_Tick;
            SetNextInterval();
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ClickCapsLock();
            Thread.Sleep(1000); // �������� � 1 �������
            ClickCapsLock();
            SetNextInterval(); // ������������� ��������� �������� ����� �������
        }

        private void SetNextInterval()
        {
            int interval = random.Next(3 * 60 * 1000, 8 * 60 * 1000); // �� 3 �� 8 ����� � �������������
            timer.Interval = interval;
        }

        private void ClickCapsLock()
        {
            // ����� ������� Caps Lock
            keybd_event((byte)Keys.CapsLock, 0, 0, 0);
            keybd_event((byte)Keys.CapsLock, 0, 2, 0);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, int dwExtraInfo);
    }
}
