using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Text;

namespace Utilities
{
   public static class Native
   {
      [Flags]
      enum ACCESS_MASK : uint
      {
         DELETE = 0x00010000,
         READ_CONTROL = 0x00020000,
         WRITE_DAC = 0x00040000,
         WRITE_OWNER = 0x00080000,
         SYNCHRONIZE = 0x00100000,

         STANDARD_RIGHTS_REQUIRED = 0x000f0000,

         STANDARD_RIGHTS_READ = 0x00020000,
         STANDARD_RIGHTS_WRITE = 0x00020000,
         STANDARD_RIGHTS_EXECUTE = 0x00020000,

         STANDARD_RIGHTS_ALL = 0x001f0000,

         SPECIFIC_RIGHTS_ALL = 0x0000ffff,

         ACCESS_SYSTEM_SECURITY = 0x01000000,

         MAXIMUM_ALLOWED = 0x02000000,

         GENERIC_READ = 0x80000000,
         GENERIC_WRITE = 0x40000000,
         GENERIC_EXECUTE = 0x20000000,
         GENERIC_ALL = 0x10000000,

         DESKTOP_READOBJECTS = 0x00000001,
         DESKTOP_CREATEWINDOW = 0x00000002,
         DESKTOP_CREATEMENU = 0x00000004,
         DESKTOP_HOOKCONTROL = 0x00000008,
         DESKTOP_JOURNALRECORD = 0x00000010,
         DESKTOP_JOURNALPLAYBACK = 0x00000020,
         DESKTOP_ENUMERATE = 0x00000040,
         DESKTOP_WRITEOBJECTS = 0x00000080,
         DESKTOP_SWITCHDESKTOP = 0x00000100,

         WINSTA_ENUMDESKTOPS = 0x00000001,
         WINSTA_READATTRIBUTES = 0x00000002,
         WINSTA_ACCESSCLIPBOARD = 0x00000004,
         WINSTA_CREATEDESKTOP = 0x00000008,
         WINSTA_WRITEATTRIBUTES = 0x00000010,
         WINSTA_ACCESSGLOBALATOMS = 0x00000020,
         WINSTA_EXITWINDOWS = 0x00000040,
         WINSTA_ENUMERATE = 0x00000100,
         WINSTA_READSCREEN = 0x00000200,

         WINSTA_ALL_ACCESS = 0x0000037f
      }

      public static class Ntdll
      {
         [StructLayout(LayoutKind.Sequential)]
         public struct PROCESS_BASIC_INFORMATION
         {
            public int ExitStatus;
            public int PebBaseAddress;
            public int AffinityMask;
            public int BasePriority;
            public int UniqueProcessId;
            public int InheritedFromUniqueProcessId;

            public int Size
            {
               get { return (6 * 4); }
            }
         }

         public enum PROCESSINFOCLASS : int
         {
            ProcessBasicInformation = 0,
            ProcessQuotaLimits,
            ProcessIoCounters,
            ProcessVmCounters,
            ProcessTimes,
            ProcessBasePriority,
            ProcessRaisePriority,
            ProcessDebugPort,
            ProcessExceptionPort,
            ProcessAccessToken,
            ProcessLdtInformation,
            ProcessLdtSize,
            ProcessDefaultHardErrorMode,
            ProcessIoPortHandlers, // Note: this is kernel mode only
            ProcessPooledUsageAndLimits,
            ProcessWorkingSetWatch,
            ProcessUserModeIOPL,
            ProcessEnableAlignmentFaultFixup,
            ProcessPriorityClass,
            ProcessWx86Information,
            ProcessHandleCount,
            ProcessAffinityMask,
            ProcessPriorityBoost,
            MaxProcessInfoClass,
            ProcessWow64Information = 26
         };

         [DllImport("ntdll.dll", SetLastError = true)]
         static extern int NtQueryInformationProcess(IntPtr hProcess, PROCESSINFOCLASS pic, ref PROCESS_BASIC_INFORMATION pbi, int cb, out int pSize);

