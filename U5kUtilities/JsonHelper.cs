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
        public static dynamic SafeDynamicObjectParse(string s)
        {
            try
            {
                return JsonConvert.DeserializeObject<dynamic>(s);
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
        public static string ToString(object obj, bool format=true)
        {
            return JsonConvert.SerializeObject(obj, format ? Formatting.Indented : Formatting.None);
        }
        public static T Parse<T>(string s)
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(s);
            }
            catch
            {
                return default(T);
            }
        }
        public static string Md5(object s)
        {
            return EncryptionHelper.StringMd5Hash(ToString(s));
        }
        public static T ArrayConvert<T>(object arr)
        {
            try
            {
                if (arr is JArray)
                    return (arr as JArray).ToObject<T>();
            }
            catch(Exception )
            {
            }
            return default(T);
        }
    }
}
