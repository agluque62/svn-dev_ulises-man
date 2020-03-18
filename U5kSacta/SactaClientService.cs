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
        public static bool Start(Action<string> notiyError)
        {
            if (module==null)
            {
                module = new SactaModule();
            }
            module.Start();
            return false;
        }

        public static bool Stop(Action<string> notifyError)
        {
            return false;
        }

        public static string Config
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

        static SactaModule module = null;
    }
}
