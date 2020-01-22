using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.Composition;

using NAudio.Wave;
using NAudio.CoreAudioApi;
using NAudio.Wave.SampleProviders;

namespace U5kManServer
{
    public interface IInputFileFormatPlugin
    {
        string Name { get; }
        string Extension { get; }
        WaveStream CreateWaveStream(string fileName);
    }

    [Export(typeof(IInputFileFormatPlugin))]
    class WaveInputFilePlugin : IInputFileFormatPlugin
    {
        public string Name
        { get { return "WAV file"; } }
        public string Extension
        { get { return ".wav"; } }

        public WaveStream CreateWaveStream(string fileName)
        {
            WaveStream readerStream = new WaveFileReader(fileName);
            if (readerStream.WaveFormat.Encoding != WaveFormatEncoding.Pcm
                  && readerStream.WaveFormat.Encoding != WaveFormatEncoding.IeeeFloat)
            {
                readerStream = WaveFormatConversionStream.CreatePcmStream(readerStream);
                readerStream = new BlockAlignReductionStream(readerStream);
            }
            return readerStream;
        }
    }

    class WASAPI
    {
        public static void Concatenate(string outputFile, IEnumerable<string> sourceFiles)
        {
            byte[] buffer = new byte[1024];
            WaveFileWriter waveFileWriter = null;
            try
            {
                foreach (string sourceFile in sourceFiles)
                {
                    using (WaveFileReader reader = new WaveFileReader(sourceFile))
                    {
                        if (waveFileWriter == null)
                            waveFileWriter = new WaveFileWriter(outputFile, reader.WaveFormat);
                        else
                        {
                            if (!reader.WaveFormat.Equals(waveFileWriter.WaveFormat))
                                throw new InvalidOperationException(
                                         "Can't concatenate WAV Files that don't share the same format");
                        }
                        int read;
                        while ((read = reader.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            waveFileWriter.WriteData(buffer, 0, read);
                        }
                    }
                }
            }
            finally
            {
                if (waveFileWriter != null)
                    waveFileWriter.Dispose();
            }
        }

        private IWavePlayer waveOut = new WasapiOut(NAudio.CoreAudioApi.AudioClientShareMode.Shared, 300);
        WaveStream fileWaveStream;
        Action<float> setVolumeDelegate;

        [ImportMany(typeof(IInputFileFormatPlugin))]
        public IEnumerable<IInputFileFormatPlugin> InputFileFormats { get; set; }

        void OnPreVolumeMeter(object sender, NAudio.Wave.SampleProviders.StreamVolumeEventArgs e)
        {
            // we know it is stereo
            //w aveformPainter1.AddMax(e.MaxSampleValues[0]);
            //waveformPainter2.AddMax(e.MaxSampleValues[1]);
        }

        public ISampleProvider CreateInputStream(string fileName)
        {
            var plugin = new WaveInputFilePlugin();
            if (plugin == null)
                throw new InvalidOperationException("Unsupported file extension");
            fileWaveStream = plugin.CreateWaveStream(fileName);
            var waveChannel = new NAudio.Wave.SampleProviders.SampleChannel(fileWaveStream);
            setVolumeDelegate = (vol) => waveChannel.Volume = vol;
            waveChannel.PreVolumeMeter += OnPreVolumeMeter;
            var postVolumeMeter = new MeteringSampleProvider(waveChannel);
            postVolumeMeter.StreamVolume += OnPostVolumeMeter;
            return postVolumeMeter;
        }

        void OnPostVolumeMeter(object sender, StreamVolumeEventArgs e)
        {
            // we know it is stereo
            //volumeMeter1.Amplitude = e.MaxSampleValues[0];
            //volumeMeter2.Amplitude = e.MaxSampleValues[1];
        }

        public WASAPI(string fileName)
        {
            ISampleProvider sampleProvider = null;
            sampleProvider = CreateInputStream(fileName);
            waveOut.Init(new SampleToWaveProvider(sampleProvider));
            waveOut.Play();
            return;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public class Uvki5WavPlayer : NucleoGeneric.BaseCode, IDisposable
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="WavFile"></param>
        public Uvki5WavPlayer(string WavFile)
        {
            try
            {
                _out = new WaveOutEvent();
                _in = new WaveFileReader(WavFile);
                _out.Init(_in);
                _out.PlaybackStopped += (sender, args) =>
                {
                    if (_playing == true)
                    {
                        _in.Position = 0;
                        _out.Play();
                    }
                };
            }
            catch (Exception x)
            {
                _error = true;
                LogException<Uvki5WavPlayer>("", x);
            }
        }

        public void Play()
        {
            if (!_error)
            {
                _playing = true;
                _out.Play();
            }
        }

        public void Stop()
        {
            if (!_error)
            {
                _playing = false;
                _out.Stop();
            }
        }

        public void Dispose()
        {
            if (_out != null)
                _out.Dispose();
            if (_in != null)
                _in.Dispose();
        }
        private WaveOutEvent _out = null;
        private WaveFileReader _in = null;
        private bool _playing = false;
        private bool _error = false;
    }
}
