using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Text;

namespace Utilities
{
	public class NLogTraceListener : TraceListener
	{
		private TraceEventType _DefaultTraceEventType = TraceEventType.Verbose;
		private string _LoggerNameFormat = "{listenerName}.{sourceName}";

		public TraceEventType DefaultTraceEventType
		{
			get { return _DefaultTraceEventType; }
			set { _DefaultTraceEventType = value; }
		}

		public string LoggerNameFormat
		{
			get { return _LoggerNameFormat; }
			set { _LoggerNameFormat = value; }
		}

		public NLogTraceListener()
			: this(string.Empty)
		{ }

		public NLogTraceListener(string initializeData)
			: this(GetPropertiesFromInitString(initializeData))
		{
		}

		public NLogTraceListener(NameValueCollection properties)
			: base()
		{
			ApplyProperties(properties);
		}

		public override void Write(object o)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, o, null)))
			{
				Log(DefaultTraceEventType, null, 0, "{0}", o);
			}
		}

		public override void Write(object o, string category)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, o, null)))
			{
				Log(DefaultTraceEventType, category, 0, "{0}", o);
			}
		}

		public override void Write(string message)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, null, null)))
			{
				Log(DefaultTraceEventType, null, 0, message);
			}
		}

		public override void Write(string message, string category)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, null, null)))
			{
				Log(DefaultTraceEventType, category, 0, message);
			}
		}

		public override void WriteLine(object o)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, o, null)))
			{
				Log(DefaultTraceEventType, null, 0, "{0}", o);
			}
		}

		public override void WriteLine(object o, string category)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, o, null)))
			{
				Log(DefaultTraceEventType, category, 0, "{0}", o);
			}
		}

		public override void WriteLine(string message)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, null, null)))
			{
				Log(DefaultTraceEventType, null, 0, message);
			}
		}

		public override void WriteLine(string message, string category)
		{
			if (((Filter == null) || Filter.ShouldTrace(null, Name, DefaultTraceEventType, 0, null, null, null, null)))
			{
				Log(DefaultTraceEventType, category, 0, message);
			}
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id)
		{
			if ((Filter == null) || Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, null))
			{
				Log(eventType, source, id, "Event Id {0}", id);
			}
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message)
		{
			if ((Filter == null) || Filter.ShouldTrace(eventCache, source, eventType, id, message, null, null, null))
			{
				Log(eventType, source, id, message);
			}
		}

		public override void TraceEvent(TraceEventCache eventCache, string source, TraceEventType eventType, int id, string message, params object[] args)
		{
			if ((Filter == null) || Filter.ShouldTrace(eventCache, source, eventType, id, message, args, null, null))
			{
				Log(eventType, source, id, message, args);
			}
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, params object[] data)
		{
			if ((Filter == null) || Filter.ShouldTrace(eventCache, source, eventType, id, null, null, null, data))
			{
				string fmt = GetFormat((object[])data);
				Log(eventType, source, id, fmt, data);
			}
		}

		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
			if ((Filter == null) || Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
			{
				string fmt = GetFormat((object)data);
				Log(eventType, source, id, fmt, data);
			}
		}

		#region Private Members

		private void ApplyProperties(NameValueCollection props)
		{
			if (props != null)
			{
				if (props["DefaultTraceEventType"] != null)
				{
					_DefaultTraceEventType = (TraceEventType)Enum.Parse(typeof(TraceEventType), props["DefaultTraceEventType"], true);
				}
				else
				{
					_DefaultTraceEventType = TraceEventType.Verbose;
				}

				if (props["Name"] != null)
				{
					Name = props["Name"];
				}
				else
				{
					Name = "Diagnostics";
				}

				if (props["LoggerNameFormat"] != null)
				{
					LoggerNameFormat = props["LoggerNameFormat"];
				}
				else
				{
					LoggerNameFormat = "{listenerName}.{sourceName}";
				}
			}
		}

		private string GetFormat(params object[] data)
		{
			if (data == null || data.Length == 0)
			{
				return null;
			}

			StringBuilder fmt = new StringBuilder();
			for (int i = 0; i < data.Length; i++)
			{
				fmt.Append('{').Append(i).Append('}');
				if (i < data.Length - 1)
				{
					fmt.Append(',');
				}
			}

			return fmt.ToString();
		}

		private LogLevel MapLogLevel(TraceEventType eventType)
		{
			switch (eventType)
			{
				case TraceEventType.Start:
				case TraceEventType.Stop:
				case TraceEventType.Suspend:
				case TraceEventType.Resume:
				case TraceEventType.Transfer:
					return LogLevel.Trace;
				case TraceEventType.Verbose:
					return LogLevel.Debug;
				case TraceEventType.Information:
					return LogLevel.Info;
				case TraceEventType.Warning:
					return LogLevel.Warn;
				case TraceEventType.Error:
					return LogLevel.Error;
				case TraceEventType.Critical:
					return LogLevel.Fatal;
				default:
					return LogLevel.Trace;
			}
		}

		private void Log(TraceEventType eventType, string source, int id, string format, params object[] args)
		{
			if (!string.IsNullOrEmpty(source))
			{
				source = LoggerNameFormat.Replace("{listenerName}", Name).Replace("{sourceName}", source);
			}

			Logger log = LogManager.GetLogger(source);
			LogLevel logLevel = MapLogLevel(eventType);

			log.Log(logLevel, format, args);
		}

		private static NameValueCollection GetPropertiesFromInitString(string initializeData)
		{
			if (initializeData == null)
			{
				return null;
			}

			NameValueCollection props = new NameValueCollection();
			string[] parts = initializeData.Split(';');

			foreach (string s in parts)
			{
				string part = s.Trim();
				if (part.Length == 0)
				{
					continue;
				}

				int ixEquals = part.IndexOf('=');
				if (ixEquals > -1)
				{
					string name = part.Substring(0, ixEquals).Trim();
					string value = (ixEquals < part.Length - 1) ? part.Substring(ixEquals + 1) : string.Empty;
					props[name] = value.Trim();
				}
				else
				{
					props[part.Trim()] = null;
				}
			}

			return props;
		}

		#endregion
	}
}