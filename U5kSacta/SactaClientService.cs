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
        public SactaClientService()
        {

        }
        public bool Start(Action<string> notiyError)
        {
            return false;
        }

        public bool Stop(Action<string> notifyError)
        {
            return false;
        }

        public string Config
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

    }
}
