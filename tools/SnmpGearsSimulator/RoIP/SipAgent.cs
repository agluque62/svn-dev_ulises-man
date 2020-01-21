//#define _TRACEAGENT_
#define _VOTER_
using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using System.Timers;

using System.ComponentModel;
using System.Reflection;

using NLog;
using Utilities;

namespace Roip
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="level"></param>
    /// <param name="data"></param>
    /// <param name="len"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void LogCb(int level, string data, int len);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void KaTimeoutCb(int call);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void RdInfoCb(int call, [In] CORESIP_RdInfo info);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="stateInfo"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void CallStateCb(int call, [In] CORESIP_CallInfo info, [In] CORESIP_CallStateInfo stateInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="call2replace"></param>
    /// <param name="info"></param>
    /// <param name="inInfo"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void CallIncomingCb(int call, int call2replace, [In] CORESIP_CallInfo info, [In] CORESIP_CallInInfo inInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="transferInfo"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void TransferRequestCb(int call, [In] CORESIP_CallInfo info, [In] CORESIP_CallTransferInfo transferInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="code"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void TransferStatusCb(int call, int code);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="confInfo"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void ConfInfoCb(int call, [In] CORESIP_ConfInfo confInfo);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="fromUri"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void OptionsReceiveCb(string fromUri);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="call"></param>
    /// <param name="info"></param>
    /// <param name="lenInfo"></param>
	[UnmanagedFunctionPointer(CallingConvention.Cdecl, CharSet = CharSet.Ansi)]
	public delegate void InfoReceivedCb(int call, string info, uint lenInfo);

	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void WG67NotifyCb(IntPtr wg67, CORESIP_WG67Info wg67Info, IntPtr userData);
	
#if _ED137_
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void UpdateOvrCallMembersCb([In] CORESIP_OvrCallMembers members);
    
	[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
	public delegate void InfoCRDCb([In] CORESIP_CRD crd);
	
	public enum CORESIP_TypeCrdInfo
	{
		CORESIP_CRD_SET_PARAMETER,
		CORESIP_CRD_RECORD,
		CORESIP_CRD_PAUSE,
		CORESIP_CRD_PTT,
		CORESIP_SQ
	}
#endif

	/// <summary>
    /// 
    /// </summary>
	public enum CORESIP_CallType
	{
        [Description("IA CALL")]
		CORESIP_CALL_IA,
        [Description("MON CALL")]
		CORESIP_CALL_MONITORING,
        [Description("G/G MON CALL")]
		CORESIP_CALL_GG_MONITORING,
        [Description("A/G MON CALL")]
		CORESIP_CALL_AG_MONITORING,
        [Description("DA CALL")]
		CORESIP_CALL_DIA,
		CORESIP_CALL_RD,
		CORESIP_CALL_UNKNOWN
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_Priority
	{
        [Description("Emergencia")]
		CORESIP_PR_EMERGENCY,
        [Description("Urgente")]
		CORESIP_PR_URGENT,
        [Description("Normal")]
		CORESIP_PR_NORMAL,
        [Description("No Urgente")]
		CORESIP_PR_NONURGENT,
		CORESIP_PR_UNKNOWN
	}
    /// <summary>
    /// 
    /// </summary>
	[Flags]
	public enum CORESIP_CallFlags
	{
        [Description("TxRx")]
		CORESIP_CALL_NO_FLAGS = 0,
		CORESIP_CALL_CONF_FOCUS = 0x1,
        [Description("Coupling")]
		CORESIP_CALL_RD_COUPLING = 0x2,
        [Description("Rx")]
		CORESIP_CALL_RD_RXONLY = 0x4,
        [Description("Tx")]
		CORESIP_CALL_RD_TXONLY = 0x8,
		CORESIP_CALL_EC = 0x10
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_CallState
	{
        [Description("Null")]
		CORESIP_CALL_STATE_NULL,					/**< Before INVITE is sent or received  */
        [Description("Calling")]
		CORESIP_CALL_STATE_CALLING,				/**< After INVITE is sent		    */
        [Description("Incoming")]
		CORESIP_CALL_STATE_INCOMING,				/**< After INVITE is received.	    */
        [Description("Early")]
		CORESIP_CALL_STATE_EARLY,					/**< After response with To tag.	    */
        [Description("Connecting")]
		CORESIP_CALL_STATE_CONNECTING,			/**< After 2xx is sent/received.	    */
        [Description("Confirmed")]
		CORESIP_CALL_STATE_CONFIRMED,			/**< After ACK is sent/received.	    */
        [Description("Disconnected")]
		CORESIP_CALL_STATE_DISCONNECTED,		/**< Session is terminated.		    */
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_CallRole
	{
		CORESIP_CALL_ROLE_UAC,
		CORESIP_CALL_ROLE_UCS
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_MediaStatus
	{
		CORESIP_MEDIA_NONE,
		CORESIP_MEDIA_ACTIVE,
		CORESIP_MEDIA_LOCAL_HOLD,
		CORESIP_MEDIA_REMOTE_HOLD,
		CORESIP_MEDIA_ERROR
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_MediaDir
	{
		CORESIP_DIR_NONE,
		CORESIP_DIR_SENDONLY,
		CORESIP_DIR_RECVONLY,
		CORESIP_DIR_SENDRECV
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_PttType
	{
		CORESIP_PTT_OFF,
        [Description("Normal")]
		CORESIP_PTT_NORMAL,
        [Description("Coupling")]
		CORESIP_PTT_COUPLING,
        [Description("Prioritario")]
		CORESIP_PTT_PRIORITY,
        [Description("Emergencia")]
		CORESIP_PTT_EMERGENCY
	}
    /// <summary>
    /// 
    /// </summary>
	public enum CORESIP_SndDevType
	{
		CORESIP_SND_INSTRUCTOR_MHP,
		CORESIP_SND_ALUMN_MHP,
		CORESIP_SND_MAX_IN_DEVICES,
		CORESIP_SND_MAIN_SPEAKERS = CORESIP_SND_MAX_IN_DEVICES,
		CORESIP_SND_LC_SPEAKER,
		CORESIP_SND_RD_SPEAKER,
		CORESIP_SND_INSTRUCTOR_RECORDER,
		CORESIP_SND_ALUMN_RECORDER,
		CORESIP_SND_RADIO_RECORDER,
		CORESIP_SND_LC_RECORDER,
        CORESIP_SND_HF_SPEAKER,
        CORESIP_SND_HF_RECORDER,
		CORESIP_SND_UNKNOWN
	}

	public class WG67Info
	{
		public struct SubscriberInfo
		{
			public ushort PttId;
			public string SubsUri;
		}

		public string DstUri;

		public bool SubscriptionTerminated;
		public uint SubscribersCount;
		public string LastReason;

		public SubscriberInfo[] Subscribers;
	}

#if _ED137_
	  // PlugTest FAA   05/2011
	  // OVR calls
	   [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	   public class CORESIP_OvrCallMembers
	   {
		   public short MembersCount;

		   [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		   public struct CORESIP_Members
		   {
			   [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
			   public string Member;
			   public int CallId;
			   bool IncommingCall;
		   }

		   [MarshalAs(UnmanagedType.ByValArray, SizeConst = SipAgent.CORESIP_MAX_OVR_CALLS_MEMBERS)]
		   public CORESIP_Members[] EstablishedOvrCallMembers;
	   }

       // PlugTest FAA 05/2011
       // Record
       [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
       public class CORESIP_CRD
       {
           public CORESIP_TypeCrdInfo _Info; 
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_CALLREF_LENGTH + 1)]
           public string CallRef;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_CONNREF_LENGTH + 1)]
           public string ConnRef;
           public int Direction;
           public int Priority;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
           public string CallingNr;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
           public string CallerNr;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_TIME_LENGTH + 1)]
           public string SetupTime;

           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
           public string ConnectedNr;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_TIME_LENGTH + 1)]
           public string ConnectedTime;

           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_TIME_LENGTH + 1)]
           public string DisconnectTime;
           public int DisconnectCause;
           public int DisconnectSource;

           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_FRECUENCY_LENGTH + 1)]
           public string FrecuencyId;

           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_TIME_LENGTH + 1)]
           public string Squ;
           [MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_TIME_LENGTH + 1)]
           public string Ptt;
       }
