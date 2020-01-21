using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace JoinU5kMcast
{
    class Program
    {
        static List<Tuple<IPAddress, IPAddress>> mcastGroup = new List<Tuple<IPAddress, IPAddress>>()
        {
            new Tuple<IPAddress, IPAddress>(IPAddress.Parse("224.100.10.1"), IPAddress.Parse("192.168.0.129")),
        };

        static void Main(string[] args)
        {
            foreach (Tuple<IPAddress, IPAddress> grp in mcastGroup)
            {
                (new UdpClient()).JoinMulticastGroup(grp.Item1, grp.Item2);
            }

            PrintMenu();
            ConsoleKeyInfo result;
            do
            {
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
                        break;
                    case ConsoleKey.D2:
                        break;
                    case ConsoleKey.D3:
                        break;
                    case ConsoleKey.D4:
                        break;
                    case ConsoleKey.D5:
                        break;
                    case ConsoleKey.D6:
                        break;
                    case ConsoleKey.D7:
                        break;
                    case ConsoleKey.D8:
                        break;
                    case ConsoleKey.D9:
                        break;
                    case ConsoleKey.D0:
                        break;
                }
            } while (result.Key != ConsoleKey.Escape);
            
        }
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("Apertura de Grupos Multicast. DFNucleo 2016");
            Console.WriteLine();
            Console.WriteLine("Grupos Abiertos:");
            foreach (Tuple<IPAddress, IPAddress> grp in mcastGroup)
            {
                Console.WriteLine("\t{0} en {1}", grp.Item1.ToString(), grp.Item2.ToString());
            }
            Console.WriteLine();
            Console.WriteLine("ESC. Salir");
            Console.WriteLine();
        }
    }
}
