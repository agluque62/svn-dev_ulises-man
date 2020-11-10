using System;
using System.Net;
using System.Net.Sockets;
using System.Collections.Generic;

namespace Utilities
{
    public struct DataGram
    {
        public byte[] Data;
        public IPEndPoint Client;
    }

    public class UdpSocket : IDisposable
    {
        #region Public

        public event GenericEventHandler<DataGram> NewDataEvent;

        public UdpClient Base
        {
            get { return _Udp; }
        }

        public int MaxReceiveThreads
        {
            get { return _MaxReceiveThreads; }
            set { _MaxReceiveThreads = value; }
        }

        /** */
        public UdpSocket()
        {
            _Udp = new UdpClient();
            _Datagrams = new Queue<DataGram>();
        }

        public UdpSocket(int port, bool share=false)
            : this(null, port, share)
        {
        }

        public UdpSocket(string ip, int port, bool share=false)
        {
            // _Logger.Debug("Creating new UdpSocket en {0}:{1}", ip, port);
            if (share == false)
                _Udp = new UdpClient(new IPEndPoint(ip != null ? IPAddress.Parse(ip) : IPAddress.Any, port));
            else
            {
                _Udp = new UdpClient();
                _Udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                _Udp.Client.Bind(new IPEndPoint(ip != null ? IPAddress.Parse(ip) : IPAddress.Any, port));
            }

            if (Environment.OSVersion.Platform != PlatformID.Unix)
            {
                uint SIO_UDP_CONNRESET = 0x9800000C;
                byte[] inValue = new byte[] { 0, 0, 0, 0 }; // == false
                byte[] outValue = new byte[] { 0, 0, 0, 0 }; // initialize to 0

                _Udp.Client.IOControl((int)SIO_UDP_CONNRESET, inValue, outValue);
            }

            _Datagrams = new Queue<DataGram>();
        }

        ~UdpSocket()
        {
            Dispose(false);
        }

        #region IDisposable Members

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion

        public void BeginReceive()
        {
            ClearReceiveBuffer();
            _Udp.BeginReceive(ReceiveCallback, null);
        }

        public void ClearReceiveBuffer()
        {
            IPEndPoint ep = new IPEndPoint(IPAddress.Any, 0);
            while (_Udp.Available > 0)
            {
                _Udp.Receive(ref ep);
            }
        }

        public void Send(IPEndPoint remoteEP, byte[] msg)
        {
            // _Logger.Trace("Sending data to {0}:{1}{2}", remoteEP, Environment.NewLine, new BinToLogString(msg));
            _Udp.Send(msg, msg.Length, remoteEP);
        }

        #endregion

        #region Private
        // static Logger _Logger = LogManager.GetCurrentClassLogger();

        UdpClient _Udp;
        Queue<DataGram> _Datagrams;
        int _MaxReceiveThreads = 10;
        int _NumRecevieThreads;
        bool _Disposed;

        void Dispose(bool bDispose)
        {
            if (!_Disposed)
            {
                _Disposed = true;

                if (bDispose)
                {
                    _Udp.Close();
                    _Udp = null;

                    lock (_Datagrams)
                    {
                        _Datagrams.Clear();
                        _Datagrams = null;
                    }
                }
            }
        }

        void ReceiveCallback(IAsyncResult ar)
        {
            /** Al destruir un objeto se llama a esta funcion con _Udp=null */
            if (_Udp == null)
                return;
            try
            {
                DataGram dg = new DataGram();
                IPEndPoint client = new IPEndPoint(IPAddress.Any, 0);
                bool processData = false;

                dg.Data = _Udp.EndReceive(ar, ref client);
                dg.Client = client;

                /** 20170119. INCI-ASPAS. Este paquete hay que lanzarlo si o si*/
                General.SafeLaunchEvent(NewDataEvent, this, dg);
                /*******************************************************/

                // _Logger.Trace("Received data from {0}:{1}{2}", client, Environment.NewLine, new BinToLogString(dg.Data));

                lock (_Datagrams)
                {
                    while (_Udp.Available > 0)
                    {
                        DataGram dgToEnqueue = new DataGram();
                        client = new IPEndPoint(IPAddress.Any, 0);

                        dgToEnqueue.Data = _Udp.Receive(ref client);
                        dgToEnqueue.Client = client;

                        _Datagrams.Enqueue(dgToEnqueue);
                    }

                    if (_NumRecevieThreads < _MaxReceiveThreads)
                    {
                        processData = true;
                        _NumRecevieThreads++;
                    }
                }

                _Udp.BeginReceive(ReceiveCallback, null);

                /** AGL.2017. */
                //lock (_Datagrams)
                //{
                //    if (/*_Datagrams.Count > 0 && */processData == false)
                //    {
                //        LogManager.GetCurrentClassLogger().Error("UdpSocket: ERROR. Datagrama Perdido. _DCount={2}, _TNum={0}, _TMax={1}", 
                //            _NumRecevieThreads, _MaxReceiveThreads, _Datagrams.Count);
                //    }
                //}

                while (processData)
                {
                    /** 20170119. INCI-ASPAS. */
                    // General.SafeLaunchEvent(NewDataEvent, this, dg);

                    lock (_Datagrams)
                    {
                        if ((_Datagrams.Count > 0) && (_NumRecevieThreads <= _MaxReceiveThreads))
                        {
                            dg = _Datagrams.Dequeue();
                            /** 20170119. INCI-ASPAS. Una vez se desencola, hay que lanzarlo */
                            General.SafeLaunchEvent(NewDataEvent, this, dg);
                            /*******************************************************/
                        }
                        else
                        {
                            _NumRecevieThreads--;
                            processData = false;
                        }
                    }
                }
            }
            catch (Exception )
            {
                if (!_Disposed)
                {
                    // _Logger.FatalException("ERROR receiving data", ex);
                    // LogManager.GetCurrentClassLogger().Error("UdpSocket: ERROR receiving data: {0}", ex.Message);
                }
            }

        }

        #endregion
    }
}
