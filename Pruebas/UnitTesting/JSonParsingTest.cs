using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Converters;

namespace UnitTesting
{
    [TestClass]
    public class JSonParsingTest
    {
        public class JSONHelper
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
    
        [TestMethod]
        public void TestMethod1()
        {
            string jdata = System.IO.File.ReadAllText("ps.json");
            
            JObject info = JSONHelper.SafeJObjectParse(jdata);
            JProperty arr = info==null ? null : info.Property("proxies");
            JArray proxies = arr == null ? null : arr.Value as JArray;
            int locales = proxies==null ? 0 :proxies.Where(u => (u.Value<int>("type") == 5 || u.Value<int>("type") == 6)
                    && u.Value<int>("status") == 0).Count();


            string jdata1 = "{}"; // System.IO.File.ReadAllText("tifxinfo.json");

            JArray arr1 = JSONHelper.SafeJArrayParse(jdata1);
            JObject prxsection = arr1 == null ? null :
                arr1.Where(u => u.Value<int>("tp") == 4).FirstOrDefault() as JObject;
            JProperty arr2 = prxsection == null ? null : prxsection.Property("res");
            JArray proxies1 = arr2 == null ? null : arr2.Value as JArray;
            int locales1 = proxies1 == null ? 0 : proxies1.Where(u => (u.Value<int>("tp") == 5 || u.Value<int>("tp") == 6)
                    && u.Value<int>("std") == 0).Count();
        }

        /// <summary>
        /// 
        /// </summary>
        public class DateClass
        {
            public string Id { get; set; }
            public DateTime TimeStamp { get; set; }
        }
        [TestMethod]
        public void JsonParseDateTest()
        {
            DateClass test1 = new DateClass() { Id = "Identificador", TimeStamp = DateTime.Now };
            string str_test1 = JsonConvert.SerializeObject(test1);

            var test2 = JsonConvert.DeserializeObject<DateClass>(str_test1);
        }
    }
}
