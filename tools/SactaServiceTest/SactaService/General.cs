using System;
using System.Text;
using System.Runtime.Remoting.Messaging;

namespace Utilities
{
   public delegate void GenericEventHandler<T>(object sender, T msg);

   public struct Pair<T>
   {
      public T First;
      public T Second;

      public Pair(T first, T second)
      {
         First = first;
         Second = second;
      }
   }

   public struct Pair<T1, T2>
   {
      public T1 First;
      public T2 Second;

      public Pair(T1 first, T2 second)
      {
         First = first;
         Second = second;
      }
   }

   public static class General
   {
      public static void SafeLaunchEvent<T>(GenericEventHandler<T> ev, object sender, T msg)
      {
         if (ev != null)
         {
            ev(sender, msg);
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

      static void AsyncSafeLaunchEventCallback(IAsyncResult ar)
      {
         object delg = ((AsyncResult)ar).AsyncDelegate;
         delg.GetType().GetMethod("EndInvoke").Invoke(delg, new object[] { ar });
      }
   }

   public class BinToLogString
   {
      private byte[] _Data;

      public BinToLogString(byte[] data)
      {
         _Data = data;
      }

      public override string ToString()
      {
         StringBuilder str = new StringBuilder();
         StringBuilder hexStr = new StringBuilder(48);
         StringBuilder asciiStr = new StringBuilder(16);

         for (int i = 0, j = 0, iTotal = (_Data.Length + 15) / 16; i < iTotal; i++)
         {
            for (int jTotal = Math.Min(_Data.Length, (i + 1) * 16); j < jTotal; j++)
            {
               hexStr.AppendFormat("{0:X02} ", _Data[j]);
               asciiStr.Append(_Data[j] > 0x20 && _Data[j] < 0x7F ? (char)_Data[j] : '.');
            }

            str.AppendFormat("{0:X08}  {1,-48} {2}\n", i * 16, hexStr, asciiStr);
            hexStr.Length = 0;
            asciiStr.Length = 0;
         }

         return str.ToString();
      }
   }
}