#endif
	
	/// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_CallInfo
	{
		public int AccountId;
		public CORESIP_CallType Type;
		public CORESIP_Priority Priority;
		public CORESIP_CallFlags Flags;

#if _VOTER_
        /** 20160608. VOTER */
        public int PreferredCodec = 0;
        public int PreferredBss = 0;
#endif
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_CallOutInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
		public string DstUri;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
		public string ReferBy;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
		public string RdFr;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
		public string RdMcastAddr;
		public uint RdMcastPort;
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_CallInInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
		public string SrcId;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
		public string SrcIp;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
		public string SrcSubId;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
		public string SrcRs;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
		public string DstId;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
		public string DstIp;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
		public string DstSubId;
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_CallTransferInfo
	{
		public IntPtr TxData;
		public IntPtr EvSub;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
		public string TsxKey;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
		public string ReferBy;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 2 * SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
		public string ReferTo;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
		public string DstId;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_IP_LENGTH + 1)]
		public string DstIp;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
		public string DstSubId;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_RS_LENGTH + 1)]
		public string DstRs;
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_CallStateInfo
	{
		public CORESIP_CallState State;
		public CORESIP_CallRole Role;

		public int LastCode;										// Util cuando State == PJSIP_INV_STATE_DISCONNECTED
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_REASON_LENGTH + 1)]
		public string LastReason;

		public int LocalFocus;
		public int RemoteFocus;
		public CORESIP_MediaStatus MediaStatus;
		public CORESIP_MediaDir MediaDir;

		// CORESIP_CALL_RD y PJSIP_INV_STATE_CONFIRMED
		public ushort PttId;
		public uint ClkRate;
		public uint ChannelCount;
		public uint BitsPerSample;
		public uint FrameTime;
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_PttInfo
	{
		public CORESIP_PttType PttType;
		public ushort PttId;
		public uint ClimaxCld;
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_RdInfo
	{
		public CORESIP_PttType PttType;
		public ushort PttId;
		public int Squelch;
		public int Sct;
		public int Bss;
		public int BssMethod;
		public int BssValue;
	}
	/// <summary>
	/// 
	/// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_ConfInfo
	{
		public uint Version;
		public uint UsersCount;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_CONF_STATE_LENGTH + 1)]
		public string State;

		public struct ConfUser
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
			public string Id;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
			public string Name;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_USER_ID_LENGTH + 1)]
			public string Role;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_CONF_STATE_LENGTH + 1)]
			public string State;
		}
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SipAgent.CORESIP_MAX_CONF_USERS)]
		public ConfUser[] Users;
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_Callbacks
	{
		public IntPtr UserData;

		public LogCb OnLog;
		public KaTimeoutCb OnKaTimeout;
		public RdInfoCb OnRdInfo;
		public CallStateCb OnCallState;
		public CallIncomingCb OnCallIncoming;
		public TransferRequestCb OnTransferRequest;
		public TransferStatusCb OnTransferStatus;
		public ConfInfoCb OnConfInfo;
		public OptionsReceiveCb OnOptionsReceive;
		public WG67NotifyCb OnWG67Notify;
		public InfoReceivedCb OnInfoReceived;
#if _ED137_
	// PlugTest FAA 05/2011
		public UpdateOvrCallMembersCb OnUpdateOvrCallMembers; //(CORESIP_EstablishedOvrCallMembers info);
		public InfoCRDCb OnInfoCrd;								//(CORESIP_CRD InfoCrd);
#endif
	}
    /// <summary>
    /// 
    /// </summary>
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_Params
	{
		public int EnableMonitoring;

		public uint KeepAlivePeriod;
		public uint KeepAliveMultiplier;
	}


	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
	public class CORESIP_WG67Info
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct CORESIP_WG67SubscriberInfo
		{
			public ushort PttId;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
			public string SubsUri;
		}

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_URI_LENGTH + 1)]
		public string DstUri;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = SipAgent.CORESIP_MAX_REASON_LENGTH + 1)]
		public string LastReason;

		public int SubscriptionTerminated;
		public uint SubscribersCount;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = SipAgent.CORESIP_MAX_WG67_SUBSCRIBERS)]
		public CORESIP_WG67SubscriberInfo[] Subscribers;
	}

	/// <summary>
    /// 
    /// </summary>
	public static class SipAgent
	{
		public const int SIP_TRYING = 100;
		public const int SIP_RINGING = 180;
		public const int SIP_QUEUED = 182;
		public const int SIP_INTRUSION_IN_PROGRESS = 183;
		public const int SIP_INTERRUPTION_IN_PROGRESS = 184;
		public const int SIP_INTERRUPTION_END = 185;
		public const int SIP_OK = 200;
		public const int SIP_ACCEPTED = 202;
		public const int SIP_BAD_REQUEST = 400;
		public const int SIP_NOT_FOUND = 404;
		public const int SIP_REQUEST_TIMEOUT = 408;
		public const int SIP_GONE = 410;
		public const int SIP_TEMPORARILY_UNAVAILABLE = 480;
		public const int SIP_BUSY = 486;
		public const int SIP_NOT_ACCEPTABLE_HERE = 488;
		public const int SIP_ERROR = 500;
		public const int SIP_CONGESTION = 503;
		public const int SIP_DECLINE = 603;

		#region Dll Interface

		public const int CORESIP_MAX_USER_ID_LENGTH = 100;
		public const int CORESIP_MAX_FILE_PATH_LENGTH = 256;
		public const int CORESIP_MAX_ERROR_INFO_LENGTH = 512;
        public const int CORESIP_MAX_HOSTID_LENGTH = 32;
		public const int CORESIP_MAX_IP_LENGTH = 25;
		public const int CORESIP_MAX_URI_LENGTH = 256;
		public const int CORESIP_MAX_SOUND_DEVICES = 10;
		public const int CORESIP_MAX_RS_LENGTH = 128;
		public const int CORESIP_MAX_REASON_LENGTH = 128;
		public const int CORESIP_MAX_WG67_SUBSCRIBERS = 25;
		public const int CORESIP_MAX_CODEC_LENGTH = 50;
		public const int CORESIP_MAX_CONF_USERS = 25;
		public const int CORESIP_MAX_CONF_STATE_LENGTH = 25;

#if _ED137_
		// PlugTest FAA 05/2011
		public const int CORESIP_MAX_OVR_CALLS_MEMBERS = 10;
		public const int CORESIP_CALLREF_LENGTH = 50;
		public const int CORESIP_CONNREF_LENGTH = 50;
		public const int CORESIP_TIME_LENGTH = 28;
		public const int CORESIP_MAX_FRECUENCY_LENGTH = 7;
#endif

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		struct CORESIP_Error
		{
			public int Code;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_FILE_PATH_LENGTH + 1)]
			public string File;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_ERROR_INFO_LENGTH + 1)]
			public string Info;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		class CORESIP_SndDeviceInfo
		{
			public CORESIP_SndDevType Type;
			public int OsInDeviceIndex;
			public int OsOutDeviceIndex;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		class CORESIP_RdRxPortInfo
		{
			public uint ClkRate;
			public uint ChannelCount;
			public uint BitsPerSample;
			public uint FrameTime;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_IP_LENGTH + 1)]
			public string Ip;
			public uint Port;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		class CORESIP_Config
		{
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_HOSTID_LENGTH + 1)]
            public string HostId;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_IP_LENGTH + 1)]
			public string IpAddress;
			public uint Port;

			public CORESIP_Callbacks Cb;

			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = CORESIP_MAX_CODEC_LENGTH + 1)]
			public string DefaultCodec;
			public uint DefaultDelayBufPframes;
			public uint DefaultJBufPframes;
			public uint SndSamplingRate;
			public float RxLevel;
			public float TxLevel;
			public uint LogLevel;

			public uint TsxTout;
			public uint InvProceedingIaTout;
			public uint InvProceedingMonitoringTout;
			public uint InvProceedingDiaTout;
			public uint InvProceedingRdTout;

            /* AGL 20131121. Variables para la configuracion del Cancelador de Eco */
            public uint EchoTail;
            public uint EchoLatency;
            /* FM */

            /// <summary>
            /// JCAM 18/01/2016
            /// Grabación según norma ED-137
            /// </summary>
            public uint RecordingEd137;
        }
