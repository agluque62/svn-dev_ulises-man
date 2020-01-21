using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uv5kClusterSim
{
    class Program
    {
        static SimulatedClusterNode node1 = new SimulatedClusterNode() { NodeName = Properties.Settings.Default.Node1Name, NodeIpKey = "ClusterSrv1Ip", NodePortKey = "ClusterSrv1Port" };
        static SimulatedClusterNode node2 = new SimulatedClusterNode() { NodeName = Properties.Settings.Default.Node2Name, NodeIpKey = "ClusterSrv2Ip", NodePortKey = "ClusterSrv2Port" };

        static void Main(string[] args)
        {
            node1.Init();
            node2.Init();

            ConsoleKeyInfo result;
            do
            {
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
                        node1.ChangeStatus();
                        break;
                    case ConsoleKey.D2:
                        node2.ChangeStatus();
                        break;
                    case ConsoleKey.D3:
                        break;
                    case ConsoleKey.D4:
                        break;
                }
            } while (result.Key != ConsoleKey.Escape);

            node1.Dispose();
            node2.Dispose();
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("Simulador Nodos Cluster para MTTO. DFNucleo 2015-2017. ");
            Console.WriteLine();
            Console.WriteLine("\t1.- Nodo#1: {0,-16} ({1,16}:{2,-5}) => {3}", node1.NodeName, node1.ListenIp, node1.ListenPort, node1.NodeStatus);
            Console.WriteLine("\t2.- Nodo#2: {0,-16} ({1,16}:{2,-5}) => {3}", node2.NodeName, node2.ListenIp, node2.ListenPort, node2.NodeStatus);
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
            Console.WriteLine();
        }
    }
}
