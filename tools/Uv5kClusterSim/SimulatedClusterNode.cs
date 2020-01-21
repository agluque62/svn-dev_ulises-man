using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Net;
using System.Net.Sockets;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Configuration;
using ClusterLib;

namespace Uv5kClusterSim
{
    public enum SimulateNodeState { NoActivo=NodeState.NoValid, Principal=NodeState.Active, Reserva=NodeState.NoActive };

    class SimulatedClusterNode : IDisposable
    {
        public string NodeName { get; set; }
        public string NodeIpKey { get; set; }
        public string NodePortKey { get; set; }
        public string ListenIp { get; set; }
        public string ListenPort { get; set; }
        public SimulateNodeState NodeStatus { get; set; }

        public void Init()
        {
            string WebConfigPath = "c:\\inetpub\\wwwroot\\NucleoDF\\u5kcfg\\web.config";

            ExeConfigurationFileMap configFile = new ExeConfigurationFileMap() { ExeConfigFilename = WebConfigPath };
            Configuration config = ConfigurationManager.OpenMappedExeConfiguration(configFile, ConfigurationUserLevel.None);
            KeyValueConfigurationCollection settings = config.AppSettings.Settings;

            if (settings.AllKeys.Contains(NodeIpKey) &&
                settings.AllKeys.Contains(NodePortKey))
            {
                ListenIp = settings[NodeIpKey].Value;
                ListenPort = settings[NodePortKey].Value;

                listen.Client.Bind(new IPEndPoint(IPAddress.Parse(ListenIp), Int32.Parse(ListenPort)));
                listen.BeginReceive(new AsyncCallback(OnDataReceive), null);
            }
            else
            {
                ListenIp = "127.0.0.1";
                ListenPort = "0";
            }
        }

        public void Dispose()
        {
        }

        public void ChangeStatus()
        {
            NodeStatus = NodeStatus == SimulateNodeState.NoActivo ? SimulateNodeState.Reserva :
                NodeStatus == SimulateNodeState.Reserva ? SimulateNodeState.Principal : SimulateNodeState.NoActivo;
        }

        UdpClient listen = new UdpClient();
        private void OnDataReceive(IAsyncResult res)
        {
            IPEndPoint RemoteIpEndPoint = null ;
            byte[] received = listen.EndReceive(res, ref RemoteIpEndPoint);
            using (MemoryStream ms = new MemoryStream(received))
            {
                try
                {
                    Object msg = (new BinaryFormatter()).Deserialize(ms) as Object;
                    if (msg is MsgType)
                    {
                        MsgType code = (MsgType)msg;
                        if (code == MsgType.GetState)
                        {
                            cluster.SetState(NodeIpKey, NodeName, NodeStatus);
                            if (NodeStatus != SimulateNodeState.NoActivo)
                            {
                                using (MemoryStream ms1 = new MemoryStream())
                                {
                                    BinaryFormatter bf = new BinaryFormatter();
                                    bf.Serialize(ms1, cluster.clusterLocal(NodeIpKey));

                                    byte[] bin_msg = ms1.ToArray();
                                    listen.SendAsync(bin_msg, bin_msg.Count(), RemoteIpEndPoint);
                                }
                            }
                        }
                    }
                }
                catch (Exception x)
                {
                }
            }
            listen.BeginReceive(new AsyncCallback(OnDataReceive), null);
        }

        static Cluster cluster = new Cluster();
    }

    class Cluster
    {
        ClusterState cluster_state_on_node1 = new ClusterState();
        ClusterState cluster_state_on_node2 = new ClusterState();

        public ClusterState clusterLocal(string nodeKey)
        {
            return nodeKey == "ClusterSrv1Ip" ? cluster_state_on_node1 : cluster_state_on_node2;
        }
        ClusterState clusterRemote(string nodeKey)
        {
            return nodeKey == "ClusterSrv1Ip" ? cluster_state_on_node2 : cluster_state_on_node1;
        }

        public void SetState(string nodeKey, string name, SimulateNodeState status)
        {
            clusterLocal(nodeKey).LocalNode.Name = clusterRemote(nodeKey).RemoteNode.Name = name;
            clusterLocal(nodeKey).LocalNode.SetState((NodeState)status, "");
            clusterRemote(nodeKey).RemoteNode.SetState((NodeState)status, "");
        }

    }
}
