using System;
using System.IO;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;

namespace Utilities
{
	[AttributeUsage(AttributeTargets.Field, Inherited = false)]
	public sealed class SerializeAsAttribute : Attribute
	{
		string _Encoding = "us-ascii";
		bool _NetEndian = true;

		int _Offset = 0;
		int _Size = -1;
		string _SizeField;
		int _Length = -1;
		string _LengthField;
		int _ElementSize = -1;
		string _ElementSizeField;
		string _RuntimeFieldType;
		Type _FieldType;

		Type _ParentType;
		object _ParentObject;

		public string Encoding
		{
			get { return _Encoding; }
			set { _Encoding = value; }
		}

		public bool NetEndian
		{
			get { return _NetEndian; }
			set { _NetEndian = value; }
		}

		public int Offset
		{
			get { return _Offset; }
			set { _Offset = value; }
		}

		public int Size
		{
			get 
			{ 
				if ((_Size < 0) && (SizeField != null))
				{
					FieldInfo info = _ParentType.GetField(SizeField);
               _Size = Math.Max(Int32.Parse(info.GetValue(_ParentObject).ToString()), -1);
					SizeField = null;
				}

				return _Size;
			}
			set { _Size = value; }
		}

		public string SizeField
		{
			get { return _SizeField; }
			set { _SizeField = value; }
		}

		public int Length
		{
			get
			{
				if ((_Length < 0) && (LengthField != null))
				{
					FieldInfo info = _ParentType.GetField(LengthField);
               _Length = Math.Max(Int32.Parse(info.GetValue(_ParentObject).ToString()), -1);
					LengthField = null;
				}

				return _Length;
			}
			set { _Length = value; }
		}

		public string LengthField
		{
			get { return _LengthField; }
			set { _LengthField = value; }
		}

		public int ElementSize
		{
			get
			{
				if ((_ElementSize < 0) && (ElementSizeField != null))
				{
					FieldInfo info = _ParentType.GetField(ElementSizeField);
               _ElementSize = Math.Max(Int32.Parse(info.GetValue(_ParentObject).ToString()), -1);
					ElementSizeField = null;
				}

				return _ElementSize;
			}
			set { _ElementSize = value; }
		}

		public string ElementSizeField
		{
			get { return _ElementSizeField; }
			set { _ElementSizeField = value; }
		}

		public string RuntimeFieldType
		{
			get { return _RuntimeFieldType; }
			set { _RuntimeFieldType = value; }
		}

		public Type FieldType
		{
			get
			{
				if (RuntimeFieldType != null)
				{
					_FieldType = (Type)_ParentType.GetMethod(RuntimeFieldType).Invoke(_ParentObject, null);
					RuntimeFieldType = null;
				}

				return _FieldType;
			}
		}

		public void SetInfo(Type parentType, object parentObj)
		{
			_ParentType = parentType;
			_ParentObject = parentObj;
		}
	}

	public sealed class CustomBinaryFormatter : IFormatter, IDisposable
	{
		enum EValueType
		{
			Invalid = 0,
			Boolean,
			Byte,
			SByte,
			Int16,
			UInt16,
			Int32,
			UInt32,
			Int64,
			UInt64,
			Single,
			Double,
			Decimal,
			Char,
			String,
			TimeSpan,
			DateTime
		}

		EValueType[] _Codes = new EValueType[] { EValueType.Invalid, EValueType.Invalid, EValueType.Invalid,
			EValueType.Boolean, EValueType.Char, EValueType.SByte, EValueType.Byte, EValueType.Int16,
			EValueType.UInt16, EValueType.Int32, EValueType.UInt32, EValueType.Int64, EValueType.UInt64,
			EValueType.Single, EValueType.Double, EValueType.Decimal, EValueType.DateTime, EValueType.Invalid, EValueType.String };

		Type _TypeOfObject = typeof(object);
		Type _TypeOfTimeSpan = typeof(TimeSpan);

		StreamingContext _Contex;
		SerializeAsAttribute _DefaultAttr;

