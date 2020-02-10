using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Utilities
{
    public class IPHelper
    {
        public static IPAddress  SafeParse(string ip)
        {
            try
            {
                return IPAddress.Parse(ip);
            }
            catch(Exception)
            {
            }
            return null;
        }
        public static IPEndPoint EndPointSafeParse(string text)
        {
            try
            {
                Uri uri;
                if (Uri.TryCreate(text, UriKind.Absolute, out uri))
                    return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
                if (Uri.TryCreate(String.Concat("tcp://", text), UriKind.Absolute, out uri))
                    return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);
                if (Uri.TryCreate(String.Concat("tcp://", String.Concat("[", text, "]")), UriKind.Absolute, out uri))
                    return new IPEndPoint(IPAddress.Parse(uri.Host), uri.Port < 0 ? 0 : uri.Port);

                throw new FormatException("Failed to parse text to IPEndPoint");
            }
            catch (Exception )
            {
            }
            return null;
        }
    }
}
