using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Uv5kiTaHfManager.Helpers
{
    public class LocalConfig
    {
        /// <summary>
        /// 
        /// </summary>
        static Dictionary<string, double> _lastConfig = null;

        /// <summary>
        /// 
        /// </summary>
        static void LoadLastConfig()
        {
            _lastConfig = new Dictionary<string, double>();
            if (Properties.Settings.Default.LastConfig == null)
            {
                Properties.Settings.Default.LastConfig = new System.Collections.Specialized.StringCollection();
            }
            foreach (string str in Properties.Settings.Default.LastConfig)
            {
                string[] members = str.Split('#');
                if (members.Length == 2)
                {
                    double result;
                    if (double.TryParse(members[1], out result))
                        _lastConfig[members[0]] = result;
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        static void SaveLastConfig()
        {
            Properties.Settings.Default.LastConfig.Clear();
            foreach (KeyValuePair<string, double> kp in _lastConfig)
            {
                Properties.Settings.Default.LastConfig.Add(String.Format("{0}#{1}", kp.Key, kp.Value));
            }
            Properties.Settings.Default.Save();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strEqu"></param>
        /// <returns></returns>
        public static double GetLastFrecuencyOnEqu(string strEqu)
        {
            if (_lastConfig == null)
                LoadLastConfig();
            if (_lastConfig.ContainsKey(strEqu))
            {
                return _lastConfig[strEqu];
            }
            return 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strEqu"></param>
        /// <param name="frecuency"></param>
        public static void SetLastFrecuencyOnEqu(string strEqu, double frecuency)
        {
            if (_lastConfig == null)
                LoadLastConfig();
            _lastConfig[strEqu] = frecuency;
            SaveLastConfig();
        }


        /// <summary>
        /// Formato ID#FR
        /// </summary>
        /// <param name="strFrec"></param>
        /// <returns></returns>
        public static string IdOfFrecuency(string strFrec)
        {
            string[] members = strFrec.Split('#');
            if (members.Length >= 2)
                return members[0];
            return "FreItem Incorrecto";
        }

        /// <summary>
        /// Formato ID#FR
        /// </summary>
        /// <param name="strFrec"></param>
        /// <returns></returns>
        public static double FrecOfFrecuency(string strFrec)
        {
            string[] members = strFrec.Split('#');
            if (members.Length >= 2)
            {
                double result;
                if (double.TryParse(members[1], out result))
                    return result;
            }
            return 0.0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="srtEqu"></param>
        /// <returns></returns>
        public static string IdOfEquipment(string strEqu)
        {
            string[] members = strEqu.Split('#');
            if (members.Length == 2)
                return members[0];
            return string.Empty;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="strEqu"></param>
        /// <returns></returns>
        public static string OidBaseOfEquipment(string strEqu)
        {
            string[] members = strEqu.Split('#');
            if (members.Length == 2)
                return members[1];
            return string.Empty;
        }
    }
}
