using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace yoyo_bot.src.bot.music
{
    /// <summary>
    /// Used to know if I need to open the stdin or stdout process stream
    /// </summary>
    enum ProcessStartMode
    {
        INPUT, OUTPUT
    }

    /// <summary>
    /// Exposes methods to handle FFMPEG process
    /// </summary>
    class MusicProcess
    {
        public Process FFMpeg { get; set; } = null;

        public MusicProcess(string file_path, ProcessStartMode start_mode)
        {
            this.Start(file_path, start_mode);
        }

        /// <summary>
        /// Starts FFMPEG process in either input or output mode
        /// </summary>
        /// <param name="file_path">File path to reproduce</param>
        /// <param name="start_mode">Start mode (Input/Output)</param>
        public void Start(string file_path, ProcessStartMode start_mode)
        {
            this.FFMpeg = Process.Start(new ProcessStartInfo
            {
                FileName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + @"\ffmpeg.exe",
                Arguments = $@"-hide_banner -loglevel panic -i ""{file_path}"" -ac 2 -f s16le -ar 48000 pipe:1",
                RedirectStandardOutput = start_mode == ProcessStartMode.OUTPUT,
                RedirectStandardInput = start_mode == ProcessStartMode.INPUT,
                UseShellExecute = false
            });
        }

        /// <summary>
        /// Kills the current running FFMPEG process
        /// </summary>
        /// <param name="mode">Mode used to start the process (Input/Output)</param>
        /// <returns></returns>
        public async Task Kill(ProcessStartMode mode)
        {
            if (this.FFMpeg == null)
                return;

            Stream processStream = mode == 
                ProcessStartMode.OUTPUT ? this.FFMpeg.StandardOutput.BaseStream 
                : this.FFMpeg.StandardInput.BaseStream;

            await processStream.FlushAsync();
            processStream.Dispose();

            this.FFMpeg.WaitForExit();
            this.FFMpeg.Close(); 
        }
    }
}
