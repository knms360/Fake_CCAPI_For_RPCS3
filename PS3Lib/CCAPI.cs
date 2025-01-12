// ************************************************* //
//    --- Copyright (c) 2015 iMCS Productions ---    //
// ************************************************* //
//              PS3Lib v4 By FM|T iMCSx              //
//                                                   //
// Features v4.5 :                                   //
// - Support CCAPI v2.60+ C# by iMCSx.               //
// - Read/Write memory as 'double'.                  //
// - Read/Write memory as 'float' array.             //
// - Constructor overload for ArrayBuilder.          //
// - Some functions fixes.                           //
//                                                   //
// Credits : Enstone, Buc-ShoTz                      //
//                                                   //
// Follow me :                                       //
//                                                   //
// FrenchModdingTeam.com                             //
// Twitter.com/iMCSx                                 //
// Facebook.com/iMCSx                                //
//                                                   //
// ************************************************* //

using System;
using S = System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Microsoft.Win32;
using System.Security.Cryptography;
using Memory;
using System.Runtime.InteropServices.WindowsRuntime;

namespace PS3Lib
{
    public class CCAPI
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr LoadLibrary(string dllName);

        [DllImport("kernel32.dll")]
        static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int connectConsoleDelegate(string targetIP);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int disconnectConsoleDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getConnectionStatusDelegate(ref int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getConsoleInfoDelegate(int index, IntPtr ptrN, IntPtr ptrI);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getDllVersionDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getFirmwareInfoDelegate(ref int firmware, ref int ccapi, ref int consoleType);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getNumberOfConsolesDelegate();
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getProcessListDelegate(ref uint numberProcesses, IntPtr processIdPtr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getProcessMemoryDelegate(uint processID, ulong offset, uint size, byte[] buffOut);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getProcessNameDelegate(uint processID, IntPtr strPtr);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int getTemperatureDelegate(ref int cell, ref int rsx);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int notifyDelegate(int mode, string msgWChar);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int ringBuzzerDelegate(int type);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int setBootConsoleIdsDelegate(int idType, int on, byte[] ID);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int setConsoleIdsDelegate(int idType, byte[] consoleID);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int setConsoleLedDelegate(int color, int status);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int setProcessMemoryDelegate(uint processID, ulong offset, uint size, byte[] buffIn);
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int shutdownDelegate(int mode);

        private connectConsoleDelegate connectConsole;
        private disconnectConsoleDelegate disconnectConsole;
        private getConnectionStatusDelegate getConnectionStatus;
        private getConsoleInfoDelegate getConsoleInfo;
        private getDllVersionDelegate getDllVersion;
        private getFirmwareInfoDelegate getFirmwareInfo;
        private getNumberOfConsolesDelegate getNumberOfConsoles;
        private getProcessListDelegate getProcessList;
        private getProcessMemoryDelegate getProcessMemory;
        private getProcessNameDelegate getProcessName;
        private getTemperatureDelegate getTemperature;
        private notifyDelegate notify;
        private ringBuzzerDelegate ringBuzzer;
        private setBootConsoleIdsDelegate setBootConsoleIds;
        private setConsoleIdsDelegate setConsoleIds;
        private setConsoleLedDelegate setConsoleLed;
        private setProcessMemoryDelegate setProcessMemory;
        private shutdownDelegate shutdown;

        private IntPtr libModule = IntPtr.Zero;
        private List<IntPtr> CCAPIFunctionsList = new List<IntPtr>();

        private enum CCAPIFunctions
        {
            ConnectConsole,
            DisconnectConsole,
            GetConnectionStatus,
            GetConsoleInfo,
            GetDllVersion,
            GetFirmwareInfo,
            GetNumberOfConsoles,
            GetProcessList,
            GetMemory,
            GetProcessName,
            GetTemperature,
            VshNotify,
            RingBuzzer,
            SetBootConsoleIds,
            SetConsoleIds,
            SetConsoleLed,
            SetMemory,
            ShutDown
        }

        public static Mem mem = new Mem();
        private S::Media.SoundPlayer player = null;
        public CCAPI()
        {
            
        }

        public enum IdType
        {
            IDPS,
            PSID
        }

        public enum NotifyIcon
        {
            INFO,
            CAUTION,
            FRIEND,
            SLIDER,
            WRONGWAY,
            DIALOG,
            DIALOGSHADOW,
            TEXT,
            POINTER,
            GRAB,
            HAND,
            PEN,
            FINGER,
            ARROW,
            ARROWRIGHT,
            PROGRESS,
            TROPHY1,
            TROPHY2,
            TROPHY3,
            TROPHY4
        }

        public enum ConsoleType
        {
            CEX = 1,
            DEX = 2,
            TOOL = 3
        }

        public enum ProcessType
        {
            VSH,
            SYS_AGENT,
            CURRENTGAME
        }

        public enum RebootFlags
        {
            ShutDown = 1,
            SoftReboot = 2,
            HardReboot = 3
        }

        public enum BuzzerMode
        {
            Continuous,
            Single,
            Double,
            Triple
        }

        public enum LedColor
        {
            Green = 1,
            Red = 2
        }

        public enum LedMode
        {
            Off,
            On,
            Blink
        }

        private TargetInfo pInfo = new TargetInfo();

        private IntPtr ReadDataFromUnBufPtr<T>(IntPtr unBuf, ref T storage)
        {
            storage = (T)Marshal.PtrToStructure(unBuf, typeof(T));
            return new IntPtr(unBuf.ToInt64() + Marshal.SizeOf((T)storage));
        }

        private class System
        {
            public static int
                connectionID = -1;
            public static uint
                processID = 0;
            public static uint[]
                processIDs;
        }

        /// <summary>Get informations from your target.</summary>
        public class TargetInfo
        {
            public int
                Firmware = 0,
                CCAPI = 0,
                ConsoleType = 0,
                TempCell = 0,
                TempRSX = 0;
            public ulong
                SysTable = 0;
        }

        /// <summary>Get Info for targets.</summary>
        public class ConsoleInfo
        {
            public string
                Name,
                Ip;
        }

        public Extension Extension
        {
            get { return new Extension(SelectAPI.ControlConsole); }
        }

        private IntPtr GetCCAPIFunctionPtr(CCAPIFunctions Function)
        {
            return CCAPIFunctionsList.ElementAt((int)Function);
        }

        private bool IsCCAPILoaded()
        {
            for (int i = 0; i < CCAPIFunctionsList.Count; i++)
                if (CCAPIFunctionsList.ElementAt(i) == IntPtr.Zero)
                    return false;
            return true;
        }

        private void CompleteInfo(ref TargetInfo Info, int fw, int ccapi, ulong sysTable, int consoleType, int tempCELL, int tempRSX)
        {
            Info.Firmware = fw;
            Info.CCAPI = ccapi;
            Info.SysTable = sysTable;
            Info.ConsoleType = consoleType;
            Info.TempCell = tempCELL;
            Info.TempRSX = tempRSX;
        }

        /// <summary>Return true if a ccapi function return a good integer.</summary>
        public bool SUCCESS(int Void)
        {
            if (Void == 0)
                return true;
            else return false;
        }

        /// <summary>Connect your console by console list.</summary>
        public bool ConnectTarget()
        {
            return true;
        }

        /// <summary>Connect your console by ip address.</summary>
        public int ConnectTarget(string targetIP)
        {
            return 0;
        }

        /// <summary>Get the status of the console.</summary>
        public int GetConnectionStatus()
        {
            if (mem.mProc.Process != null)
            {
                return 0;
            }
            return -1;
        }

        /// <summary>Disconnect your console.</summary>
        public int DisconnectTarget()
        {
            mem.CloseProcess();
            return 0;
        }

        /// <summary>Attach the default process (Current Game).</summary>
        public int AttachProcess()
        {
            AttachDialog attachDialog = new AttachDialog();
            if (mem.mProc.Process != null)
            {
                return 0;
            }
            return -1;
        }

        /// <summary>Attach your desired process.</summary>
        public int AttachProcess(ProcessType procType)
        {
            AttachDialog attachDialog = new AttachDialog();
            if (mem.mProc.Process != null)
            {
                return 0;
            }
            return -1;
        }

        /// <summary>Attach your desired process.</summary>
        public int AttachProcess(uint process)
        {
            bool flag = mem.OpenProcess(Convert.ToInt32(process));
            if (flag)
            {
                return 0;
            }
            return -1;
        }

        /// <summary>Get a list of all processes available.</summary>
        public int GetProcessList(out uint[] processIds)
        {
            List<uint> procid = new List<uint>();
            S::Diagnostics.Process[] ps = S::Diagnostics.Process.GetProcesses();
            foreach (S::Diagnostics.Process p in ps)
            {
                try
                {
                    procid.Add(Convert.ToUInt32(p.Id));
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Error: {0}", ex.Message);
                }
            }
            processIds = procid.ToArray();
            if (procid.Count > 0)
            {
                return 0;
            }
            return -1;
        }

        /// <summary>Get the process name of your choice.</summary>
        public int GetProcessName(uint processId, out string name)
        {
            try
            {
                S::Diagnostics.Process ps = S.Diagnostics.Process.GetProcessById(Convert.ToInt32(processId));
                name = ps.ProcessName;
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: {0}", ex.Message);
            }
            name = "";
            return -1;
        }

        /// <summary>Return the current process attached. Use this function only if you called AttachProcess before.</summary>
        public uint GetAttachedProcess()
        {
            return Convert.ToUInt32(mem.mProc.Process.Id);
        }

        /// <summary>Set memory to offset (uint).</summary>
        public int SetMemory(uint offset, byte[] buffer)
        {
            ulong offset2 = Convert.ToUInt64(offset) + 0x400000000;
            if (mem.GetCode("0x" + offset2.ToString("X"), "", 8) == UIntPtr.Zero)
            {
                return -1;
            }
            mem.WriteBytes("0x" + offset2.ToString("X"), buffer);
            return 0;
        }

        /// <summary>Set memory to offset (ulong).</summary>
        public int SetMemory(ulong offset, byte[] buffer)
        {
            ulong offset2 = offset + 0x400000000;
            if (mem.GetCode("0x" + offset2.ToString("X"), "", 8) == UIntPtr.Zero)
            {
                return -1;
            }
            mem.WriteBytes("0x" + offset2.ToString("X"), buffer);
            return 0;
        }

        /// <summary>Set memory to offset (string hex).</summary>
        public int SetMemory(ulong offset, string hexadecimal, EndianType Type = EndianType.BigEndian)
        {
            byte[] Entry = StringToByteArray(hexadecimal);
            if (Type == EndianType.LittleEndian)
                Array.Reverse(Entry);
            ulong offset2 = offset + 0x400000000;
            if (mem.GetCode("0x" + offset2.ToString("X"), "", 8) == UIntPtr.Zero)
            {
                return -1;
            }
            mem.WriteBytes("0x" + offset2.ToString("X"), Entry);
            return 0;
        }

        /// <summary>Get memory from offset (uint).</summary>
        public int GetMemory(uint offset, byte[] buffer)
        {
            ulong offset2 = Convert.ToUInt64(offset) + 0x400000000;
            byte[] getdata = mem.ReadBytes("0x" + offset2.ToString("X"), buffer.Length);
            if (getdata == null)
            {
                return -1;
            }
            else
            {
                buffer = getdata;
            }
            return 0;
        }

        /// <summary>Get memory from offset (ulong).</summary>
        public int GetMemory(ulong offset, byte[] buffer)
        {
            ulong offset2 = offset + 0x400000000;
            byte[] getdata = mem.ReadBytes("0x" + offset2.ToString("X"), buffer.Length);
            if (getdata == null)
            {
                return -1;
            }
            else
            {
                buffer = getdata;
            }
            return 0;
        }

        /// <summary>Like Get memory but this function return directly the buffer from the offset (uint).</summary>
        public byte[] GetBytes(uint offset, uint length)
        {
            ulong offset2 = Convert.ToUInt64(offset) + 0x400000000;
            return mem.ReadBytes("0x" + offset2.ToString("X"), length);
        }

        /// <summary>Like Get memory but this function return directly the buffer from the offset (ulong).</summary>
        public byte[] GetBytes(ulong offset, uint length)
        {
            ulong offset2 = offset + 0x400000000;
            return mem.ReadBytes("0x" + offset2.ToString("X"), length);
        }

        /// <summary>Display the notify message on your PS3.</summary>
        public int Notify(NotifyIcon icon, string message)
        {
            S::Windows.Forms.NotifyIcon N = new S::Windows.Forms.NotifyIcon();
            return notify((int)icon, message);
        }

        /// <summary>Display the notify message on your PS3.</summary>
        public int Notify(int icon, string message)
        {
            return notify(icon, message);
        }

        /// <summary>You can shutdown the console or just reboot her according the flag selected.</summary>
        public int ShutDown(RebootFlags flag)
        {
            mem.CloseProcess();
            return 1;
        }

        /// <summary>Your console will emit a song.</summary>
        public int RingBuzzer(BuzzerMode flag)
        {
            try
            {
                if (flag == BuzzerMode.Single)
                {
                    player = new S::Media.SoundPlayer(Properties.Resources._1beep);
                    player.PlaySync();
                    return 0;
                }
                else if (flag == BuzzerMode.Double)
                {
                    player = new S::Media.SoundPlayer(Properties.Resources._2beep);
                    player.PlaySync();
                    return 0;
                }
                else if (flag == BuzzerMode.Triple || flag == BuzzerMode.Continuous)
                {
                    player = new S::Media.SoundPlayer(Properties.Resources._3beep);
                    player.PlaySync();
                    return 0;
                }
            }
            catch
            {
                return -1;
            }
            return -1;
        }

        /// <summary>Change leds for your console.</summary>
        public int SetConsoleLed(LedColor color, LedMode mode)
        {
            return 0;
        }

        private int GetTargetInfo()
        {
            int result = -1; int[] sysTemp = new int[2];
            int fw = 0, ccapi = 0, consoleType = 0; ulong sysTable = 0;
            result = getFirmwareInfo(ref fw, ref ccapi, ref consoleType);
            if (result >= 0)
            {
                result = getTemperature(ref sysTemp[0], ref sysTemp[1]);
                if (result >= 0)
                    CompleteInfo(ref pInfo, fw, ccapi, sysTable, consoleType, sysTemp[0], sysTemp[1]);
            }

            return result;
        }

        /// <summary>Get informations of your console and store them into TargetInfo class.</summary>
        public int GetTargetInfo(out TargetInfo Info)
        {
            Info = new TargetInfo();
            int result = -1; int[] sysTemp = new int[2];
            int fw = 0, ccapi = 0, consoleType = 0; ulong sysTable = 0;
            result = getFirmwareInfo(ref fw, ref ccapi, ref consoleType);
            if (result >= 0)
            {
                result = getTemperature(ref sysTemp[0], ref sysTemp[1]);
                if (result >= 0)
                {
                    CompleteInfo(ref Info, fw, ccapi, sysTable, consoleType, sysTemp[0], sysTemp[1]);
                    CompleteInfo(ref pInfo, fw, ccapi, sysTable, consoleType, sysTemp[0], sysTemp[1]);
                }
            }
            return result;
        }

        /// <summary>Return the current firmware of your console in string format.</summary>
        public string GetFirmwareVersion()
        {
            if (pInfo.Firmware == 0)
                GetTargetInfo();

            string ver = pInfo.Firmware.ToString("X8");
            string char1 = ver.Substring(1, 1) + ".";
            string char2 = ver.Substring(3, 1);
            string char3 = ver.Substring(4, 1);
            return char1 + char2 + char3;
        }

        /// <summary>Return the current temperature of your system in string.</summary>
        public string GetTemperatureCELL()
        {
            if (pInfo.TempCell == 0)
                GetTargetInfo(out pInfo);

            return pInfo.TempCell.ToString() + " C";
        }

        /// <summary>Return the current temperature of your system in string.</summary>
        public string GetTemperatureRSX()
        {
            if (pInfo.TempRSX == 0)
                GetTargetInfo(out pInfo);
            return pInfo.TempRSX.ToString() + " C";
        }

        /// <summary>Return the type of your firmware in string format.</summary>
        public string GetFirmwareType()
        {
            if (pInfo.ConsoleType == 0)
                GetTargetInfo(out pInfo);
            string type = "UNK";
            if (pInfo.ConsoleType == (int)ConsoleType.CEX)
                type = "CEX";
            else if (pInfo.ConsoleType == (int)ConsoleType.DEX)
                type = "DEX";
            else if (pInfo.ConsoleType == (int)ConsoleType.TOOL)
                type = "TOOL";
            return type;
        }

        /// <summary>Clear informations into the DLL (PS3Lib).</summary>
        public void ClearTargetInfo()
        {
            pInfo = new TargetInfo();
        }

        /// <summary>Set a new ConsoleID in real time. (string)</summary>
        public int SetConsoleID(string consoleID)
        {
            MessageBox.Show("SetConsoleID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Set a new ConsoleID in real time. (bytes)</summary>
        public int SetConsoleID(byte[] consoleID)
        {
            MessageBox.Show("SetConsoleID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Set a new PSID in real time. (string)</summary>
        public int SetPSID(string PSID)
        {
            MessageBox.Show("SetPSID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Set a new PSID in real time. (bytes)</summary>
        public int SetPSID(byte[] consoleID)
        {
            MessageBox.Show("SetPSID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Set a console ID when the console is running. (string)</summary>
        public int SetBootConsoleID(string consoleID, IdType Type = IdType.IDPS)
        {
            MessageBox.Show("SetBootConsoleID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Set a console ID when the console is running. (bytes)</summary>
        public int SetBootConsoleID(byte[] consoleID, IdType Type = IdType.IDPS)
        {
            MessageBox.Show("SetBootConsoleID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Reset a console ID when the console is running.</summary>
        public int ResetBootConsoleID(IdType Type = IdType.IDPS)
        {
            MessageBox.Show("ResetBootConsoleID: Unsuported By RPCS3", "Error.", MessageBoxButtons.OK, MessageBoxIcon.Hand);
            return 0;
        }

        /// <summary>Return CCAPI Version.</summary>
        public int GetDllVersion()
        {
            return 260;
        }

        /// <summary>Return a list of informations for each console available.</summary>
        public List<ConsoleInfo> GetConsoleList()
        {
            return new List<CCAPI.ConsoleInfo>
            {
                new CCAPI.ConsoleInfo
                {
                    Ip = "127.0.0.1",
                    Name = "RPCS3"
                }
            };
        }

        internal static byte[] StringToByteArray(string hex)
        {
            try
            {
                string replace = hex.Replace("0x", "");
                string Stringz = replace.Insert(replace.Length - 1, "0");

                int Odd = replace.Length;
                bool Nombre;
                if (Odd % 2 == 0)
                    Nombre = true;
                else
                    Nombre = false;
                if (Nombre == true)
                {
                    return Enumerable.Range(0, replace.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(replace.Substring(x, 2), 16))
                    .ToArray();
                }
                else
                {
                    return Enumerable.Range(0, replace.Length)
                    .Where(x => x % 2 == 0)
                    .Select(x => Convert.ToByte(Stringz.Substring(x, 2), 16))
                    .ToArray();
                }
            }
            catch
            {
                MessageBox.Show("Incorrect value (empty)", "StringToByteArray Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return new byte[1];
            }
        }
    }
}
