using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Configuration;
using System.Runtime.Serialization.Formatters.Binary;

namespace U5kManServer
{
    [Serializable()]
    class U5kManUserProperty
    {
        public string id { get; set; }
        public int grp { get; set; }
        public int tp { get; set; }
        public List<Tuple<string,string>> opt { get; set; }
        public string key { get; set; }
        public object val { get; set; }
    }
    
    /// <summary>
    /// 
    /// </summary>
    class U5kManUserProperties
    {
        protected List<U5kManUserProperty> props = new List<U5kManUserProperty>()
        {
            new U5kManUserProperty(){id=Properties.strings.Idioma, grp=-1, tp=1, 
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>("Español","es"),
                    new Tuple<string,string>("Ingles","en"),
                    new Tuple<string,string>("Frances","fr")
                }, key="General_Idioma",val=null},
            new U5kManUserProperty(){id=Properties.strings.ServidorDual, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="General_ServidorDual", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_008 /*"Patron de Reloj"*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="General_HayReloj", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_009 /*"PABX Interna"*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="General_HayPabx", val=null},
            new U5kManUserProperty(){id=Properties.strings.SACTA, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="General_HaySacta", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_015/*Altavoz HF*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="General_HayAltavozHF", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_010 /*"Sonido"*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="General_SonidoAlarmas", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_013 /*"Incidencias sin Reconocer"*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>("8","8"), new Tuple<string,string>("16","16"),
                    new Tuple<string,string>("32","32"), new Tuple<string,string>("64","64")
                }, key="General_LineasIncidencias", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_011 /*"Generar Historico"*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="Historico_GenerarHistoricos", val=null},
            new U5kManUserProperty(){id=Properties.strings.EVENTOSPTT, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.SI,"False"),
                    new Tuple<string,string>(Properties.strings.NO, "True")
                }, key="Historico_PttAndSqhOnBdt", val=null},
            new U5kManUserProperty(){id=Properties.strings.WAP_MSG_012 /*"Dias en Historico"*/, grp=-1, tp=1,
                opt=new List<Tuple<string,string>>()
                {
                    new Tuple<string,string>(Properties.strings.WAP_OPT_001 /*"1 Semana" */,"7"),
                    new Tuple<string,string>(Properties.strings.WAP_OPT_002 /*"2 Semanas"*/,"14"),
                    new Tuple<string,string>(Properties.strings.WAP_OPT_003 /*"1 Mes"    */,"30"),
                    new Tuple<string,string>(Properties.strings.WAP_OPT_004 /*"3 Meses"  */,"91"),
                    new Tuple<string,string>(Properties.strings.WAP_OPT_005 /*"6 Meses"  */,"182"),
                    new Tuple<string,string>(Properties.strings.WAP_OPT_006 /*"1 Año"    */,"365")
                }, key="Historico_DiasEnHistorico", val=null},
            new U5kManUserProperty(){id="",grp=-1,tp=0,opt=null,key="",val=null}
        };
        public U5kManUserProperties()
        {
            Load();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str_group"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public object PropertyGet(string key)
        {
            U5kManUserProperty prop = PropertyFind(key);
            if (prop != null)
            {
                return prop.val;
            }
            return null;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="str_group"></param>
        /// <param name="key"></param>
        /// <param name="val"></param>
        public void PropertySet(string key, object val)
        {
            U5kManUserProperty prop = PropertyFind(key);
            if (prop != null)
            {
                prop.val = val.ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Load()
        {
            props.Clear();
            foreach (U5kManUserProperty prop in props)
            {
                prop.val = U5kManServer.Properties.u5kManServer.Default[prop.key].ToString();
            }
        }
        /// <summary>
        /// 
        /// </summary>
        public void Save()
        {
            foreach (U5kManUserProperty prop in props)
            {
                U5kManServer.Properties.u5kManServer.Default[prop.key] = prop.val.ToString();
            }
            U5kManServer.Properties.u5kManServer.Default.Save();
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="grp_key"></param>
        /// <param name="prp_key"></param>
        /// <returns></returns>
        protected U5kManUserProperty PropertyFind(string prp_key)
        {
            var props_lst = props.Where(p => p.key == prp_key).ToList();
            if (props_lst.Count() > 0)
            {
                return props_lst[0];
            }
            return null;
        }

    }
}
