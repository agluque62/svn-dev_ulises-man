using System;
using System.IO;
using System.Timers;
using System.Collections.Generic;
using System.Text;
using System.Net;

using NLog;
using SimulSACTA.Properties;
using Utilities;

namespace SimulSACTA
{
    class Sacta : IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        public string[] SectorUcs
        {
            get { return _SectorUcs; }
            set { _SectorUcs = value; }
        }
        /// <summary>
        /// 
        /// </summary>
        public Sacta()
        {
            Settings stts = Settings.Default;

            _Comm = new UdpSocket[2];

            _Comm[0] = new UdpSocket(stts.SactaIpA, stts.ListenPortA);
            _Comm[1] = new UdpSocket(stts.SactaIpB, stts.ListenPortB);

            if (stts.EnableMulticast == true)
            {
                _Comm[0].Base.JoinMulticastGroup(IPAddress.Parse(stts.SactaMcastA), IPAddress.Parse(stts.SactaIpA));
                _Comm[1].Base.JoinMulticastGroup(IPAddress.Parse(stts.SactaMcastB), IPAddress.Parse(stts.SactaIpA));
            }

            _Comm[0].NewDataEvent += OnNewData;
            _Comm[1].NewDataEvent += OnNewData;

            _EndPoint = new IPEndPoint[2];
            _EndPoint[0] = new IPEndPoint(IPAddress.Parse(stts.ScvIpA), stts.ScvPortA);
            _EndPoint[1] = new IPEndPoint(IPAddress.Parse(stts.ScvIpB), stts.ScvPortB);

            _LastScvReceived = new DateTime[2];

            _PeriodicTasks = new Timer(_PeriodicTasksInterval);
            _PeriodicTasks.AutoReset = false;
            _PeriodicTasks.Elapsed += PeriodicTasks;
        }
        /// <summary>
        /// 
        /// </summary>
        ~Sacta()
        {
            Dispose(false);
        }
        /// <summary>
        /// 
        /// </summary>
        public void Run()
        {
            _Comm[0].BeginReceive();
            _Comm[1].BeginReceive();
            _PeriodicTasks.Enabled = true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectorUcs"></param>
        public void RunSectorization(string[] sectorUcs)
        {
            SendSectorization(sectorUcs, null);
        }

        #region IDisposable Members
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion

        #region Private
        /// <summary>
        /// 
        /// </summary>
        const uint _PeriodicTasksInterval = 1000;
        /// <summary>
        /// 
        /// </summary>
        UdpSocket[] _Comm;
        IPEndPoint[] _EndPoint;
        DateTime[] _LastScvReceived;
        DateTime _LastPresenceSended;
        Timer _PeriodicTasks;
        int _ActivityState;
        uint _ActivityTimeOut = Settings.Default.ActivityTimeOut;
        string[] _SectorUcs;
        ushort _SeqNum;
        uint _SectVersion;
        bool _SendInit;
        bool _Disposed;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="bDispose"></param>
        void Dispose(bool bDispose)
        {
            if (!_Disposed)
            {
                _Disposed = true;

                if (bDispose)
                {
                    _PeriodicTasks.Enabled = false;
                    _PeriodicTasks.Close();
                    _PeriodicTasks = null;

                    _Comm[0].Dispose();
                    _Comm[1].Dispose();
                    _Comm = null;

                    _EndPoint = null;
                    _LastScvReceived = null;
                    _SectorUcs = null;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="dg"></param>
        void OnNewData(object sender, DataGram dg)
        {
            try
            {
                MemoryStream ms = new MemoryStream(dg.Data);
                CustomBinaryFormatter bf = new CustomBinaryFormatter();
                SactaMsg msg = bf.Deserialize<SactaMsg>(ms);

                if (IsValid(msg))
                {
                    int net = (sender == _Comm[0] ? 0 : 1);
                    _LastScvReceived[net] = DateTime.Now;

                    switch (msg.Type)
                    {
                        case SactaMsg.MsgType.Presence:
                            _ActivityTimeOut = (uint)(((SactaMsg.PresenceInfo)(msg.Info)).ActivityTimeOutSg * 1000);
                            if (_ActivityTimeOut < 5) _ActivityTimeOut = Settings.Default.ActivityTimeOut;
                            break;
                        case SactaMsg.MsgType.SectAsk:
                            MainForm.LogMethod("INFO", "Recibida Peticion de Sectorizacion");
                            SendSectorization(_SectorUcs, msg);
                            break;
                        case SactaMsg.MsgType.SectAnwer:
                            SactaMsg.SectAnswerInfo info = (SactaMsg.SectAnswerInfo)(msg.Info);
                            MainForm.LogMethod("INFO", String.Format("Sectorizacion V-{0}: {1}", info.Version, (info.Result == 1 ? "Implantada" : "Rechazada")));
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                if (!_Disposed)
                {
                    MainForm.LogMethod("ERROR", String.Format("Excepción al Procesar Datos Recibidos: {0}",ex.Message));
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        bool IsValid(SactaMsg msg)
        {
            Settings stts = Settings.Default;

            return ((msg.DomainOrg == stts.ScvDomain) && (msg.DomainDst == stts.SactaDomain) &&
               (msg.CenterOrg == stts.ScvCenter) && (msg.CenterDst == stts.SactaCenter) &&
               (msg.UserDst == stts.SactaGroupUser) && stts.ScvUsers.Contains(msg.UserOrg.ToString()));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PeriodicTasks(object sender, ElapsedEventArgs e)
        {
            try
            {
                int activityState = ((uint)((DateTime.Now - _LastScvReceived[0]).TotalMilliseconds) < _ActivityTimeOut ? 1 : 0);
                activityState |= ((uint)((DateTime.Now - _LastScvReceived[1]).TotalMilliseconds) < _ActivityTimeOut ? 2 : 0);

                if (activityState != _ActivityState)
                {
                    _ActivityState = activityState;
                    MainForm.LogMethod("INFO", String.Format("Estado de Actividad de SCV Cambiado a {0}", _ActivityState));
                }

                if ((uint)((DateTime.Now - _LastPresenceSended).TotalMilliseconds) > Settings.Default.PresenceInterval)
                {
                    if (!_SendInit)
                    {
                        SendInit();
                        _SendInit = true;
                    }
                    SendPresence();
                }
            }
            catch (Exception ex)
            {
                if (!_Disposed)
                {
                    MainForm.LogMethod("ERROR", String.Format("Excepción en Tareas Periodicas de SACTA: {0}", ex.Message));
                }
            }
            finally
            {
                if (!_Disposed)
                {
                    _PeriodicTasks.Enabled = true;
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void SendInit()
        {
            CustomBinaryFormatter bf = new CustomBinaryFormatter();
            MemoryStream ms = new MemoryStream();
            SactaMsg msg = new SactaMsg(SactaMsg.MsgType.Init, SactaMsg.InitId);

            bf.Serialize(ms, msg);
            byte[] data = ms.ToArray();

            foreach (string sUser in Settings.Default.ScvUsers)
            {
                MainForm.LogMethod("INFO", String.Format("Enviado MSG Init a SCV {0}", sUser));

                byte[] user = BitConverter.GetBytes(UInt16.Parse(sUser));
                Array.Reverse(user);
                Array.Copy(user, 0, data, 6, 2);

                _Comm[0].Send(_EndPoint[0], data);
                _Comm[1].Send(_EndPoint[1], data);
            }

            _SeqNum = 0;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectorUcs"></param>
        /// <param name="ask"></param>
        void SendSectorization(string[] sectorUcs, SactaMsg ask)
        {
            CustomBinaryFormatter bf = new CustomBinaryFormatter();

            MemoryStream ms = new MemoryStream();
            SactaMsg msg = new SactaMsg(SactaMsg.MsgType.Sectorization, _SeqNum++, _SectVersion++, sectorUcs);

            bf.Serialize(ms, msg);
            byte[] data = ms.ToArray();

            if (ask != null)
            {
                MainForm.LogMethod("INFO", String.Format("Enviando Sectorizacion a SCV {0} ({1})", ask.UserOrg, strSect(sectorUcs)));

                byte[] user = BitConverter.GetBytes(ask.UserOrg);
                Array.Reverse(user);
                Array.Copy(user, 0, data, 6, 2);

                _Comm[0].Send(_EndPoint[0], data);
                _Comm[1].Send(_EndPoint[1], data);
            }
            else
            {
                foreach (string sUser in Settings.Default.ScvUsers)
                {
                    MainForm.LogMethod("INFO", String.Format("Enviando Sectorizacion V-{0} a SCV {1} ({2})", ((SactaMsg.SectInfo)(msg.Info)).Version, sUser, strSect(sectorUcs)));

                    byte[] user = BitConverter.GetBytes(UInt16.Parse(sUser));
                    Array.Reverse(user);
                    Array.Copy(user, 0, data, 6, 2);

                    _Comm[0].Send(_EndPoint[0], data);
                    _Comm[1].Send(_EndPoint[1], data);
                }
            }
        }
        /// <summary>
        /// 
        /// </summary>
        void SendPresence()
        {
            CustomBinaryFormatter bf = new CustomBinaryFormatter();

            MemoryStream ms = new MemoryStream();
            SactaMsg msg = new SactaMsg(SactaMsg.MsgType.Presence, _SeqNum++);

            bf.Serialize(ms, msg);
            byte[] data = ms.ToArray();

            foreach (string sUser in Settings.Default.ScvUsers)
            {
                byte[] user = BitConverter.GetBytes(UInt16.Parse(sUser));
                Array.Reverse(user);
                Array.Copy(user, 0, data, 6, 2);

                _Comm[0].Send(_EndPoint[0], data);
                _Comm[1].Send(_EndPoint[1], data);
            }

            _LastPresenceSended = DateTime.Now;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sectorUcs"></param>
        /// <returns></returns>
        string strSect(string[] sectorUcs) 
        {
            StringBuilder sb = new StringBuilder();
            for (int i=0; i<sectorUcs.Length; i+=2) 
            {
                sb.Append(sectorUcs[i].ToString());
                sb.Append("->");
                sb.Append(sectorUcs[i+1].ToString());
                sb.Append(",");
            }
            return sb.ToString();
        }

        #endregion

        /** 20170214. AGL. Para permitir las 'escuchas mcast' */
        class UdpSocket2
        {
        }

    }
}