#if _VOTER_
        const string coresip = "coresip-voter";
#else
        const string coresip = "coresip";
#endif

        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_Init([In] CORESIP_Config info, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_Start(out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern void CORESIP_End();
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_SetLogLevel(uint level, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_SetParams([In] CORESIP_Params info, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CreateAccount([In] string acc, int defaultAcc, out int accId, out CORESIP_Error error);

        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_CreateAccountAndRegisterInProxy([In] string acc, int defaultAcc, out int accId, string proxy_ip,
                                                                uint expire_seg, string username, string pass, out CORESIP_Error error);


        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_DestroyAccount(int accId, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_AddSndDevice([In] CORESIP_SndDeviceInfo info, out int dev, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CreateWavPlayer([In] string file, int loop, out int wavPlayer, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_DestroyWavPlayer(int wavPlayer, out CORESIP_Error error);		
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CreateRdRxPort([In] CORESIP_RdRxPortInfo info, string localIp, out int mcastPort, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int	CORESIP_DestroyRdRxPort(int mcastPort, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CreateSndRxPort(string id, out int sndRxPort, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_DestroySndRxPort(int sndRxPort, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_BridgeLink(int src, int dst, int on, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_SendToRemote(int dev, int on, string id, string ip, uint port, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_ReceiveFromRemote(string localIp, string mcastIp, uint mcastPort, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_SetVolume(int id, int volume, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_GetVolume(int dev, out int volume, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallMake([In] CORESIP_CallInfo info, [In] CORESIP_CallOutInfo outInfo, out int call, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallHangup(int call, int code, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallAnswer(int call, int code, int addToConference, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallHold(int call, int hold, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallTransfer(int call, int dstCall, [In] string dst, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallPtt(int call, [In] CORESIP_PttInfo info, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallConference(int call, int conf, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallSendConfInfo(int call, [In] CORESIP_ConfInfo info, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CallSendInfo(int call, [In] string info, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int	CORESIP_TransferAnswer(string tsxKey, IntPtr txData, IntPtr evSub, int code, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int	CORESIP_TransferNotify(IntPtr evSub, int code, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_SendOptionsMsg([In] string dst, out CORESIP_Error error);

		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CreateWavRecorder([In] string file, out int wavPlayer, out CORESIP_Error error);
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_DestroyWavRecorder(int wavPlayer, out CORESIP_Error error);		
		
		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_CreateWG67Subscription(string dst, ref IntPtr wg67, ref CORESIP_Error error);

		[DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
		static extern int CORESIP_DestroyWG67Subscription(IntPtr wg67, ref CORESIP_Error error);

        /** AGL */
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void WavRemoteEnd(IntPtr obj);
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_Wav2RemoteStart([In] string file, string id, string ip, int port, ref WavRemoteEnd cbend, ref CORESIP_Error error);
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_Wav2RemoteEnd(IntPtr obj, ref CORESIP_Error error);

        /* GRABACION VOIP START */
        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_RdPttEvent(bool on, [In] string freqId, int dev,  out CORESIP_Error error);

        [DllImport(coresip, CallingConvention = CallingConvention.Cdecl, CharSet = CharSet.Ansi, ExactSpelling = true)]
        static extern int CORESIP_RdSquEvent(bool on, [In] string freqId, out CORESIP_Error error);
        /* GRABACION VOIP END */

		#endregion
		
#if _ED137_
		// PlugTest FAA 05/2011
		/// <summary>
		/// 
		/// </summary>
		public static event UpdateOvrCallMembersCb UpdateOvrCallMembers
		{
			add { _Cb.OnUpdateOvrCallMembers += value; }
			remove { _Cb.OnUpdateOvrCallMembers -= value; }
		}

		// PlugTest FAA 05/2011
		/// <summary>
		/// 
		/// </summary>
		public static event InfoCRDCb InfoCRD
		{
			add { _Cb.OnInfoCrd += value; }
			remove { _Cb.OnInfoCrd -= value; }
		}
#endif		
		/// <summary>
        /// 
        /// </summary>
		public static event InfoReceivedCb InfoReceived
		{
			add { _Cb.OnInfoReceived += value; }
			remove { _Cb.OnInfoReceived -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event KaTimeoutCb KaTimeout
		{
			add { _Cb.OnKaTimeout += value; }
			remove { _Cb.OnKaTimeout -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event RdInfoCb RdInfo
		{
			add { _Cb.OnRdInfo += value; }
			remove { _Cb.OnRdInfo -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event CallStateCb CallState
		{
			add { _Cb.OnCallState += value; }
			remove { _Cb.OnCallState -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event CallIncomingCb CallIncoming
		{
			add { _Cb.OnCallIncoming += value; }
			remove { _Cb.OnCallIncoming -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event TransferRequestCb TransferRequest
		{
			add { _Cb.OnTransferRequest += value; }
			remove { _Cb.OnTransferRequest -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event TransferStatusCb TransferStatus
		{
			add { _Cb.OnTransferStatus += value; }
			remove { _Cb.OnTransferStatus -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
		public static event ConfInfoCb ConfInfo
		{
			add { _Cb.OnConfInfo += value; }
			remove { _Cb.OnConfInfo -= value; }
		}

		public static event WG67NotifyCb WG67Notify
		{
			add { _Cb.OnWG67Notify += value; }
			remove { _Cb.OnWG67Notify -= value; }
		}

        ///
		//public static event ReplaceRequestCb ReplaceRequest
		//{
		//   add { _Cb.OnReplaceRequest += value; }
		//   remove { _Cb.OnReplaceRequest -= value; }
		//}
        /// <summary>
        /// 
        /// </summary>
		public static event OptionsReceiveCb OptionsReceive
		{
			add { _Cb.OnOptionsReceive += value; }
			remove { _Cb.OnOptionsReceive -= value; }
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId">Identificador del Agente (string libre)</param>
        /// <param name="ip">Direccion IP del Agente para las sesiones SIP.</param>
        /// <param name="port">Puerto UDP del Agente para las sesiones SIP.</param>
		public static void Init(string accId, string ip, uint port)
		{
            RcsHfSimulator.Properties.Settings Settings = RcsHfSimulator.Properties.Settings.Default;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.Init");
#endif
			_Ip = ip;
			_Port = port;

			if (Settings.SipLogLevel <= 3)
			{
				// AGL... _Cb.OnLog += OnLogCb;
                _Cb.OnLog = OnLogCb;
            }

			CORESIP_Config cfg = new CORESIP_Config();

            cfg.HostId = accId;     //GRABACION VOIP
			cfg.IpAddress = ip;
			cfg.Port = port;
			cfg.Cb = _Cb;
			cfg.DefaultCodec = Settings.DefaultCodec;
			cfg.DefaultDelayBufPframes = Settings.DefaultDelayBufPframes;
			cfg.DefaultJBufPframes = Settings.DefaultJBufPframes;
			cfg.SndSamplingRate = Settings.SndSamplingRate;
			cfg.RxLevel = Settings.RxLevel;
			cfg.TxLevel = Settings.TxLevel;
			cfg.LogLevel = Settings.SipLogLevel;
			cfg.TsxTout = Settings.TsxTout;
			cfg.InvProceedingIaTout = Settings.InvProceedingIaTout;
			cfg.InvProceedingMonitoringTout = Settings.InvProceedingMonitoringTout;
			cfg.InvProceedingDiaTout = Settings.InvProceedingDiaTout;
			cfg.InvProceedingRdTout = Settings.InvProceedingRdTout;

            // AGL 20131121.
            cfg.EchoTail = Settings.EchoTail;
            cfg.EchoLatency = Settings.EchoLatency;
            // FM           

            /// JCAM 18/01/2016
            /// Grabación según norma ED-137
            cfg.RecordingEd137 = Settings.RecordingEd137;

			CORESIP_Error err;
			if (CORESIP_Init(cfg, out err) != 0)
			{
				throw new Exception(err.Info);
			}

			CORESIP_Params sipParams = new CORESIP_Params();

			sipParams.EnableMonitoring = 0;
			sipParams.KeepAlivePeriod = Settings.KAPeriod;
			sipParams.KeepAliveMultiplier = Settings.KAMultiplier;

			SetParams(sipParams);

			int id;
			string sipAcc = string.Format("<sip:{0}@{1}:{2}>", accId, ip, port);

			if (CORESIP_CreateAccount(sipAcc, 1, out id, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.Init");
#endif

		}
        /// <summary>
        /// 
        /// </summary>
		public static void Start()
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.Start");
#endif
            CORESIP_Error err;
			if (CORESIP_Start(out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.Start");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
		public static void End()
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.End");
#endif
            CORESIP_End();
			_Accounts.Clear();

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.End");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
		public static void SetLogLevel(LogLevel level)
		{
			uint eqLevel = 0;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SetLogLevel {0}", level.Name);
#endif
            if (level == LogLevel.Fatal)
			{
				eqLevel = 1;
			}
			else if (level == LogLevel.Error)
			{
				eqLevel = 2;
			}
			else if (level == LogLevel.Warn)
			{
				eqLevel = 3;
			}
			else if (level == LogLevel.Info)
			{
				eqLevel = 4;
			}
			else if (level == LogLevel.Debug)
			{
				eqLevel = 5;
			}
			else if (level == LogLevel.Trace)
			{
				eqLevel = 6;
			}
			CORESIP_Error err;
			CORESIP_SetLogLevel(eqLevel, out err);
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SetLogLevel");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="cfg"></param>
		public static void SetParams(CORESIP_Params cfg)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SetParmas");
#endif
            CORESIP_Error err;
			CORESIP_SetParams(cfg, out err);
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SetParams");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
		public static void CreateAccount(string accId)
		{
			int id;
			CORESIP_Error err;
			string sipAcc = string.Format("<sip:{0}@{1}:{2}>", accId, _Ip, _Port);

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateAccount");
#endif
            if (CORESIP_CreateAccount(sipAcc, 0, out id, out err) != 0)
			{
				throw new Exception(err.Info);
			}
			_Accounts[accId] = id;
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateAccount");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        public static void CreateAccountAndRegisterInProxy(string accId, string proxy_ip, uint expire_seg, string username, string pass)
        {
            int id;
            CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateAccountAndRegisterInProxy");
#endif
            if (CORESIP_CreateAccountAndRegisterInProxy(accId, 0, out id, proxy_ip, expire_seg, username, pass, out err) != 0)
            {
                throw new Exception(err.Info);
            }
            _Accounts[accId] = id;
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateAccountAndRegisterInProxy");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
		public static void DestroyAccounts()
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyAccounts");
#endif
            foreach (int id in _Accounts.Values)
			{
				CORESIP_Error err;

				if (CORESIP_DestroyAccount(id, out err) != 0)
				{
                    _Logger.Error("SipAgent.DestroyAccounts: " + err.Info);
				}
			}
			_Accounts.Clear();
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyAccounts");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="inDevId"></param>
        /// <param name="outDevId"></param>
        /// <returns></returns>
		public static int AddSndDevice(CORESIP_SndDevType type, int inDevId, int outDevId)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AddSndDevice {0}, {1}, {2}", type.ToString(), inDevId, outDevId);
#endif
            CORESIP_SndDeviceInfo info = new CORESIP_SndDeviceInfo();

			info.Type = type;
			info.OsInDeviceIndex = inDevId;
			info.OsOutDeviceIndex = outDevId;

			int dev=0;
			CORESIP_Error err;

            if (CORESIP_AddSndDevice(info, out dev, out err) != 0)            
            {            
                throw new Exception(err.Info);                
            }

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AddSndDevice");
#endif
            return dev;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="file"></param>
        /// <param name="loop"></param>
        /// <returns></returns>
		public static int CreateWavPlayer(string file, bool loop)
		{
			int wavPlayer;
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateWavPlayer {0}, {1}", file, loop);
#endif

			if (CORESIP_CreateWavPlayer(file, loop ? 1 : 0, out wavPlayer, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateWavPlayer");
#endif
            return wavPlayer;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wavPlayer"></param>
		public static void DestroyWavPlayer(int wavPlayer)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyWavPlayer {0}", wavPlayer);
#endif

			if (CORESIP_DestroyWavPlayer(wavPlayer, out err) != 0)
			{
                _Logger.Error("SipAgent.DestroyWavPlayer: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyWavPlayer");
#endif
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="file"></param>
		public static int CreateWavRecorder(string file)
		{
			int wavRecorder;
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateWavRecorder {0}", file);
#endif

			if (CORESIP_CreateWavRecorder(file, out wavRecorder, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateWavRecorder");
#endif
            return wavRecorder;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wavRecorder"></param>
		public static void DestroyWavRecorder(int wavRecorder)
		{
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyWavRecorder {0}", wavRecorder);
#endif
            if (CORESIP_DestroyWavRecorder(wavRecorder, out err) != 0)
			{
				_Logger.Error("SipAgent.DestroyWavRecorder: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyWavRecorder");
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="rs"></param>
        /// <param name="localIp"></param>
        /// <returns></returns>
//        public static int CreateRdRxPort(RdSrvRxRs rs, string localIp)
//        {
//#if _TRACEAGENT_
//            _Logger.Debug("Entrando en SipAgent.CreateRdRxPort {0}, {1}", rs.ToString(), localIp);
//#endif
//            CORESIP_RdRxPortInfo info = new CORESIP_RdRxPortInfo();

//            info.ClkRate = rs.ClkRate;
//            info.ChannelCount = rs.ChannelCount;
//            info.BitsPerSample = rs.BitsPerSample;
//            info.FrameTime = rs.FrameTime;
//            info.Ip = rs.McastIp;
//            info.Port = rs.RdRxPort;

//            int mcastPort;
//            CORESIP_Error err;

//            if (CORESIP_CreateRdRxPort(info, localIp, out mcastPort, out err) != 0)
//            {
//                throw new Exception(err.Info);
//            }
//#if _TRACEAGENT_
//            _Logger.Debug("Saliendo de SipAgent.CreateRdRxPort");
//#endif

//            return mcastPort;
//        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
		public static void DestroyRdRxPort(int port)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyRdRxPort {0}", port);
#endif

			if (CORESIP_DestroyRdRxPort(port, out err) != 0)
			{
                _Logger.Error("SipAgent.DestroyRdrxPort: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyRdRxPort");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
		public static int CreateSndRxPort(string name)
		{
			int sndRxPort;
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateSndRxPort {0}", name);
#endif
            if (CORESIP_CreateSndRxPort(name, out sndRxPort, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateSndRxPort");
#endif
			return sndRxPort;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="port"></param>
		public static void DestroySndRxPort(int port)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroySndRxPort {0}", port);
#endif
            if (CORESIP_DestroySndRxPort(port, out err) != 0)
			{
                _Logger.Error("SipAgent.DestroySndRxPort: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroySndRxPort");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcId"></param>
        /// <param name="dstId"></param>
		public static void MixerLink(int srcId, int dstId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MixerLink {0}, {1}", srcId, dstId);
#endif
			if (CORESIP_BridgeLink(srcId, dstId, 1, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MixerLink");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="srcId"></param>
        /// <param name="dstId"></param>
		public static void MixerUnlink(int srcId, int dstId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MixerUnLink {0},{1}", srcId, dstId);
#endif
            if (CORESIP_BridgeLink(srcId, dstId, 0, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MixerUnLink");
#endif
        }
        /// <summary>
        /// Usado para la transmision del programa HMI hacia el nodebox-master. Cuando se hace PTT.
        /// </summary>
        /// <param name="sndDevId">Identificador de dispositivo asociado a un microfono</param>
        /// <param name="id">Identificador asociado al nodebox-master.</param>
        /// <param name="ip">Direccion mcast en la que escucha el nodebox-master</param>
        /// <param name="port">Puesto UDP asociado al grupo mcast donde escucha el nodebox-master.</param>
		public static void SendToRemote(int sndDevId, string id, string ip, uint port)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendToRemte {0}, {1}, {2}, {3}", sndDevId, id, ip, port);
#endif
            if (CORESIP_SendToRemote(sndDevId, 1, id, ip, port, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendtoRemote");
#endif
        }
        /// <summary>
        /// Usado por el nodebox-master para recibir tramas del HMI....
        /// </summary>
        /// <param name="localIp"></param>
        /// <param name="mcastIp"></param>
        /// <param name="mcastPort"></param>
		public static void ReceiveFromRemote(string localIp, string mcastIp, uint mcastPort)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.ReceiveFromRemote {0}, {1}, {2}", localIp, mcastIp, mcastPort);
#endif
            if (CORESIP_ReceiveFromRemote(localIp, mcastIp, mcastPort, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.ReceiveFromRemote");
#endif
        }
        /// <summary>
        /// Usado por el HMI para 'redireccionar' su transmision. YA no va hacia el nodebox-master. (por ejemplo para cuando utiliza telefonia).
        /// </summary>
        /// <param name="sndDevId">Identificador del dispositivo asociado a un micrófono.</param>
		public static void UnsendToRemote(int sndDevId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.UnsendToRemote {0}", sndDevId);
#endif
            if (CORESIP_SendToRemote(sndDevId, 0, null, null, 0, out err) != 0)
			{
                _Logger.Error("SipAgent.UnsendToRemote: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.UnsendToRemote");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="volume"></param>
		public static void SetVolume(int id, int volume)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SetVolume {0}, {1}", id, volume);
#endif
            if (CORESIP_SetVolume(id, volume, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SetVolume");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
		public static int GetVolume(int id)
		{
			int volume;
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.GetVolume {0}", id);
#endif
            if (CORESIP_GetVolume(id, out volume, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.GetVolume");
#endif
            return volume;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="referBy"></param>
        /// <param name="priority"></param>
        /// <returns></returns>
		public static int MakeTlfCall(string accId, string dst, string referBy, CORESIP_Priority priority)
		{
            int retorno = MakeTlfCall(accId, dst, referBy, priority, CORESIP_CallFlags.CORESIP_CALL_NO_FLAGS);
            return retorno;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="referBy"></param>
        /// <param name="priority"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
		public static int MakeTlfCall(string accId, string dst, string referBy, CORESIP_Priority priority, CORESIP_CallFlags flags)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakTlfCall {0}, {1}, {2}, {3}, {4}", accId, dst, referBy, priority, flags);
#endif
            int acc;
			if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = CORESIP_CallType.CORESIP_CALL_DIA;
			info.Priority = priority;
			info.Flags = flags;

			outInfo.DstUri = dst;
			outInfo.ReferBy = referBy;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
				throw new Exception(err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeTlfCall");
#endif
            return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
		public static int MakeLcCall(string accId, string dst)
		{
			int acc;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakLcCall {0}, {1}", accId, dst);
#endif
            if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = CORESIP_CallType.CORESIP_CALL_IA;
			info.Priority = CORESIP_Priority.CORESIP_PR_URGENT;

			outInfo.DstUri = dst;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeLcCall");
#endif
			return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="frecuency"></param>
        /// <param name="flags"></param>
        /// <param name="mcastIp">Grupo Multicast de Recepcion para los HMI.</param>
        /// <param name="mcastPort">Puerto del grupo asociado al recurso radio.</param>
        /// <returns></returns>
		public static int MakeRdCall(string accId, string dst, string frecuency, CORESIP_CallFlags flags, string mcastIp, uint mcastPort,
            CORESIP_Priority prioridad=CORESIP_Priority.CORESIP_PR_NORMAL)
		{
			int acc;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakRdCall {0}, {1}, {2}, {3}, {4}, {5}, {6}", accId, dst, frecuency, flags, mcastIp,mcastPort,prioridad);
#endif
            if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = CORESIP_CallType.CORESIP_CALL_RD;
            /* AGL*/
            info.Priority = prioridad;  // CORESIP_Priority.CORESIP_PR_EMERGENCY;
			info.Flags = flags;
#if _VOTER_
			/** 20160609 */
            info.PreferredBss = 0;          // Globales.IndexOfPreferredBss;
            info.PreferredCodec = 0;        // Globales.IndexOfPreferredCodec;
#endif

			outInfo.DstUri = dst;
			outInfo.RdFr = frecuency;
			outInfo.RdMcastAddr = mcastIp;
			outInfo.RdMcastPort = mcastPort;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
                _Logger.Error("SipAgent.MakeRdCall: [" + outInfo.DstUri + "]" +  err.Info);
				callId = -1;
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeRdCall");
#endif
			return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <returns></returns>
		public static int MakeMonitoringCall(string accId, string dst)
		{
            return MakeMonitoringCall(accId, dst, CORESIP_CallType.CORESIP_CALL_MONITORING);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="accId"></param>
        /// <param name="dst"></param>
        /// <param name="type"></param>
        /// <returns></returns>
		public static int MakeMonitoringCall(string accId, string dst, CORESIP_CallType type)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.MakeMonitoringCall {0}, {1}, {2}", accId, dst, type);
#endif
            int acc;
			if (string.IsNullOrEmpty(accId) || !_Accounts.TryGetValue(accId, out acc))
			{
				acc = -1;
			}

			CORESIP_CallInfo info = new CORESIP_CallInfo();
			CORESIP_CallOutInfo outInfo = new CORESIP_CallOutInfo();

			info.AccountId = acc;
			info.Type = type;
			info.Priority = CORESIP_Priority.CORESIP_PR_NONURGENT;  // .CORESIP_PR_NORMAL;

			outInfo.DstUri = dst;

			int callId;
			CORESIP_Error err;

			if (CORESIP_CallMake(info, outInfo, out callId, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.MakeMonitoringCall");
#endif
			return callId;
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void HangupCall(int callId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.HangupCall {0}", callId);
#endif
            if (CORESIP_CallHangup(callId, 0, out err) != 0)
			{
                _Logger.Error("SipAgent.HangupCall_1: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.HangupCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="code"></param>
		public static void HangupCall(int callId, int code)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.HangupCall {0}, {1}", callId, code);
#endif
            if (CORESIP_CallHangup(callId, code, out err) != 0)
			{
                _Logger.Error("SipAgent.HangupCall_2: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.HangupCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="response"></param>
		public static void AnswerCall(int callId, int response)
		{
			AnswerCall(callId, response, false);
		}
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="response"></param>
        /// <param name="addToConference"></param>
		public static void AnswerCall(int callId, int response, bool addToConference)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AnswerCall {0}, {1}, {2}", callId, response, addToConference);
#endif
            if (CORESIP_CallAnswer(callId, response, addToConference ? 1 : 0, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AnswerCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void HoldCall(int callId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.HoldCall {0}", callId);
#endif
            if (CORESIP_CallHold(callId, 1, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.HoldCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void UnholdCall(int callId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.UnholdCall {0}", callId);
#endif

			if (CORESIP_CallHold(callId, 0, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.UnholdCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="dstCallId"></param>
        /// <param name="dst"></param>
		public static void TransferCall(int callId, int dstCallId, string dst)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.TransferCall {0}, {1}, {2}", callId, dstCallId, dst);
#endif
            if (CORESIP_CallTransfer(callId, dstCallId, dst, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.TransferCall");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="pttId"></param>
        /// <param name="pttType"></param>
		public static void PttOn(int callId, ushort pttId, CORESIP_PttType pttType)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.PttOn {0}, {1}, {2}", callId, pttId, pttType);
#endif
            CORESIP_PttInfo info = new CORESIP_PttInfo();

			info.PttType = pttType;
			info.PttId = pttId;

			CORESIP_Error err;

			if (CORESIP_CallPtt(callId, info, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.PttOn");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void PttOff(int callId)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.PttOff {0}", callId);
#endif
            CORESIP_PttInfo info = new CORESIP_PttInfo();
			info.PttType = CORESIP_PttType.CORESIP_PTT_OFF;

			CORESIP_Error err;

			if (CORESIP_CallPtt(callId, info, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.PttOff");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void AddCallToConference(int callId)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.AddCallToConference {0}", callId);
#endif
            if (CORESIP_CallConference(callId, 1, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.AddCallToConference");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
		public static void RemoveCallFromConference(int callId)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.RemoveCallFromConference {0}", callId);
#endif
            if (CORESIP_CallConference(callId, 0, out err) != 0)
			{
                _Logger.Error("SipAgent.RemoveCallFromConference: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.RemoveCallFromConference");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="info"></param>
		public static void SendConfInfo(int callId, CORESIP_ConfInfo info)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendConfInfo {0}, {1}", callId, info.Version);
#endif

			if (CORESIP_CallSendConfInfo(callId, info, out err) != 0)
			{
                _Logger.Error("SipAgent.SendConfInfo: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendConfInfo");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="callId"></param>
        /// <param name="info"></param>
		public static void SendInfo(int callId, string info)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendInfo {0}, {1}", callId, info);
#endif
            if (CORESIP_CallSendInfo(callId, info, out err) != 0)
			{
                _Logger.Error("SipAgent.SendInfo: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendInfo");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="dst"></param>
		public static void SendOptionsMsg(string dst)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.SendOptionsMsg {0}", dst);
#endif
            if (CORESIP_SendOptionsMsg(dst, out err) != 0)
			{
				throw new Exception(err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.SendOptionsMsg");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="tsxKey"></param>
        /// <param name="txData"></param>
        /// <param name="evSub"></param>
        /// <param name="code"></param>
		public static void TransferAnswer(string tsxKey, IntPtr txData, IntPtr evSub, int code)
		{
			CORESIP_Error err;

#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.TransferAnswer {0}, {1}, {2}, {3}", tsxKey, txData, evSub, code);
#endif
            if (CORESIP_TransferAnswer(tsxKey, txData, evSub, code, out err) != 0)
			{
                _Logger.Error("SipAgent.TransferAnswer: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.TransferAnswer");
#endif
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="evSub"></param>
        /// <param name="code"></param>
		public static void TransferNotify(IntPtr evSub, int code)
		{
			CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.Transfer {0}, {1}", evSub, code);
#endif

			if (CORESIP_TransferNotify(evSub, code, out err) != 0)
			{
                _Logger.Error("SipAgent.TransferNotify: " + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.TransferNotify");
#endif
        }

		/// <summary>
		/// 
		/// </summary>
		/// <param name="dst"></param>
		/// <returns></returns>
		public static IntPtr CreateWG67Subscription(string dst)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.CreateWG67Subscription {0}", dst);
#endif
            IntPtr wg67 = IntPtr.Zero;
			CORESIP_Error err = new CORESIP_Error();

			if (CORESIP_CreateWG67Subscription(dst, ref wg67, ref err) != 0)
			{
				_Logger.Error("Error creating WG67 KEY-IN Subscription" + err.Info);
			}

#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.CreateWG67Subcription");
#endif
            return wg67;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="wg67"></param>
		public static void DestroyWG67Subscription(IntPtr wg67)
		{
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.DestroyWG67Subscription {0}", wg67);
#endif
            CORESIP_Error err = new CORESIP_Error();

			if (CORESIP_DestroyWG67Subscription(wg67, ref err) != 0)
			{
				_Logger.Error("Error destroying WG67 KEY-IN Subscription" + err.Info);
			}
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.DestroyWG67Subscription");
#endif
        }

        /** AGL */
        public static void Wav2Remote(string file, string id, string ip, int port)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.WavToRemote {0}, {1}, {2}, {3}", file, id, ip, port);
#endif
            CORESIP_Error err = new CORESIP_Error();
            WavRemoteEnd cb = new WavRemoteEnd(Wav2RemoteEnd);
            CORESIP_Wav2RemoteStart(file, id, ip, port, ref cb, ref err);
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.Wav2Remote");
#endif
        }

        /** */
        public static void Wav2RemoteEnd(IntPtr obj)
        {
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.WavToRemoteEnd {0}", obj);
#endif
            CORESIP_Error err = new CORESIP_Error();
            CORESIP_Wav2RemoteEnd(obj, ref err);
            /** Meter un Evento.... */
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.WavToRemoteEnd");
#endif
        }

        /* GRABACION VOIP START */
        /** */
        public static void RdPttEvent(bool on, string freqId, int dev)
        {
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.RdPttEvent {0}, {1}", freqId, dev);
#endif
            if (CORESIP_RdPttEvent(on, freqId, dev, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.RdPttEvent");
#endif
        }

        public static void RdSquEvent(bool on, string freqId)
        {
            CORESIP_Error err;
#if _TRACEAGENT_
            _Logger.Debug("Entrando en SipAgent.RdSquEvent {0}", freqId);
#endif
            if (CORESIP_RdSquEvent(on, freqId, out err) != 0)
            {
                throw new Exception(err.Info);
            }
#if _TRACEAGENT_
            _Logger.Debug("Saliendo de SipAgent.RdSquEvent");
#endif
        }

        /* GRABACION VOIP END */

		#region Private Members
        /// <summary>
        /// 
        /// </summary>
		private static Logger _Logger = LogManager.GetCurrentClassLogger();
        /// <summary>
        /// 
        /// </summary>
		private static CORESIP_Callbacks _Cb = new CORESIP_Callbacks();
        /// <summary>
        /// 
        /// </summary>
		private static Dictionary<string, int> _Accounts = new Dictionary<string, int>();
        /// <summary>
        /// 
        /// </summary>
		private static string _Ip = null;
        public static string IP
        {
            get { return _Ip; }
        }
        /// <summary>
        /// 
        /// </summary>
		private static uint _Port = 0;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="level"></param>
        /// <param name="data"></param>
        /// <param name="len"></param>
		private static void OnLogCb(int level, string data, int len)
		{
			LogLevel eqLevel = level < 7 ? LogLevel.FromOrdinal(6 - level) : LogLevel.Off;

            //_Logger.Log(eqLevel, data);
            _Logger.Debug(data);
		}

		#endregion
	}
}
