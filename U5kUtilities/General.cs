using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Remoting.Messaging;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Utilities
{
	public delegate void GenericEventHandler(object sender);
	public delegate void GenericEventHandler<T>(object sender, T par);
	public delegate void GenericEventHandler<T1, T2>(object sender, T1 par1, T2 par2);
	public delegate void GenericEventHandler<T1, T2, T3>(object sender, T1 par1, T2 par2, T3 par3);
	public delegate void GenericEventHandler<T1, T2, T3, T4>(object sender, T1 par1, T2 par2, T3 par3, T4 par4);
	public delegate void GenericEventHandler<T1, T2, T3, T4, T5>(object sender, T1 par1, T2 par2, T3 par3, T4 par4, T5 par5);

	public struct Pair
	{
		public readonly object First;
		public readonly object Second;

		public Pair(object first, object second)
		{
			First = first;
			Second = second;
		}

		public override string ToString()
		{
			return string.Format("{0} {1}", First, Second);
		}
	}

	public struct Pair<T>
   {
      public readonly T First;
      public readonly T Second;

      public Pair(T first, T second)
      {
         First = first;
         Second = second;
      }

		public override string ToString()
		{
			return string.Format("{0} {1}", First, Second);
		}
   }

   public struct Pair<T1, T2>
   {
      public readonly T1 First;
      public readonly T2 Second;

      public Pair(T1 first, T2 second)
      {
         First = first;
         Second = second;
      }

		public override string ToString()
		{
			return string.Format("{0} {1}", First, Second);
		}
   }

	public struct Tuple<T1, T2, T3>
	{
      public readonly T1 First;
      public readonly T2 Second;
		public readonly T3 Third;

      public Tuple(T1 first, T2 second, T3 third)
      {
         First = first;
         Second = second;
			Third = third;
      }

		public override string ToString()
		{
			return string.Format("{0} {1} {2}", First, Second, Third);
		}
	}

	//public class EventArg<T> : EventArgs
	//{
	//   public T Info;

	//   public EventArg(T info)
	//   {
	//      Info = info;
	//   }
	//}

   public static class General
   {
        //static Logger _Logger = LogManager.GetCurrentClassLogger();

		public static void SafeLaunchEvent(GenericEventHandler ev, object sender)
		{
			if (ev != null)
			{
				ev(sender);
			}
		}

		public static void SafeLaunchEvent<T>(GenericEventHandler<T> ev, object sender, T par)
      {
         if (ev != null)
         {
            ev(sender, par);
         }
      }

		public static void SafeLaunchEvent<T1, T2>(GenericEventHandler<T1, T2> ev, object sender, T1 par1, T2 par2)
		{
			if (ev != null)
			{
				ev(sender, par1, par2);
			}
		}

		public static void SafeLaunchEvent<T1, T2, T3>(GenericEventHandler<T1, T2, T3> ev, object sender, T1 par1, T2 par2, T3 par3)
		{
			if (ev != null)
			{
				ev(sender, par1, par2, par3);
			}
		}

		public static void SafeLaunchEvent<T1, T2, T3, T4>(GenericEventHandler<T1, T2, T3, T4> ev, object sender, T1 par1, T2 par2, T3 par3, T4 par4)
		{
			if (ev != null)
			{
				ev(sender, par1, par2, par3, par4);
			}
		}

		public static void SafeLaunchEvent<T1, T2, T3, T4, T5>(GenericEventHandler<T1, T2, T3, T4, T5> ev, object sender, T1 par1, T2 par2, T3 par3, T4 par4, T5 par5)
		{
			if (ev != null)
			{
				ev(sender, par1, par2, par3, par4, par5);
			}
		}

		public static void SafeLaunchEvent(EventHandler ev, object sender)
		{
			if (ev != null)
			{
				ev(sender, EventArgs.Empty);
			}
		}

		public static void SafeLaunchEvent<T>(EventHandler<T> ev, object sender, T e)
			where T : EventArgs
		{
			if (ev != null)
			{
				ev(sender, e);
			}
		}

      public static void AsyncSafeLaunchEvent<T>(GenericEventHandler<T> ev, object sender, T msg)
      {
         if (ev != null)
         {
            GenericEventHandler<T> auxEv = delegate(object s, T m)
            {
               ev(s, m);
            };
            auxEv.BeginInvoke(sender, msg, AsyncSafeLaunchEventCallback, null);
         }
      }

      public static ushort Crc16(byte[] data)
      {
         const ushort POLINOMIO = 0x9021;
         int crc = 0;

         for (int i = 0, num = data.Length; i < num; i++)
         {
            byte c = data[i];

            for (int j = 0; j < 8; j++)
            {
               int cn = c ^ ((crc >> 8) & 0xff);

               crc <<= 1;
               if ((cn & 0x80) > 0)
               {
                  crc ^= POLINOMIO;
               }

               c <<= 1;
            }
         }

         crc = (crc << 8) | (crc >> 8);
         crc &= 0x7f7f;

         return (ushort)crc;
      }

		public static bool TimeElapsed(int? last, int interval)
		{
			if (last.HasValue)
			{
				if (Environment.TickCount - last.Value <= interval)
				{
					return false;
				}
			}

			return true;
		}

		public static bool TimeElapsed(ref int? last, int interval)
		{
			if (last.HasValue)
			{
				if (Environment.TickCount - last.Value <= interval)
				{
					return false;
				}

				last = null;
			}

			return true;
		}

		public static List<string> GetOperationalV4Ips()
		{
			List<string> ips = new List<string>();
			NetworkInterface[] nets = NetworkInterface.GetAllNetworkInterfaces();

			foreach (NetworkInterface iface in nets)
			{
				if ((iface.NetworkInterfaceType != NetworkInterfaceType.Loopback) &&
					(iface.OperationalStatus == OperationalStatus.Up))
				{
					foreach (UnicastIPAddressInformation ip in iface.GetIPProperties().UnicastAddresses)
					{
						if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
						{
							ips.Add(ip.Address.ToString());
						}
					}
				}
			}

			ips.Sort();
			return ips;
		}

      static void AsyncSafeLaunchEventCallback(IAsyncResult ar)
      {
         object delg = ((AsyncResult)ar).AsyncDelegate;
         delg.GetType().GetMethod("EndInvoke").Invoke(delg, new object[] { ar });
      }
   }

   public class BinToLogString
   {
      private byte[] _Data;
		private int _Length;

      public BinToLogString(byte[] data)
      {
         _Data = data;
			_Length = data.Length;
      }

		public BinToLogString(byte[] data, int length)
		{
			_Data = data;
			_Length = length;
		}

      public override string ToString()
      {
         StringBuilder str = new StringBuilder();

			if ((_Data != null) && (_Length > 0))
			{
				StringBuilder hexStr = new StringBuilder(48);
				StringBuilder asciiStr = new StringBuilder(16);

				for (int i = 0, j = 0, iTotal = (_Length + 15) / 16; i < iTotal; i++)
				{
					for (int jTotal = Math.Min(_Length, (i + 1) * 16); j < jTotal; j++)
					{
						hexStr.AppendFormat("{0:X02} ", _Data[j]);
						asciiStr.Append(_Data[j] > 0x20 && _Data[j] < 0x7F ? (char)_Data[j] : '.');
					}

					str.AppendFormat("{0:X08}  {1,-48} {2}{3}", i * 16, hexStr, asciiStr, Environment.NewLine);
					hexStr.Length = 0;
					asciiStr.Length = 0;
				}

				int newLineLength = Environment.NewLine.Length;
				str.Remove(str.Length - newLineLength, newLineLength);
			}

         return str.ToString();
      }
   }
}