		byte[] _NullData = new byte[512];
		Encoding _DefaultEncoding;

		public CustomBinaryFormatter() : this(null) { }

      public CustomBinaryFormatter(SerializeAsAttribute defaultAttr)
      {
         _DefaultAttr = defaultAttr != null ? defaultAttr : new SerializeAsAttribute();
         _Contex = new StreamingContext(StreamingContextStates.Clone | StreamingContextStates.File | StreamingContextStates.Persistence);
         if (_DefaultAttr.Encoding != "BCD")
         {
            _DefaultEncoding = Encoding.GetEncoding(_DefaultAttr.Encoding);
         }
      }

      ~CustomBinaryFormatter()
      {
         Dispose(false);
      }

      #region IDisposable Members

      public void Dispose()
      {
         Dispose(true);
         GC.SuppressFinalize(this);
      }

      #endregion

      #region IFormatter Members

		public SerializationBinder Binder
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		public StreamingContext Context
		{
			get { return _Contex; }
			set { _Contex = value; }
		}

		public ISurrogateSelector SurrogateSelector
		{
			get { throw new Exception("The method or operation is not implemented."); }
			set { throw new Exception("The method or operation is not implemented."); }
		}

		public void Serialize(Stream serializationStream, object graph)
		{
			if (serializationStream == null)
			{
				throw new ArgumentNullException("serializationStream");
			}
			if (graph == null)
			{
				throw new ArgumentNullException("graph");
			}

			WriteMember(new BinaryWriter(serializationStream), graph.GetType(), graph, _DefaultAttr);
		}

		public object Deserialize(Stream serializationStream)
		{
			throw new Exception("The method or operation is not implemented.");
		}

		#endregion

      void Dispose(bool bDispose)
      {
         if (bDispose)
         {
            _Codes = null;
            _TypeOfObject = null;
            _TypeOfTimeSpan = null;
            _DefaultAttr = null;
            _NullData = null;
            _DefaultEncoding = null;
         }
      }

		public T Deserialize<T>(Stream serializationStream)
		{
			if (serializationStream == null)
			{
				throw new ArgumentNullException("serializationStream");
			}

			object obj;
			ReadMember(new BinaryReader(serializationStream), typeof(T), out obj, _DefaultAttr);

			return (T)obj;
		}

		SerializeAsAttribute GetSerializeAsAttribute(Type parentType, object parentObject, MemberInfo member)
		{
			object[] customAttrs = member.GetCustomAttributes(typeof(SerializeAsAttribute), false);
			if (customAttrs.Length > 0)
			{
				Debug.Assert(customAttrs.Length == 1);

				SerializeAsAttribute attr = (SerializeAsAttribute)customAttrs[0];
				attr.SetInfo(parentType, parentObject);

				return attr;
			}

			return _DefaultAttr;
		}

		int WriteMember(BinaryWriter writer, Type type, object obj, SerializeAsAttribute attr)
		{
			int nullSize = 0;
			int size = 0;

			nullSize += WriteNullData(writer, attr.Offset);

			if (attr.Size == 0)
			{
				return nullSize;
			}

			if (obj != null)
			{
				EValueType code = _Codes[(int)Type.GetTypeCode(type)];

				if ((code != EValueType.Invalid) || (type == _TypeOfTimeSpan))
				{
					size += WriteValue(writer, code, obj, attr);
				}
				else if (type.IsArray)
				{
					size += WriteArray(writer, type, obj, attr);
				}
				else
				{
					size += WriteObject(writer, type, obj, attr);
				}
			}

			if (attr.Size >= 0)
			{
				if (size > attr.Size)
				{
					throw new SerializationException("Object (= " + type + ") size (= " + size + ") rise specified attribute size (=" + attr.Size + ")");
				}

				nullSize += WriteNullData(writer, attr.Size - size);
			}

			return (nullSize + size);
		}

		int WriteNullData(BinaryWriter writer, int numBytes)
		{
			if (numBytes > 0)
			{
				if (numBytes > _NullData.Length)
				{
					Array.Resize(ref _NullData, numBytes);
				}

				writer.Write(_NullData, 0, numBytes);
				return numBytes;
			}

			return 0;
		}

