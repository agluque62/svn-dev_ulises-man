using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using NLog;

namespace AudioDevicesSimulator.Model
{
    class SimulatedIao : IDisposable
    {
        public event Action<string> NotifyChange;

        public SimulatedIao() 
        {
            try
            {
                fsw = new FileSystemWatcher()
                {
                    Path = PathOfDir,
                    EnableRaisingEvents = true
                };

                fsw.Changed += (sender, ev) =>
                {
                    if (NotifyChange != null)
                        NotifyChange(ev.FullPath);
                };
            }
            catch(Exception x)
            {
                LogManager.GetLogger("AudioDeviceSimulator").Error("Excepcion en Construtor", x);
            }
        }
        
        public void Dispose() 
        { 
        }

        /** */
        public bool Ejecutivo 
        {
            get
            {
                return ReadFromFile(true, 1, 0);
            }
            set
            {
                WriteToFile(true, 1, 0, value ? '1' : '0');
            }
        }
        public bool Ayudante
        {
            get
            {
                return ReadFromFile(true, 2, 0);
            }
            set
            {
                WriteToFile(true, 2, 0, value ? '1' : '0');
            }
        }
        public bool PttEjecutivo
        {
            get
            {
                return ReadFromFile(true, 1, 1);
            }
            set
            {
                WriteToFile(true, 1, 1, value ? '1' : '0');
            }
        }
        public bool PttAyudante
        {
            get
            {
                return ReadFromFile(true, 2, 1);
            }
            set
            {
                WriteToFile(true, 2, 1, value ? '1' : '0');
            }
        }
        public bool AltavozRadio
        {
            get
            {
                return ReadFromFile(true, 3, 0);
            }
            set
            {
                WriteToFile(true, 3, 0, value ? '1' : '0');
            }
        }
        public bool AltavozRadio2
        {
            get
            {
                return ReadFromFile(true, 3, 1);
            }
            set
            {
                WriteToFile(true, 3, 1, value ? '1' : '0');
            }
        }
        public bool AltavozLc
        {
            get
            {
                return ReadFromFile(true, 4, 0);
            }
            set
            {
                WriteToFile(true, 4, 0, value ? '1' : '0');
            }
        }
        public bool CableDeGrabacion
        {
            get
            {
                return ReadFromFile(true, 4, 1);
            }
            set
            {
                WriteToFile(true, 4, 1, value ? '1' : '0');
            }
        }

        /** V1. LED'S */
        public bool LedAltavozRadio
        {
            get
            {
                return ReadFromFile(false, 3, 0);
            }
        }
        public bool LedAltavozRadio2
        {
            get
            {
                return ReadFromFile(false, 3, 1);
            }
        }
        public bool LedAltavozTelefonia
        {
            get
            {
                return ReadFromFile(false, 4, 0);
            }
        }

        /** */
        bool ReadFromFile(bool IsIn, int indexFile, int indexSignal)
        {
            try
            {
                string val = ReadSimulatedCMediaFile(PathOfFile(IsIn, indexFile), indexSignal);
                return val.ElementAt(indexSignal) == '0' ? false : true;
            }
            catch (Exception x)
            {
                LogManager.GetLogger("AudioDeviceSimulator").Error("Excepcion en ReadFromFile", x);
            }
            return false;
        }
        void WriteToFile(bool IsIn, int indexFile, int indexSignal, char valor)
        {
            try
            {
                StringBuilder val = new StringBuilder(ReadSimulatedCMediaFile(PathOfFile(IsIn, indexFile), indexSignal));
                val[indexSignal] = valor;
                File.WriteAllText(PathOfFile(IsIn, indexFile), val.ToString());
            }
            catch (Exception x)
            {
                LogManager.GetLogger("AudioDeviceSimulator").Error("Excepcion en WriteToFile", x);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="indexFile"></param>
        /// <returns></returns>
        string PathOfDir
        {
            get
            {
                return Properties.Settings.Default.PathOfHmi == String.Empty ? 
                    String.Format("{0}\\NucleoCC\\Ulises5000I-TA\\cmedia_sim\\", Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86)) :
                    String.Format("{0}\\cmedia_sim\\", Properties.Settings.Default.PathOfHmi);
            }
        }
        string PathOfFile(bool isIn, int indexFile)
        {
            string name_template = isIn ? "cmedia_#{1}.sim" : "cmedia_#{1}_out.sim";
            return String.Format("{0}" + name_template, PathOfDir, indexFile);
        }
        string ReadSimulatedCMediaFile(string path, int indexSignal)
        {
            string val = File.ReadAllText(path);
            if (indexSignal >= val.Length ||
                (val.ElementAt(indexSignal) != '0' && val.ElementAt(indexSignal) != '1'))
                throw new Exception("El contenido del fichero es erroneo...");
            return val;
        }

        FileSystemWatcher fsw = null;
    }

}
