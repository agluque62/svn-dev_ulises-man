using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;

namespace U5kSacta
{
    public class SactaClientService
    {
        public SactaClientService(Func<object> dbService)
        {
            SactaBdt.Init(dbService);
        }
        public  bool Start(Action<string> notiyError)
        {
            if (module==null)
            {
                module = new SactaModule();
            }
            SactaBdt.Start();
            module.Start();
            return true;
        }

        public  bool Stop(Action<string> notifyError)
        {
            SactaBdt.Stop();
            module?.Stop();
            module = null;
            return false;
        }

        public  string Config
        {
            get
            {
                string cfgstr = "{}";
                SactaConfig.GetConfig((cfg, error) =>
                {
                    try 
                    {
                        cfgstr = JsonConvert.SerializeObject(cfg);
                    }
                    catch(Exception)
                    {
                        // Todo. Notificacion o log de la excepcion.
                    }
                });
                return cfgstr;
            }
            set
            {
                try
                {
                    SactaConfig.SetConfig(JsonConvert.DeserializeObject<SactaConfig>(value), (error) =>
                    {
                        if (error != null)
                        {
                            // Todo. Notificacion o log de la excepcion.
                        }
                    });
                }
                catch(Exception )
                {
                    // Todo. Notificación o log de la excepción.
                }
            }

        }
        SactaModule module = null;

        
    }
}