		int WriteObject(BinaryWriter writer, Type type, object obj, SerializeAsAttribute attr)
		{
			int size = 0;

			if (type.BaseType != _TypeOfObject)
			{
				size += WriteObject(writer, type.BaseType, obj, attr);
			}

			MemberInfo[] membersInfo = FormatterServices.GetSerializableMembers(type, _Contex);
			object[] membersData = FormatterServices.GetObjectData(obj, membersInfo);
			Debug.Assert(membersInfo.Length == membersData.Length);

			for (int i = 0; i < membersInfo.Length; i++)
			{
				MemberInfo memberInfo = membersInfo[i];
				Debug.Assert(memberInfo is FieldInfo);

				object memberData = membersData[i];
				Type memberType = memberData != null ? memberData.GetType() : ((FieldInfo)memberInfo).FieldType;
				SerializeAsAttribute memberAttr = GetSerializeAsAttribute(type, obj, memberInfo);

				size += WriteMember(writer, memberType, memberData, memberAttr);
			}

			return size;
		}

		int WriteArray(BinaryWriter writer, Type type, object obj, SerializeAsAttribute attr)
		{
			int size = 0;
			Array ar = (Array)obj;

			if (ar.Rank > 1)
			{
				throw new SerializationException("Multidimensional array not supported (" + type + ")");
			}
			if (attr.Length < 0)
			{
				throw new SerializationException("Array need attribute Length or LengthField");
			}

			SerializeAsAttribute elementAttr = new SerializeAsAttribute();
			elementAttr.Encoding = attr.Encoding;
			elementAttr.Size = attr.ElementSize;

			for (int i = 0; i < attr.Length; i++)
			{
				object element = i < ar.Length ? ar.GetValue(i) : null;
				Type elementType = type.GetElementType();

				if ((element == null) && (elementAttr.Size < 0))
				{
					throw new SerializationException("Null elements in array needs attribute ElementSize or ElementSizeField");
				}

				size += WriteMember(writer, elementType, element, elementAttr);
			}

			return size;
		}

		int WriteValue(BinaryWriter writer, EValueType code, object obj, SerializeAsAttribute attr)
		{
			byte[] data = null;

			switch (code)
			{
				case EValueType.Boolean:
					data = BitConverter.GetBytes((bool)obj);
					break;
				case EValueType.Byte:
				case EValueType.SByte:
					data = new byte[1];
					data[0] = (byte)obj;
					break;
				case EValueType.Int16:
               data = BitConverter.GetBytes((short)obj);
               if (attr.NetEndian) { Array.Reverse(data); };
               break;
            case EValueType.UInt16:
               data = BitConverter.GetBytes((ushort)obj);
               if (attr.NetEndian) { Array.Reverse(data); };
               break;
            case EValueType.Int32:
               data = BitConverter.GetBytes((int)obj);
               if (attr.NetEndian) { Array.Reverse(data); };
               break;
            case EValueType.UInt32:
               data = BitConverter.GetBytes((uint)obj);
               if (attr.NetEndian) { Array.Reverse(data); };
               break;
            case EValueType.Int64:
               data = BitConverter.GetBytes((long)obj);
               if (attr.NetEndian) { Array.Reverse(data); };
               break;
            case EValueType.UInt64:
               data = BitConverter.GetBytes((ulong)obj);
               if (attr.NetEndian) { Array.Reverse(data); };
               break;
				case EValueType.Single:
               data = BitConverter.GetBytes((float)obj);
               break;
            case EValueType.Double:
               data = BitConverter.GetBytes((double)obj);
               break;
            case EValueType.String:
					if (attr.Encoding == "BCD")
					{
						string str = ((string)obj).TrimEnd(' ');

						data = new byte[1 + ((str.Length + 1) / 2)];
						data[0] = (byte)str.Length;

						for (int i = 0, pos = 1; i < str.Length; i++, pos++)
						{
							data[pos] = (byte)(((byte)str[i] - 0x30) * 16);
							if (i < str.Length - 1)
							{
								data[pos] += (byte)((byte)str[++i] - 0x30);
							}
						}
					}
					else
					{
						Encoding sEnc = attr.Encoding != _DefaultAttr.Encoding ? Encoding.GetEncoding(attr.Encoding) : _DefaultEncoding;
						data = sEnc.GetBytes((string)obj);
						if (attr.Size < 0)
						{
							Array.Resize(ref data, data.Length + 1);
						}
					}
					break;
				default:
					throw new SerializationException("Not supported codeType (" + code + ")");
			}

			writer.Write(data, 0, data.Length);
			return data.Length;
		}

