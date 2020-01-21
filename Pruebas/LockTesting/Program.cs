using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockTesting
{
    class Locker
    {
        public Object InternalLocker = new Object();
        public Locker() { }
    }

    class Program
    {
        static void Main(string[] args)
        {
            Locker locker = new Locker();
            Task.Factory.StartNew(() =>
            {
                lock (locker)
                {
                    System.Threading.Thread.Sleep(1000);
                    Console.WriteLine("Task 1.....");
                    Console.WriteLine("0.3 = {0}", (int)Math.Ceiling(0.3/*, MidpointRounding.ToEven*/));
                    Console.WriteLine("0.7 = {0}", (int)Math.Ceiling(0.7/*, MidpointRounding.ToEven*/));
                    Console.WriteLine("1.3 = {0}", (int)Math.Ceiling(1.3/*, MidpointRounding.ToEven*/));
                    Console.WriteLine("1.7 = {0}", (int)Math.Ceiling(1.7/*, MidpointRounding.ToEven*/));

                    PrintTimeout(625);
                    PrintTimeout(1400);
                    PrintTimeout(4598);
                }
            });
            Task.Factory.StartNew(() =>
            {
                lock (locker.InternalLocker)
                {
                    System.Threading.Thread.Sleep(100);
                    Console.WriteLine("Task 2.....");
                }
            });

            Console.ReadLine();
        }

        static void PrintTimeout(int timeout)
        {
            int localTimeout = timeout > 1000 ? 1000 : timeout;
            int localReint = (int)Math.Ceiling(((decimal)timeout/localTimeout));

            Console.WriteLine("{0} => {1},{2}", timeout, localTimeout, localReint);

        }
    }
}
