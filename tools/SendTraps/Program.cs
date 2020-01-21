using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;

namespace SendTraps
{
    class Program
    {
        static void Main(string[] args)
        {

            ConsoleKeyInfo result;
            do
            {
                PrintMenu();
                result = Console.ReadKey(true);
                switch (result.Key)
                {
                    case ConsoleKey.D1:
                        EnviaTrap();
                        break;
                }
            } while (result.Key != ConsoleKey.Escape);
        }

        /// <summary>
        /// 
        /// </summary>
        static void PrintMenu()
        {
            Console.Clear();
            Console.WriteLine("SendTraps. DFNucleo 2016...2017.");
            Console.WriteLine();
            Console.WriteLine("\t1.- Envia Trap...");
            Console.WriteLine();
            Console.WriteLine("\tESC. Salir");
        }

        static void EnviaTrap()
        {
            IPAddress address = IPAddress.Parse(Properties.Settings.Default.IpTo);

            Messenger.SendTrapV2(0, VersionCode.V2, new IPEndPoint(address, 162),
                     new OctetString("public"),
                     new ObjectIdentifier(Properties.Settings.Default.EnterpriseOid),
                     0,
                     new List<Variable>()
                     {
                         new Variable(new ObjectIdentifier(".1.3.6.1.4.1.7916.8.2.2.11.0"), new OctetString("------------------------"))    // OID Altavoz...
                     });
        }
    }
}