		int ReadMember(BinaryReader reader, Type type, out object obj, SerializeAsAttribute attr)
		{
			int nullSize = 0;
			int size = 0;

			nullSize += ReadNullData(reader, attr.Offset);
			obj = null;

			if (attr.Size == 0)
			{
				return nullSize;
			}

			EValueType code = _Codes[(int)Type.GetTypeCode(type)];

			if ((code != EValueType.Invalid) || (type == _TypeOfTimeSpan))
			{
				size += ReadValue(reader, code, out obj, attr);
			}
			else if (type.IsArray)
			{
				size += ReadArray(reader, type, out obj, attr);
			}
			else
			{
				obj = FormatterServices.GetUninitializedObject(attr.FieldType == null ? type : attr.FieldType);
				size += ReadObject(reader, type, ref obj, attr);
			}

			if (attr.Size >= 0)
			{
				if (size > attr.Size)
				{
					throw new SerializationException("Object (= " + type + ") size (= " + size + ") rise specified attribute size (=" + attr.Size + ")");
				}

				nullSize += ReadNullData(reader, attr.Size - size);
			}

			return (nullSize + size);
		}

		int ReadNullData(BinaryReader reader, int numBytes)
		{
			if (numBytes > 0)
			{
				if (numBytes > _NullData.Length)
				{
					Array.Resize(ref _NullData, numBytes);
				}

				reader.Read(_NullData, 0, numBytes);
				return numBytes;
			}

			return 0;
		}

		int ReadObject(BinaryReader reader, Type type, ref object obj, SerializeAsAttribute attr)
		{
			int size = 0;

			if (type.BaseType != _TypeOfObject)
			{
				size += ReadObject(reader, type.BaseType, ref obj, attr);
			}

			MemberInfo[] membersInfo = FormatterServices.GetSerializableMembers(type, _Contex);
			object[] membersData = new object[membersInfo.Length];

			for (int i = 0; i < membersInfo.Length; i++)
			{
				MemberInfo memberInfo = membersInfo[i];
				Debug.Assert(memberInfo is FieldInfo);

				SerializeAsAttribute memberAttr = GetSerializeAsAttribute(type, obj, memberInfo);

				object memberData;
				Type memberType = memberAttr.FieldType == null ? ((FieldInfo)memberInfo).FieldType : memberAttr.FieldType;

				size += ReadMember(reader, memberType, out memberData, memberAttr);
				membersData[i] = memberData;

            obj = FormatterServices.PopulateObjectMembers(obj, membersInfo, membersData);
			}

			return size;
		}

		int ReadArray(BinaryReader reader, Type type, out object obj, SerializeAsAttribute attr)
		{
			int size = 0;


			if (attr.Length < 0)
			{
				throw new SerializationException("Array need attribute Length or LengthField");
			}

			Array ar = Array.CreateInstance(type.GetElementType(), attr.Length);

			SerializeAsAttribute elementAttr = new SerializeAsAttribute();
			elementAttr.Encoding = attr.Encoding;
			elementAttr.Size = attr.ElementSize;

			for (int i = 0; i < attr.Length; i++)
			{
				object element;
				Type elementType = attr.FieldType == null ? type.GetElementType() : attr.FieldType; 

				size += ReadMember(reader, elementType, out element, elementAttr);
				ar.SetValue(element, i);
			}

			obj = ar;
			return size;
		}

