using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Distance.Shark
{
    public static class Field
    {
        public static class Frame
        {
            public const string Number = "frame.number";
            public const string Protocols = "frame.protocols";
        }
    }
    /// <summary>
    /// Represents a wrapper around TSHARK tool. 
    /// </summary>
    public abstract class TSharkProcess<TDecodedRecord>
    {
        static TSharkProcess()
        {
            switch (System.Environment.OSVersion.Platform)
            {
                case PlatformID.Unix:
                    m_tsharkPath = @"/usr/local/bin/tshark";
                    break;
                case PlatformID.Win32NT:
                    m_tsharkPath = @"C:\Program Files\Wireshark\tshark.exe";
                    break;
            }
        }

        private bool m_exportObjects;
        private string m_exportedObjectsPath;
        private string m_pipeName = "tshark";
        private string m_cmdline;
        private static string m_tsharkPath;

        private Process m_tsharkProcess;
        /// <summary>
        /// Creates a new TSHARK process runner.
        /// </summary>
        public TSharkProcess()
        {

        }

        public TSharkProcess(string pipeName): this()
        {
            m_pipeName = pipeName;
        }

        /// <summary>
        /// Gets or sets the name of pipe from which TSHARK will read frames.
        /// </summary>
        public string PipeName
        {
            get => m_pipeName;
            set
            {
                if (this.IsRunning) throw new InvalidOperationException("Cannot set PipeName when the process is running.");
                m_pipeName = value;
            }
        }

        protected abstract string GetOutputFilter();

        /// <summary>
        /// Executes TSHARK process. 
        /// </summary>
        /// <returns>true; if no errors occured or false on errors.</returns>
        public bool Start()
        {
            try
            {
                var process = new Process();
                var pipeName = $@"\\.\pipe\{m_pipeName}";
                
                var exportObjectArgument = m_exportObjects ? $"--export-objects \"http,{m_exportedObjectsPath}\"" : "";
               

                process.StartInfo.FileName = m_tsharkPath;
                process.StartInfo.Arguments = $"-i {pipeName} -T ek {GetOutputFilter()} {exportObjectArgument}";

                process.OutputDataReceived += Process_OutputDataReceived;
                process.ErrorDataReceived += Process_ErrorDataReceived;
                process.Exited += Process_Exited;

                process.StartInfo.RedirectStandardOutput = true;
                process.StartInfo.RedirectStandardError = true;
                process.StartInfo.UseShellExecute = false;

                m_cmdline = $"{process.StartInfo.FileName} {process.StartInfo.Arguments}";
                
                process.Start();
                process.BeginOutputReadLine();
                process.BeginErrorReadLine();
                m_tsharkProcess = process;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            // -T ek is newline delimited JSON format.
            // {"index" : {"_index": "packets-2017-06-27", "_type": "pcap_file", "_score": null}}
            // { "timestamp" : "1452112930292", "layers" : { "frame_number": ["28"],"frame_protocols": ["eth:ethertype:ip:tcp"]
            if (e.Data != null)
            {
                if (e.Data.StartsWith("{\"timestamp\""))
                {
                    var result = GetResult(e.Data);
                    OnPacketDecoded(result);
                }
            }
        }

        string m_errorOutput;
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            m_errorOutput += e.Data + Environment.NewLine;
        }

        private void Process_Exited(object sender, EventArgs e)
        {
            Console.Error.WriteLine($"{m_cmdline} exited!");
        }

        /// <summary>
        /// Gets the <see cref="Task"/> that finishes when the process completes.
        /// Use <see cref="Task.WaitAll(Task[])"/> for waiting to completion.
        /// Note that this must be called after <see cref="TSharkProcess.Start"/> method otherwise it returns finished task.
        /// </summary>
        public Task Completion => Task.Run(() => m_tsharkProcess?.WaitForExit());

        public bool IsRunning => !(m_tsharkProcess?.HasExited ?? true);

        /// <summary>
        /// Sets or gets the flag that determines whether to export objects.
        /// </summary>
        public bool ExportObjects { get => m_exportObjects; set => m_exportObjects = value; }

        /// <summary>
        /// Gets or sets the path where to export objects.
        /// </summary>
        public string ExportedObjectsPath { get => m_exportedObjectsPath; set => m_exportedObjectsPath = value; }

        /// <summary>
        /// Gets the command line that representing the currently running TSHark process.
        /// </summary>
        public string Cmdline { get => m_cmdline; }
        public string TsharkExefile { get => m_tsharkPath; set => m_tsharkPath = value; }
        public string ErrorOutput { get => m_errorOutput; }

        /// <summary>
        /// Releases resources associated with the process when the process finishes. 
        /// Calling this method when process is still running causes blocking the caller 
        /// until the process finishes.
        /// </summary>
        public void Close()
        {
            if (m_tsharkProcess != null)
            {
                m_tsharkProcess.WaitForExit();
                m_tsharkProcess.Close();
            }
        }
        /// <summary>
        /// Immediately stops the associated TSHARK process.
        /// </summary>
        public void Kill()
        {
            if (m_tsharkProcess != null)
            {
                m_tsharkProcess.Kill();
                m_tsharkProcess.WaitForExit();
            }
        }

        /// <summary>
        /// This event is called when the packet was decoded.
        /// </summary>
        public event EventHandler<TDecodedRecord> PacketDecoded;
        private void OnPacketDecoded(TDecodedRecord packetFields)
        {
            PacketDecoded?.Invoke(this, packetFields);
        }

        /// <summary>
        /// Gets the <see cref="TDecodedRecord"/> object for the result line generated by the TSHARK process.
        /// </summary>
        /// <param name="line">Result line generated by the associated TSHARK process.</param>
        /// <returns><see cref="TDecodedRecord"/> object for the result line generated by the TSHARK process.</returns>
        protected abstract TDecodedRecord GetResult(string line);
    }
}