         public static int GetParentProcessID(int processId)
         {
            IntPtr process = Kernel32.OpenProcess(Kernel32.ProcessAccessFlags.QueryInformation, false, processId);
            if (process == IntPtr.Zero)
            {
               throw new InvalidOperationException("ERROR opening process handle");
            }

            PROCESS_BASIC_INFORMATION pbi = new PROCESS_BASIC_INFORMATION();
            int retLen;

            int ntStatus = NtQueryInformationProcess(process, PROCESSINFOCLASS.ProcessBasicInformation, ref pbi, pbi.Size, out retLen);
            Kernel32.CloseHandle(process);

            if (ntStatus != 0)
            {
               throw new InvalidOperationException("ERROR getting parent PID");
            }

            return pbi.InheritedFromUniqueProcessId;
         }
      }

      public static class Kernel32
      {
			public const uint GENERIC_READ = 0x80000000;
			public const uint GENERIC_WRITE = 0x40000000;
			public const uint OPEN_EXISTING = 3;
			public const uint FILE_FLAG_OVERLAPPED = 0x40000000;
			public const uint FILE_SHARE_READ = 1;
			public const uint FILE_SHARE_WRITE = 2; 

			[Flags]
         public enum ProcessAccessFlags : int
         {
            All = 0x001F0FFF,
            Terminate = 0x00000001,
            CreateThread = 0x00000002,
            VMOperation = 0x00000008,
            VMRead = 0x00000010,
            VMWrite = 0x00000020,
            DupHandle = 0x00000040,
            SetInformation = 0x00000200,
            QueryInformation = 0x00000400,
            Synchronize = 0x00100000
         }

         [StructLayout(LayoutKind.Sequential)]
         public struct SystemTime
         {
            public short Year;
            public short Month;
            public short DayOfWeek;
            public short Day;
            public short Hour;
            public short Minute;
            public short Second;
            public short Millisecond;
         }

         [DllImport("kernel32.dll", SetLastError = true)]
         public static extern bool AllocConsole();

         [DllImport("kernel32.dll", SetLastError = true)]
         public static extern bool FreeConsole();

         [DllImport("kernel32", SetLastError = true)]
         public static extern bool AttachConsole(int dwProcessId);

         [DllImport("kernel32.dll")]
         public static extern bool SetConsoleTitle(string lpConsoleTitle);

         [DllImport("kernel32.dll")]
         public static extern IntPtr OpenProcess(ProcessAccessFlags dwDesiredAccess, [MarshalAs(UnmanagedType.Bool)] bool bInheritHandle, int dwProcessId);

			[DllImport("kernel32.dll", SetLastError = true)]
			public static extern IntPtr CreateFile([In] string strName, uint nAccess, uint nShareMode, IntPtr lpSecurity, uint nCreationFlags, uint nAttributes, IntPtr lpTemplate);

			[DllImport("kernel32.dll", SetLastError = true)]
         [return: MarshalAs(UnmanagedType.Bool)]
         public static extern bool CloseHandle(IntPtr hObject);

         [DllImport("kernel32.dll", SetLastError = true)]
         public static extern int SetLocalTime(ref SystemTime s);

         [DllImport("kernel32.dll")]
         public static extern bool WritePrivateProfileString(string lpAppName, string lpKeyName, string lpString, string lpFileName);

         [DllImport("kernel32.dll")]
         public static extern uint GetPrivateProfileString(string lpAppName, string lpKeyName, string lpDefault, StringBuilder lpReturnedString, int nSize, string lpFileName);
      }

      public static class AdvApi32
      {
         [StructLayout(LayoutKind.Sequential)]
         public struct SERVICE_STATUS
         {
            public int ServiceType;
            public int CurrentState;
            public int ControlsAccepted;
            public int Win32ExitCode;
            public int ServiceSpecificExitCode;
            public int CheckPoint;
            public int WaitHint;
         }

         public enum ServiceState
         {
            SERVICE_STOPPED = 0x00000001,
            SERVICE_START_PENDING = 0x00000002,
            SERVICE_STOP_PENDING = 0x00000003,
            SERVICE_RUNNING = 0x00000004,
            SERVICE_CONTINUE_PENDING = 0x00000005,
            SERVICE_PAUSE_PENDING = 0x00000006,
            SERVICE_PAUSED = 0x00000007,
         }

