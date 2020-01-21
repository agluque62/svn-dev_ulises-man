using System;
using System.Collections.Generic;
using System.Xml.Serialization;

using Utilities;
using SactaSectionHandler;
//using SactaModule.Properties;

namespace Sacta
{
	class PSIInfo
	{
		public int LastSectMsgId;
		public uint LastSectVersion;
	}

	[Serializable]
	class SactaMsg
	{
		public enum MsgType : ushort { Init = 0, SectAsk = 707, SectAnwer = 710, Presence = 1530, Sectorization = 1632 }

		public const ushort InitId = 0x4000;

		[Serializable]
		[XmlInclude(typeof(PresenceInfo))]
		[XmlInclude(typeof(SectInfo))]
		[XmlInclude(typeof(SectAnswerInfo))]
		public class DataInfoBase { }

		[Serializable]
		public class PresenceInfo : DataInfoBase
		{
			public ushort NumTPChars = 3;

			[SerializeAs(Length = 10, ElementSize = 1)]
			public byte[] ProcessorType = new byte[10] { 0x53, 0x43, 0x56, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

			public ushort ProcessorNumber = 1; //Settings.Default.ProcessorNumber;
			public ushort Reserved = 0;
			public byte ProcessorState = 1;
			public byte ProcessorSubState = 0;
			public ushort PresencePerioditySg = (ushort)(CfgSacta.CfgTimeouts.Presencia / 1000);	// Settings.Default.PresenceInterval / 1000);
			public ushort ActivityTimeOutSg = (ushort)(CfgSacta.CfgTimeouts.TimeOutActividad/1000);	// Settings.Default.ActivityTimeOut / 1000);
		}

		[Serializable]
		public class SectInfo : DataInfoBase
		{
			[Serializable]
			public class SectorInfo
			{
				public byte PCVSacta = 0;
				public byte Reserved = 0;

				[SerializeAs(Size = 4)]
				public string SectorCode = "";

				public byte Ucs = 0;
				public byte UcsType = 0;
			}

			public uint Version = 0;
			public ushort Reserved = 0;
			public ushort NumSectors = 0;

			[SerializeAs(LengthField = "NumSectors")]
			public SectorInfo[] Sectors = new SectorInfo[0];
		}

		[Serializable]
		public class SectAnswerInfo : DataInfoBase
		{
			public uint Version = 0;
			public byte Result = 0;
			public byte Reserved = 0;
		}

		public byte DomainOrg = (byte)CfgSacta.CfgSactaDominio.Origen;			// Settings.Default.ScvDomain;
		public byte CenterOrg = (byte)CfgSacta.CfgSactaCentro.Origen;				// Settings.Default.ScvCenter;
		public ushort UserOrg = (ushort)CfgSacta.CfgSactaUsuarioSettings.Origen;	// Settings.Default.ScvUser;
		public byte DomainDst = (byte)CfgSacta.CfgSactaDominio.Destino;			// Settings.Default.SactaDomain;
		public byte CenterDst = (byte)CfgSacta.CfgSactaCentro.Destino;			// Settings.Default.SactaCenter;
		public ushort UserDst = (ushort)CfgSacta.CfgSactaUsuarioSettings.Grupo;		// Settings.Default.SactaGroupUser;
		public ushort Session = 0;
		public MsgType Type;
		public ushort Id;
		public ushort Length;
		public uint Hour;

		[SerializeAs(RuntimeFieldType = "GetRuntimeParamType")]
		public DataInfoBase Info;

		public Type GetRuntimeParamType()
		{
			switch (Type)
			{
				case MsgType.Init:
				case MsgType.SectAsk:
					return typeof(DataInfoBase);
				case MsgType.Presence:
					return typeof(PresenceInfo);
				case MsgType.Sectorization:
					return typeof(SectInfo);
				case MsgType.SectAnwer:
					return typeof(SectAnswerInfo);
				default:
					throw new Exception("Invalid SactaMsg type (" + (int)Type + ")");
			}
		}

		public SactaMsg(MsgType type, ushort id)
		{
			Type = type;
			Id = id;
			Hour = (uint)(DateTime.Now - new DateTime(1970, 1, 1)).TotalSeconds;

			switch (type)
			{
				case MsgType.Presence:
					Length = 11;
					Info = new PresenceInfo();
					break;
				case MsgType.SectAnwer:
					Length = 3;
					Info = new SectAnswerInfo();
					break;
			}
		}
	}
}
