using System;
using System.Xml.Serialization;

using SimulSACTA.Properties;
using Utilities;

namespace SimulSACTA
{
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
            public byte[] ProcessorType = new byte[10] { 0x50, 0x53, 0x49, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00 };

            public ushort ProcessorNumber = Settings.Default.ProcessorNumber;
            public ushort Reserved = 0;
            public byte ProcessorState = 1;
            public byte ProcessorSubState = 0;
            public ushort PresencePerioditySg = (ushort)(Settings.Default.PresenceInterval / 1000);
            public ushort ActivityTimeOutSg = (ushort)(Settings.Default.ActivityTimeOut / 1000);
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
                public string SectorCode;

                public byte Ucs;
                public byte UcsType = 2;
            }

            public uint Version;
            public ushort Reserved = 0;
            public ushort NumSectors;

            [SerializeAs(LengthField = "NumSectors")]
            public SectorInfo[] Sectors;

            public SectInfo(uint version, string[] sectorUcs)
            {
                Version = version;
                NumSectors = (ushort)(sectorUcs.Length / 2);
                Sectors = new SectorInfo[NumSectors];

                for (int i = 0; i < NumSectors; i++)
                {
                    Sectors[i] = new SectorInfo();
                    Sectors[i].SectorCode = sectorUcs[i * 2];
                    Sectors[i].Ucs = Byte.Parse(sectorUcs[(i * 2) + 1]);
                }
            }
        }

        [Serializable]
        public class SectAnswerInfo : DataInfoBase
        {
            public uint Version = 0;
            public byte Result = 0;
            public byte Reserved = 0;
        }

        public byte DomainOrg = Settings.Default.SactaDomain;
        public byte CenterOrg = Settings.Default.SactaCenter;
        public ushort UserOrg = Settings.Default.SactaSPVUser;
        public byte DomainDst = Settings.Default.ScvDomain;
        public byte CenterDst = Settings.Default.ScvCenter;
        public ushort UserDst = 0;
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
            : this(type, id, 0, null)
        {
        }

        public SactaMsg(MsgType type, ushort id, uint sectVersion, string[] sectorUcs)
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
                case MsgType.Sectorization:
                    UserOrg = Settings.Default.SactaSPSIUser;
                    Info = new SectInfo(sectVersion, sectorUcs);
                    Length = (ushort)(4 + (4 * ((SectInfo)Info).NumSectors));
                    break;
            }
        }
    }
}
