using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

namespace U5kSacta
{
    class SactaLog
    {
        private static void Log<T>(LogLevel level, String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Logger _logger = LogManager.GetLogger(typeof(T).FullName/*.Name*/);
            String display = String.Format("[{0},{1}]: {2}", memberName, sourceLineNumber, msg);
            _logger.Log(level, display);
        }
        public static void Trace<T>(String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Log<T>(LogLevel.Trace, msg, memberName, sourceLineNumber);
        }
        public static void Info<T>(String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Log<T>(LogLevel.Info, msg, memberName, sourceLineNumber);
        }
        public static void Debug<T>(String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Log<T>(LogLevel.Debug, msg, memberName, sourceLineNumber);
        }
        public static void Error<T>(String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Log<T>(LogLevel.Error, msg, memberName, sourceLineNumber);
        }
        public static void Warning<T>(String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Log<T>(LogLevel.Warn, msg, memberName, sourceLineNumber);
        }
        public static void Fatal<T>(String msg,
            [System.Runtime.CompilerServices.CallerMemberName] string memberName = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int sourceLineNumber = 0)
        {
            Log<T>(LogLevel.Fatal, msg, memberName, sourceLineNumber);
        }

    }
}