         [Flags]
         public enum SCM_ACCESS : uint
         {
            SC_MANAGER_CONNECT = 0x00001,
            SC_MANAGER_CREATE_SERVICE = 0x00002,
            SC_MANAGER_ENUMERATE_SERVICE = 0x00004,
            SC_MANAGER_LOCK = 0x00008,
            SC_MANAGER_QUERY_LOCK_STATUS = 0x00010,
            SC_MANAGER_MODIFY_BOOT_CONFIG = 0x00020,
            SC_MANAGER_ALL_ACCESS = ACCESS_MASK.STANDARD_RIGHTS_REQUIRED |
                SC_MANAGER_CONNECT |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_LOCK |
                SC_MANAGER_QUERY_LOCK_STATUS |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_READ = ACCESS_MASK.STANDARD_RIGHTS_READ |
                SC_MANAGER_ENUMERATE_SERVICE |
                SC_MANAGER_QUERY_LOCK_STATUS,

            GENERIC_WRITE = ACCESS_MASK.STANDARD_RIGHTS_WRITE |
                SC_MANAGER_CREATE_SERVICE |
                SC_MANAGER_MODIFY_BOOT_CONFIG,

            GENERIC_EXECUTE = ACCESS_MASK.STANDARD_RIGHTS_EXECUTE |
                SC_MANAGER_CONNECT | SC_MANAGER_LOCK,

            GENERIC_ALL = SC_MANAGER_ALL_ACCESS
         }

         [Flags]
         public enum SERVICE_ACCESS : uint
         {
            STANDARD_RIGHTS_REQUIRED = 0xF0000,
            SERVICE_QUERY_CONFIG = 0x00001,
            SERVICE_CHANGE_CONFIG = 0x00002,
            SERVICE_QUERY_STATUS = 0x00004,
            SERVICE_ENUMERATE_DEPENDENTS = 0x00008,
            SERVICE_START = 0x00010,
            SERVICE_STOP = 0x00020,
            SERVICE_PAUSE_CONTINUE = 0x00040,
            SERVICE_INTERROGATE = 0x00080,
            SERVICE_USER_DEFINED_CONTROL = 0x00100,
            SERVICE_ALL_ACCESS = (STANDARD_RIGHTS_REQUIRED |
                              SERVICE_QUERY_CONFIG |
                              SERVICE_CHANGE_CONFIG |
                              SERVICE_QUERY_STATUS |
                              SERVICE_ENUMERATE_DEPENDENTS |
                              SERVICE_START |
                              SERVICE_STOP |
                              SERVICE_PAUSE_CONTINUE |
                              SERVICE_INTERROGATE |
                              SERVICE_USER_DEFINED_CONTROL)
         }

         public const uint SERVICE_NO_CHANGE = 0xffffffff;

         [DllImport("AdvApi32.dll", EntryPoint = "SetServiceStatus")]
         public static extern bool SetServiceStatus(IntPtr hServiceStatus, SERVICE_STATUS lpServiceStatus);

         [DllImport("advapi32.dll", SetLastError = true)]
         public static extern IntPtr OpenSCManager(string machineName, string databaseName, uint dwAccess);

         [DllImport("advapi32.dll", SetLastError = true)]
         public static extern IntPtr LockServiceDatabase(IntPtr hSCManager);

         [DllImport("advapi32.dll", SetLastError = true)]
         public static extern IntPtr OpenService(IntPtr hSCManager, string lpServiceName, uint dwDesiredAccess);

         [DllImport("advapi32.dll", SetLastError = true)]
         public static extern Boolean ChangeServiceConfig(IntPtr hService, uint nServiceType, uint nStartType, uint nErrorControl, string lpBinaryPathName, string lpLoadOrderGroup, IntPtr lpdwTagId, string lpDependencies, string lpServiceStartName, string lpPassword, string lpDisplayName);

         [DllImport("advapi32.dll", SetLastError = true)]
         [return: MarshalAs(UnmanagedType.Bool)]
         public static extern bool CloseServiceHandle(IntPtr hSCObject);

