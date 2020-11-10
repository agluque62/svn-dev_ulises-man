using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;

using NLog;

using U5kBaseDatos;
using U5kManServer;

namespace NucleoGeneric
{
    /// <summary>
    /// Objecto inicial del arbol de objetos.
    /// </summary>
    // [DataContract]
    public class BaseCode
    {

        /** 20160905. AGL. Localizacion del Servicio. */
        public String ServiceSite
        {
            get
            {
                return System.Environment.MachineName;
            }
        }

        private static LogLevel _logLevel;
        private static LogLevel _logLevelLocal;

        public BaseCode()
        {
            _logLevel = LogLevel.FromString("Info");
            _logLevelLocal = LogLevel.FromString("Debug");
        }

        #region Logs

#region Logs - Base
#if _FILTER_V1_
        private static BaseStoreFilter filter = new BaseStoreFilter();
#endif

        /// <summary>
        /// Utiliza esta funcion para escribir en la consola.
        /// </summary>
        public void LogConsole<T>(LogLevel level, String message)
        {
            if (level == LogLevel.Fatal)
                Console.ForegroundColor = ConsoleColor.Red;
            else if (level == LogLevel.Error)
                Console.ForegroundColor = ConsoleColor.DarkRed;
            else if (level == LogLevel.Warn)
                Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(" [" + DateTime.Now + "][" + level + "] [" + typeof(T).Name.ToUpper() + "] " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }

        /// <summary>
        /// Utiliza esta funcion para escribir en el Fichero Log.
        /// </summary>
        static private void LogLogger<T>(LogLevel level, String message)
        {
            Logger _logger = LogManager.GetLogger(typeof(T).Name);
            _logger.Log(level, message);
        }
        static private string From(string caller, int line)
        {
            return String.Format("[{0}:{1}]", caller, line);
        }
        /// <summary>
        /// Utiliza esta función para realizar un log, y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        /// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        /// </summary>
        static private void Log<T>(String key, LogLevel level, String message,
            eIncidencias type, eTiposInci thw, string idhw, Object[] issueMessages,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            try
            {
                // Origen del Mensaje...
                String msgOrg = String.Format("[{2}]:{0}[{1}]", typeof(T).Name, From(caller, lineNumber), U5kManService._Master ? "M" : "S");
                // Clear the string
                message = message.Replace("'", "").Replace("\"", "");

                LogLogger<T>(level, (type != eIncidencias.IGNORE ? "[" + ((Int32)type).ToString() + "] " : "") + msgOrg + ": " + message);

                if (type != eIncidencias.IGNORE)
                {
#if _FILTER_V1_
                    if (HistThread.hproc != null && filter.ToStore(key) == true)
#else
                    if (HistThread.hproc != null)
#endif
                    {
                        HistThread.hproc.AddInci(DateTime.Now, 0, type, (int)thw, idhw, issueMessages);
                    }
                }
            }
            catch (Exception x)
            {
                LogManager.GetLogger("ExceptionInLog").Error(x, "Exception Logging [[" + message + "]]: ");
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="scv"></param>
        /// <param name="inci"></param>
        /// <param name="thw"></param>
        /// <param name="idhw"></param>
        /// <param name="parametros"></param>
        public static void RecordEvent<T>(DateTime when, eIncidencias inci, eTiposInci thw, string idhw, object[] parametros,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            string key = string.Format("{0}_{1}_{2}", (Int32)inci, idhw, StrParams(parametros));
            /** 20181210. Para que el filtro de incidencias repetidas no afecte a los eventos de PTT y SQH */
            LogLevel level = inci == eIncidencias.ITO_PTT ||
                (inci == eIncidencias.IGW_EVENTO &&
                Array.FindIndex(parametros, e => (e as string).ToLower().Contains("ptt") || (e as string).ToLower().Contains("sqh")) >= 0) ? LogLevel.Trace : LogLevel.Debug;

            Log<T>(key, level, String.Format("Historico [{0},{1}] de {2}", inci, thw, idhw), inci, thw, idhw, parametros, lineNumber, caller);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="level"></param>
        /// <param name="message"></param>
        /// <param name="type"></param>
        /// <param name="issueMessages"></param>
        /// <param name="lineNumber"></param>
        /// <param name="caller"></param>
        static protected void Log<T>(LogLevel level, String message,
            eIncidencias type, Object[] issueMessages,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            String msgOrg = String.Format("{0}[{1}]", typeof(T).Name, From(caller, lineNumber));
            String msgInci = String.Format("{0},{1}", (Int32)type, msgOrg);
            if (issueMessages != null)
            {
                foreach (string msg in issueMessages)
                {
                    msgInci += ("," + msg.Replace(",", ";"));
                }
            }
            else
            {
                msgInci = String.Format("{0},{1},{2}", (Int32)type, msgOrg, message);
            }
            string key = string.Format("{0}_{1}_{2}", (Int32)type, "MTTO", msgInci);
            Log<T>(key, level, message, type, eTiposInci.TEH_SISTEMA, "MTTO", new Object[] { msgInci }, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static public void Log<T>(/*String from, */LogLevel level, String message, eIncidencias type,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(level, message, type, null, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static private void Log<T>(/*String from, */LogLevel level, String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(level, message, eIncidencias.IGNORE, lineNumber, caller);
        }

#endregion

#region Trace

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo TRACE, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>        
        static protected void LogTrace<T>(/*String from, */String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Trace, message, eIncidencias.IGNORE, lineNumber, caller);
        }

        ///// <summary>
        ///// Utiliza esta función para realizar un log de tipo TRACE, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        ///// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        ///// </summary>
        //static protected void LogTrace<T>(/*String from, */String message, eIncidencias type,
        //    [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        //{
        //    Log<T>(LogLevel.Trace, message, type, lineNumber, caller);
        //}

        ///// <summary>
        ///// Utiliza esta función para realizar un log de tipo TRACE y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        ///// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        ///// </summary>
        //static protected void LogTrace<T>(/*String from, */String message, eIncidencias type, Object[] issueMessages,
        //    [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        //{
        //    Log<T>(LogLevel.Trace, message, type, issueMessages, lineNumber, caller);
        //}

#endregion

#region Debug

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo DEBUG, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogDebug<T>(/*String from, */String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Debug, message, eIncidencias.IGNORE, lineNumber, caller);
        }

        ///// <summary>
        ///// Utiliza esta función para realizar un log de tipo DEBUG, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        ///// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        ///// </summary>
        //static protected void LogDebug<T>(/*String from, */String message, eIncidencias type,
        //    [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        //{
        //    Log<T>(LogLevel.Debug, message, type, lineNumber, caller);
        //}

        ///// <summary>
        ///// Utiliza esta función para realizar un log de tipo DEBUG y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        ///// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        ///// </summary>
        //static protected void LogDebug<T>(/*String from, */String message, eIncidencias type, Object[] issueMessages,
        //    [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        //{
        //    Log<T>(LogLevel.Debug, message, type, issueMessages, lineNumber, caller);
        //}

#endregion

#region Info

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo INFO, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogInfo<T>(/*String from, */String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Info, message, eIncidencias.IGNORE, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo INFO, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogInfo<T>(/*String from, */String message, eIncidencias type,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Info, message, type, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo INFO, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogInfo<T>(/*String from, */String message, eIncidencias type, Object[] issueMessages,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Info, message, type, issueMessages, lineNumber, caller);
        }

#endregion

#region Warn

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo WARN, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogWarn<T>(/*String from, */String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Warn, message, eIncidencias.IGNORE, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo WARN, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogWarn<T>(/*String from, */String message, eIncidencias type,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Warn, message, type, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo WARN, y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        /// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        /// </summary>
        static protected void LogWarn<T>(/*String from, */String message, eIncidencias type, Object[] issueMessages,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Warn, message, type, issueMessages, lineNumber, caller);
        }

#endregion

#region Error

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo ERROR, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogError<T>(/*String from, */Exception ex, String header = "",
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            if (!String.IsNullOrEmpty(header))
                header = "[" + header + "] ";
            Log<T>(/*from, */LogLevel.Error, header + ex.Message/*, U5kiIncidencias.U5kiIncidencia.U5KI_ERROR_GENERIC*/, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo ERROR, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogError<T>(/*String from, */String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(/*from, */LogLevel.Error, message/*, U5kiIncidencias.U5kiIncidencia.U5KI_ERROR_GENERIC*/, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo ERROR, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogError<T>(/*String from, */String message, eIncidencias type,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(/*from, */LogLevel.Error, message, type, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo ERROR, y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        /// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        /// </summary>
        static protected void LogError<T>(/*String from, */String message, eIncidencias type, Object[] issueMessages,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(LogLevel.Error, message, type, issueMessages, lineNumber, caller);
        }

#endregion

#region Fatal

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo ERROR, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogFatal<T>(/*String from, */Exception ex,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(/*from, */LogLevel.Error, ex.Message/*, U5kiIncidencias.U5kiIncidencia.U5KI_ERROR_GENERIC*/, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo FATAL, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogFatal<T>(/*String from, */String message,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(/*from, */LogLevel.Fatal, message/*, U5kiIncidencias.U5kiIncidencia.U5KI_ERROR_GENERIC*/, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo FATAL, y adicionalmente enviar una incidencia con el string del mensaje literalmente. 
        /// <para>El mensaje NO PUEDE contener comas(',') porque se utilizan como separador.</para>
        /// </summary>
        static protected void LogFatal<T>(/*String from, */String message, eIncidencias type,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(/*from, */LogLevel.Fatal, message, type, lineNumber, caller);
        }

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo FATAL, y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        /// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        /// </summary>
        static protected void LogFatal<T>(/*String from, */String message, eIncidencias type, Object[] issueMessages,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            Log<T>(/*from, */LogLevel.Fatal, message, type, issueMessages, lineNumber, caller);
        }

#endregion

        /// <summary>
        /// Utiliza esta función para realizar un log de tipo ERROR, y adicionalmente enviar una incidencia, con mensajes diferentes entre el del Log y el de la incidencia.
        /// La funcion coge cada uno de los parametros y los separa con comas para que el software en el destino los interprete.
        /// </summary>
        static public void LogException<T>(String message, Exception ex, bool severity = false, bool bRegistroHistorico = false,
            [System.Runtime.CompilerServices.CallerLineNumber] int lineNumber = 0, [System.Runtime.CompilerServices.CallerMemberName] string caller = null)
        {
            message += " [EXCEPTION ERROR]: " + ex.Message;
            if (null != ex.InnerException)
                message += " [INNER EXCEPTION ERROR]" + ex.InnerException.Message;

            Log<T>(severity ? LogLevel.Error : LogLevel.Warn, message, eIncidencias.IGNORE, lineNumber, caller);

            /** */
            if (bRegistroHistorico == true)
            {
                Log<T>(severity ? LogLevel.Error : LogLevel.Warn, "EXCEPTION ERROR", eIncidencias.IGRL_U5KI_SERVICE_ERROR, new object[] { ex.Message }, lineNumber, caller);
            }
        }

        /** */
        protected static Object[] Params(params Object[] objs)
        {
            return objs;
        }
        /** */
        protected static string StrParams(Object[] objs)
        {
            StringBuilder str = new StringBuilder("[");
            if (objs != null)
            {
                foreach (Object obj in objs)
                {
                    str.Append(obj.ToString() + ",");
                }
            }
            str.Append("]");
            return str.ToString();
        }


#if _PARA_BORRAR_
        /// <summary>
        /// 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="from"></param>
        /// <param name="x"></param>
        /// <param name="message"></param>
        public void LogException<T>(String Key, Exception x, String message="")
        {
            Exception last_exception = last_exceptions.ContainsKey(Key) ? last_exceptions[Key] : null;
            if (last_exception==null || last_exception.GetType() != x.GetType())
            {
                Logger _logger = LogManager.GetLogger(typeof(T).Name);

                message = Key + " " + message + " " + x.Message;
                _logger.Error(message);
                _logger.Trace(x, message);

                last_exceptions[Key] = x;
            }
        }
        Dictionary<String, Exception> last_exceptions = new Dictionary<string, Exception>();
#endif
#endregion

#if _PARA_BORRAR_
        static Dictionary<String, Tuple<String, int>> _lastLogCollection = new Dictionary<string, Tuple<string, int>>();
        static bool PrintLog(String quien, String Message)
        {
            if (_lastLogCollection.ContainsKey(quien) == false)
            {
                _lastLogCollection[quien] = new Tuple<string, int>(Message, 0);
                return true;
            }
            Tuple<String, int> lastLog = _lastLogCollection[quien];
            if (lastLog.Item1 != Message)
            {
                _lastLogCollection[quien] = new Tuple<string, int>(Message, 0);
                return true;
            }
            if (lastLog.Item2 > 3)
            {
                _lastLogCollection[quien] = new Tuple<string, int>(Message, 0);
                return true;
            }
            _lastLogCollection[quien] = new Tuple<string, int>(Message, lastLog.Item2 + 1);
            return false;
        }
#endif
        /// <summary>
        /// 
        /// </summary>
        protected class ManagedSemaphore
        {
            public static void Init()
            {
                owners.Clear();
                sems.Clear();
            }

            public static string Create(string id)
            {
                //if (sems.ContainsKey(id))
                //    Throw("ManagedSemaphore Repetido: " + id);
                sems[id] = new System.Threading.Semaphore(1, 1);
                return id;
            }

            static public void WaitOne(string id)
            {
                if (!sems.ContainsKey(id))
                    Throw("ManagedSemaphore no creado: " + id);
                if (sems[id].WaitOne(60000) == false)
                    Throw(String.Format("Semaforo Pillado: {0}", id));

                owners[id] = System.Threading.Thread.CurrentThread.ManagedThreadId;
            }

            static public void Release(string id)
            {
                if (!sems.ContainsKey(id))
                    Throw("ManagedSemaphore no creado: " + id);

                if (sems[id].WaitOne(0) == true)
                    Throw("ManagedSemaphore. Release fuera de lugar: " + id);

                sems[id].Release();
                owners[id] = -1;
            }

            static protected void Throw(string msg)
            {
                throw new Exception(msg);
            }

            static protected Dictionary<string, int> owners = new Dictionary<string, int>();
            static protected Dictionary<string, System.Threading.Semaphore> sems = new Dictionary<string, System.Threading.Semaphore>();
        }
        /// <summary>
        /// 
        /// </summary>
        public class ImAliveTick
        {
            public ImAliveTick(Int16 Interval)
            {
                interval = TimeSpan.FromSeconds(Interval);
                next = DateTime.Now + interval;
            }
            public void Tick(String who, Action cb)
            {
                TimeSpan elapsed = next - DateTime.Now;
                if (elapsed <= TimeSpan.FromSeconds(0))
                {
                    if (cb != null) cb();
                    next = DateTime.Now + interval;
                }
            }
            public void Message(string msg)
            {
#if DEBUG
                LogWarn<NGThread>(msg);
#else
                LogDebug<NGThread>(msg);
#endif
            }
            TimeSpan interval;
            DateTime next;
        }
        public ImAliveTick IamAlive = new ImAliveTick(60);

#if _FILTER_V1_
        /// <summary>
        /// 
        /// </summary>
        protected class BaseStoreFilter
        {
            class StoreFilteData
            {
                public DateTime timestamp { get; set; }
                public Int32 repeats { get; set; }
            }
            public BaseStoreFilter()
            {
                // PttAndSqhFilter = U5kManService.cfgSettings/* U5kManServer.Properties.u5kManServer.Default*/.Historico_PttSqhOnBdt;
                LogRepeatControlTime = U5kManServer.Properties.u5kManServer.Default.LogRepeatFilterSecs;
                LogRepeatSupervisionTime = U5kManServer.Properties.u5kManServer.Default.LogRepeatSupervisionMin;
            }
            public bool ToStore(string keyString)
            {
                lock (locker)
                {
                    /** Control de repetición */
                    bool bStore = true;
                    DateTime now = DateTime.Now;
                    Int32 repeats = 1;
                    Int32 key = keyString.GetHashCode();

                    if (_control.ContainsKey(key))
                    {
                        TimeSpan elapsed = now - _control[key].timestamp;
                        bStore = elapsed > TimeSpan.FromSeconds(LogRepeatControlTime);
                        repeats = bStore ? 1 : _control[key].repeats + 1;
                    }
                    /** Aviso para evento que se está repitiendo */
                    if (repeats == 20)
                    {
                    }

                    _control[key] = new StoreFilteData() { timestamp = now, repeats = repeats };
                    /** Limpiar los eventos que no se repiten... */
                    CleanOld(now);
                    return bStore;
                }
            }
            /// <summary>
            /// 
            /// </summary>
            /// <param name="now"></param>
            void CleanOld(DateTime now)
            {
                TimeSpan elapsed = now - lastClean;
                if (elapsed > TimeSpan.FromMinutes(LogRepeatSupervisionTime))
                {
                    var keysForClean = (from item in _control
                                        where now - item.Value.timestamp > TimeSpan.FromMinutes(5)
                                        select item.Key).ToList();

                    keysForClean.ForEach((key) => _control.Remove(key));
                    lastClean = now;
                }
            }

            int LogRepeatControlTime { get; set; }
            int LogRepeatSupervisionTime { get; set; }
            Dictionary<Int32, StoreFilteData> _control = new Dictionary<Int32, StoreFilteData>();
            DateTime lastClean = DateTime.Now;
            object locker = new object();
        }
#endif

        public static void ConfigCultureSet()
        {
            U5kGenericos.CurrentCultureSet((idioma) =>
            {
                LogTrace<BaseCode>("ConfigCultureSet => " + idioma);
            });
        }
    }
}