		int ReadValue(BinaryReader reader, EValueType code, out object obj, SerializeAsAttribute attr)
		{
			int size = 0;

			switch (code)
			{
				case EValueType.Boolean:
					obj = reader.ReadBoolean();
					size = 1;
					break;
				case EValueType.Byte:
					obj = reader.ReadByte();
					size = 1;
					break;
				case EValueType.SByte:
					obj = reader.ReadSByte();
					size = 1;
					break;
				case EValueType.Int16:
					if (attr.NetEndian)
					{
						byte[] data = reader.ReadBytes(2);
						Array.Reverse(data);
						obj = BitConverter.ToInt16(data, 0);
					}
					else
					{
						obj = reader.ReadInt16();
					}
					size = 2;
					break;
				case EValueType.UInt16:
					if (attr.NetEndian)
					{
						byte[] data = reader.ReadBytes(2);
						Array.Reverse(data);
						obj = BitConverter.ToUInt16(data, 0);
					}
					else
					{
						obj = reader.ReadUInt16();
					}
					size = 2;
					break;
				case EValueType.Int32:
					if (attr.NetEndian)
					{
						byte[] data = reader.ReadBytes(4);
						Array.Reverse(data);
						obj = BitConverter.ToInt32(data, 0);
					}
					else
					{
						obj = reader.ReadInt32();
					}
					size = 4;
					break;
				case EValueType.UInt32:
					if (attr.NetEndian)
					{
						byte[] data = reader.ReadBytes(4);
						Array.Reverse(data);
						obj = BitConverter.ToUInt32(data, 0);
					}
					else
					{
						obj = reader.ReadUInt32();
					}
					size = 4;
					break;
				case EValueType.Int64:
					if (attr.NetEndian)
					{
						byte[] data = reader.ReadBytes(8);
						Array.Reverse(data);
						obj = BitConverter.ToInt64(data, 0);
					}
					else
					{
						obj = reader.ReadInt64();
					}
					size = 8;
					break;
				case EValueType.UInt64:
					if (attr.NetEndian)
					{
						byte[] data = reader.ReadBytes(8);
						Array.Reverse(data);
						obj = BitConverter.ToUInt64(data, 0);
					}
					else
					{
						obj = reader.ReadUInt64();
					}
					size = 8;
					break;
				case EValueType.Single:
					obj = reader.ReadSingle();
					break;
				case EValueType.Double:
					obj = reader.ReadDouble();
					break;
				case EValueType.String:
					if (attr.Encoding == "BCD")
					{
						byte len = reader.ReadByte();
						byte[] data = reader.ReadBytes((len + 1) / 2);
						StringBuilder str = new StringBuilder(len);

						for (int i = 0, j = 0; i < len; i += 2, j++)
						{
							str.Append((char)((data[j] >> 4) + 0x30));
							if (i < len - 1)
							{
								str.Append((char)((data[j] & 0x0F) + 0x30));
							}
						}

						obj = str.ToString();
						size = 1 + ((len + 1) / 2);
					}
					else
					{
						Encoding sEnc = attr.Encoding != _DefaultAttr.Encoding ? Encoding.GetEncoding(attr.Encoding) : _DefaultEncoding;
						if (attr.Size > 0)
						{
							byte[] data = reader.ReadBytes(attr.Size);
                     int len = Array.IndexOf(data, (byte)0);
							obj = sEnc.GetString(data, 0, len >= 0 ? len : data.Length);
                     if (obj == null) obj = "";
							size = attr.Size;
						}
						else
						{
							byte[] data = new byte[0];
							byte ch;

							do
							{
								ch = reader.ReadByte();
								Array.Resize(ref data, data.Length + 1);
								data[data.Length - 1] = ch;

							} while (ch != 0);

							obj = sEnc.GetString(data);
							size = data.Length;
						}
					}
					break;
				default:
					throw new SerializationException("Not supported codeType (" + code + ")");
			}

			return size;
		}
   }
}