         [DllImport("advapi32.dll", SetLastError = true)]
         [return: MarshalAs(UnmanagedType.Bool)]
         public static extern bool UnlockServiceDatabase(IntPtr hSCObject);
      }

      public static class User32
      {
         public const int SW_HIDE = 0;
         public const int SW_SHOWNORMAL = 1;

         [DllImport("user32.dll")]
         public static extern IntPtr GetForegroundWindow();

         [DllImport("user32.dll", SetLastError = true)]
         public static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

         [DllImport("user32.dll", SetLastError = true)]
         public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
      }

      public static class IpHlpApi
      {
         const int ERROR_BUFFER_OVERFLOW = 111;

         const int MAX_ADAPTER_DESCRIPTION_LENGTH = 128;
         const int MAX_ADAPTER_NAME_LENGTH = 256;
         const int MAX_ADAPTER_ADDRESS_LENGTH = 8;

         const int MAX_INTERFACE_NAME_LEN = 256;
         const int MAXLEN_PHYSADDR = 8;
         const int MAXLEN_IFDESCR = 256;

         public const int MIB_IF_TYPE_OTHER = 1;
         public const int MIB_IF_TYPE_ETHERNET = 6;
         public const int MIB_IF_TYPE_TOKENRING = 9;
         public const int MIB_IF_TYPE_FDDI = 15;
         public const int MIB_IF_TYPE_PPP = 23;
         public const int MIB_IF_TYPE_LOOPBACK = 24;
         public const int MIB_IF_TYPE_SLIP = 28;

