using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace YimInjector
{
    public class Injector
    {
        static readonly IntPtr INTPTR_ZERO = (IntPtr)0;

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr OpenProcess(uint dwDesiredAccess, int bInheritHandle, uint dwProcessId);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr VirtualAllocEx(IntPtr hProcess, IntPtr lpAddress, IntPtr dwSize, uint flAllocationType, uint flProtect);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern int WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] buffer, uint size, int lpNumberOfBytesWritten);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern IntPtr CreateRemoteThread(IntPtr hProcess, IntPtr lpThreadAttribute, IntPtr dwStackSize, IntPtr lpStartAddress,
            IntPtr lpParameter, uint dwCreationFlags, IntPtr lpThreadId);

        public static bool Inject(Process process, string dllPath)
        {
            uint processId = (uint)process.Id;
            IntPtr hndProc = OpenProcess((0x2 | 0x8 | 0x10 | 0x20 | 0x400), 1, processId);
            if (hndProc == INTPTR_ZERO)
            {
                return false;
            }
            IntPtr loadLibraryAddress = GetProcAddress(GetModuleHandle("kernel32.dll"), "LoadLibraryA");
            if (loadLibraryAddress == INTPTR_ZERO)
            {
                return false;
            }
            IntPtr address = VirtualAllocEx(hndProc, (IntPtr)null, (IntPtr)dllPath.Length, (0x1000 | 0x2000), 0x40);
            if (address == INTPTR_ZERO)
            {
                return false;
            }
            byte[] bytes = Encoding.ASCII.GetBytes(dllPath);
            if (WriteProcessMemory(hndProc, address, bytes, (uint)bytes.Length, 0) == 0)
            {
                return false;
            }
            if (CreateRemoteThread(hndProc, (IntPtr)null, INTPTR_ZERO, loadLibraryAddress, address, 0, (IntPtr)null) == INTPTR_ZERO)
            {
                return false;
            }
            CloseHandle(hndProc);
            return true;
        }
    }
}
