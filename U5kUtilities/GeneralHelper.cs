using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities
{
    public class GeneralHelper
    {
        public static T SafeStringToEnum<T>(string input)
        {
            if (Enum.IsDefined(typeof(T), input))
            {
                var val = Enum.Parse(typeof(T), input);
                return (T)val;
            }
            return (T)(object)0;
        }
        public static T SafeIntToEnum<T>(int input)
        {
            if (Enum.IsDefined(typeof(T), input))
            {
                var val = Enum.Parse(typeof(T), input.ToString());
                return (T)val;
            }
            return (T)(object)0;
        }
    }
}