         public const int IF_OPER_STATUS_NON_OPERATIONAL = 0;
         public const int IF_OPER_STATUS_UNREACHABLE = 1;
         public const int IF_OPER_STATUS_DISCONNECTED = 2;
         public const int IF_OPER_STATUS_CONNECTING = 3;
         public const int IF_OPER_STATUS_CONNECTED = 4;
         public const int IF_OPER_STATUS_OPERATIONAL = 5;

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         public struct IP_ADDRESS_STRING
         {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
            public string Address;
         }

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         public struct IP_ADDR_STRING
         {
            public IntPtr Next;
            public IP_ADDRESS_STRING IpAddress;
            public IP_ADDRESS_STRING IpMask;
            public int Context;
         }

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
         public struct IP_ADAPTER_INFO
         {
            public IntPtr Next;
            public int ComboIndex;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_NAME_LENGTH + 4)]
            public string AdapterName;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_ADAPTER_DESCRIPTION_LENGTH + 4)]
            public string AdapterDescription;
            public int AddressLength;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAX_ADAPTER_ADDRESS_LENGTH)]
            public byte[] Address;
            public int Index;
            public int Type;
            public int DhcpEnabled;
            public IntPtr CurrentIpAddress;
            public IP_ADDR_STRING IpAddressList;
            public IP_ADDR_STRING GatewayList;
            public IP_ADDR_STRING DhcpServer;
            public bool HaveWins;
            public IP_ADDR_STRING PrimaryWinsServer;
            public IP_ADDR_STRING SecondaryWinsServer;
            public int LeaseObtained;
            public int LeaseExpires;
         }

         [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
         public struct MIB_IFROW
         {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_INTERFACE_NAME_LEN)]
            public string wszName;
            public int dwIndex;
            public int dwType;
            public int dwMtu;
            public int dwSpeed;
            public int dwPhysAddrLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXLEN_PHYSADDR)]
            public byte[] bPhysAddr;
            public int dwAdminStatus;
            public int dwOperStatus;
            public int dwLastChange;
            public int dwInOctets;
            public int dwInUcastPkts;
            public int dwInNUcastPkts;
            public int dwInDiscards;
            public int dwInErrors;
            public int dwInUnknownProtos;
            public int dwOutOctets;
            public int dwOutUcastPkts;
            public int dwOutNUcastPkts;
            public int dwOutDiscards;
            public int dwOutErrors;
            public int dwOutQLen;
            public int dwDescrLen;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = MAXLEN_IFDESCR)]
            public byte[] bDescr;
         }

         [DllImport("iphlpapi.dll", CharSet = CharSet.Ansi)]
         public static extern int GetAdaptersInfo(IntPtr pAdapterInfo, ref Int64 pBufOutLen);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int GetAdapterIndex(string adapter, out int index);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int GetIfEntry(IntPtr pIfRow);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int AddIPAddress(int Address, int IpMask, int IfIndex, out int NTEContext, out int NTEInstance);

         [DllImport("iphlpapi.dll", SetLastError = true)]
         public static extern int DeleteIPAddress(int NTEContext);

         public static int GetAdapterIndex(string adapter)
         {
            int index;

            int ret = GetAdapterIndex(adapter, out index);
            if (ret != 0)
            {
               throw new System.ComponentModel.Win32Exception(ret);
            }

            return index;
         }

         public static int GetAdapterInfo(string adapter, out IP_ADAPTER_INFO info)
         {
            long structSize = Marshal.SizeOf(typeof(IP_ADAPTER_INFO));
            IntPtr pArray = Marshal.AllocHGlobal(new IntPtr(structSize));

            try
            {
               int ret = GetAdaptersInfo(pArray, ref structSize);

               if (ret == ERROR_BUFFER_OVERFLOW)
               {
                  pArray = Marshal.ReAllocHGlobal(pArray, new IntPtr(structSize));
                  ret = GetAdaptersInfo(pArray, ref structSize);
               }

               if (ret != 0)
               {
                  throw new System.ComponentModel.Win32Exception(ret);
               }

               IntPtr pEntry = pArray;

               while (pEntry != IntPtr.Zero)
               {
                  info = (IP_ADAPTER_INFO)Marshal.PtrToStructure(pEntry, typeof(IP_ADAPTER_INFO));

                  if (info.IpAddressList.IpAddress.Address == adapter)
                  {
                     return info.IpAddressList.Context;
                  }

                  IntPtr pAddr = info.IpAddressList.Next;

                  while (pAddr != IntPtr.Zero)
                  {
                     IP_ADDR_STRING addr = (IP_ADDR_STRING)Marshal.PtrToStructure(pAddr, typeof(IP_ADDR_STRING));
                     if (addr.IpAddress.Address == adapter)
                     {
                        return addr.Context;
                     }

                     pAddr = addr.Next;
                  }

                  pEntry = info.Next;
               }

               throw new InvalidOperationException("Adapter not found");
            }
            finally
            {
               Marshal.FreeHGlobal(pArray);
            }
         }

         public static void GetIfEntry(int index, out MIB_IFROW info)
         {
            info = new MIB_IFROW();
            info.dwIndex = index;

            long structSize = Marshal.SizeOf(typeof(MIB_IFROW));
            IntPtr pIfRow = Marshal.AllocHGlobal(new IntPtr(structSize));
            Marshal.StructureToPtr(info, pIfRow, false);

            try
            {
               int ret = GetIfEntry(pIfRow);

               if (ret != 0)
               {
                  throw new System.ComponentModel.Win32Exception(ret);
               }

               info = (MIB_IFROW)Marshal.PtrToStructure(pIfRow, typeof(MIB_IFROW));
            }
            finally
            {
               Marshal.FreeHGlobal(pIfRow);
            }
         }

         public static int AddIPAddress(string ip, string mask, int index)
         {
            int NTEContext, NTEInstance;

            int ret = AddIPAddress(BitConverter.ToInt32(IPAddress.Parse(ip).GetAddressBytes(), 0),
               BitConverter.ToInt32(IPAddress.Parse(mask).GetAddressBytes(), 0), index, out NTEContext, out NTEInstance);
            if (ret != 0)
            {
               throw new System.ComponentModel.Win32Exception(ret);
            }

            return NTEContext;
         }

         public static void DeleteIPAddress(string ip)
         {
            IP_ADAPTER_INFO adapter;
            int context = GetAdapterInfo(ip, out adapter);

            int ret = DeleteIPAddress(context);
            if (ret != 0)
            {
               throw new System.ComponentModel.Win32Exception(ret);
            }
         }
      }
   }
}
