using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net/*.Sockets*/;
using System.Net.NetworkInformation;
using System.Dynamic;

//using System.Management;

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Pipeline;
using Lextm.SharpSnmpLib.Objects;

namespace U5kManMibRevC
{
    public delegate void AgentDataGet(Action<Object> dataGet);
    
    public interface IU5kMib
    {
        void StoreTo(ObjectStore store);
    }

    public class Ed137RevCMib : IU5kMib, IDisposable
    {
        public Ed137RevCMib(AgentDataGet agentDataGet, AgentDataGet scvDataGet, string oidBase)
        {
            u5KMIBIIMib = new U5kMIBIIMib(agentDataGet);
            u5KSCVMib = new U5kSCVMib(scvDataGet, oidBase);
        }

        public void StoreTo(ObjectStore store)
        {
            Store = store;
            u5KMIBIIMib.StoreTo(store);
            u5KSCVMib.StoreTo(store);
        }

        public void Dispose()
        {
            u5KMIBIIMib.Dispose();
            u5KSCVMib.Dispose();
            
        }

        private readonly U5kMIBIIMib u5KMIBIIMib = null;
        private readonly U5kSCVMib u5KSCVMib = null;
        /** */
        private static ObjectStore Store { get; set; }
        public static ScalarObject SnmpObjectGet(string oid)
        {
            return Store.GetObject(new ObjectIdentifier(oid));
        }

#if DEBUG
#endif
    }

    abstract class U5kiMIb : IU5kMib, IDisposable
    {
        public void Dispose()
        {
            mibGroups?.ForEach(grp =>
            {
                grp.Dispose();
            });
        }
        public void StoreTo(ObjectStore store)
        {
            mibGroups?.ForEach(grp =>
            {
                grp.StoreTo(store);
            });
        }

        protected abstract void LoadGroups();
        protected List<U5kMibGroup> mibGroups;
    }

    abstract class U5kMibGroup : IU5kMib, IDisposable
    {
        public U5kMibGroup(string oidbase= ".1.3.6.1.4.1.7916")
        {
            OidBase = oidbase;
            LoadElements();
        }
        public void StoreTo(ObjectStore store)
        {
            snmpObjects?.ForEach(o =>
            {
                store.Add(o);
            });
        }
        public void Dispose()
        {
            snmpObjects?.ForEach(o =>
            {
                if (o is IDisposable)
                {
                    ((IDisposable)o).Dispose();
                }
            });
        }

        /** */
        protected static string OidBase = ".1.3.6.1.4.1.7916";        // Nucleo
        // protected string OidBase = ".1.3.6.1.4.1.2363.6";      // Eurocontrol
        protected List<SnmpObjectBase> snmpObjects;
        protected abstract void LoadElements();

    }
}
