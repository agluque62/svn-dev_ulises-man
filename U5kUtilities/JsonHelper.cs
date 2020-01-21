using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;


namespace Utilities
{
    public class JsonHelper
    {
        public static JObject SafeJObjectParse(string s)
        {
            try
            {
                return JObject.Parse(s);
            }
            catch (Exception)
            {
                return null;
            }
        }
        public static JArray SafeJArrayParse(string s)
        {
            try
            {
                return JArray.Parse(s);
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
