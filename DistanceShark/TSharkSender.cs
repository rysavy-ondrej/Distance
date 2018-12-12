/**************************************************************************
*                           MIT License
* 
* Copyright (C) 2015 Frederic Chaxel <fchaxel@free.fr>
*
* Permission is hereby granted, free of charge, to any person obtaining
* a copy of this software and associated documentation files (the
* "Software"), to deal in the Software without restriction, including
* without limitation the rights to use, copy, modify, merge, publish,
* distribute, sublicense, and/or sell copies of the Software, and to
* permit persons to whom the Software is furnished to do so, subject to
* the following conditions:
*
* The above copyright notice and this permission notice shall be included
* in all copies or substantial portions of the Software.
*
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
* EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
* MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.
* IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY
* CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT,
* TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE
* SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
*
*********************************************************************/
using SharpPcap;
using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
//
// object creation could be done with 
//      var ws=new Wireshark.WiresharkSender("bacnet",DatalinkType.Ethernet);  // pipe name is \\.\pipe\bacnet
//
// data to wireshark could be sent with something like that
//      if (ws.isConnected)
//          ws.SendToWireshark(new byte[]{0x55,0xFF,0,5,6,0,0,4}, 0, 8);
//
// Wireshark can be launch with : Wireshark -ni \\.\pipe\bacnet
//
// ... enjoy
//
namespace Distance.Shark
{
    // Pcap Global Header
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct pcap_hdr_g
    {
        UInt32 magic_number;   /* magic number */
        UInt16 version_major;  /* major version number */
        UInt16 version_minor;  /* minor version number */
        Int32 thiszone;       /* GMT to local correction */
        UInt32 sigfigs;        /* accuracy of timestamps */
        UInt32 snaplen;        /* max length of captured packets, in octets */
        UInt32 network;        /* data link type */

        public pcap_hdr_g(UInt32 snaplen, UInt32 network)
        {
            magic_number = 0xa1b2c3d4;
            version_major = 2;
            version_minor = 4;
            thiszone = 0;
            sigfigs = 0;
            this.snaplen = snaplen;
            this.network = network;
        }

        // struct Marshaling
        // Maybe a 'manual' byte by byte serialization could be required on some systems
        // work well on Win32, Win64 .NET 3.0 to 4.5
        public byte[] ToByteArray()
        {
            int rawsize = Marshal.SizeOf(this);
            byte[] rawdatas = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(this, buffer, false);
            handle.Free();
            return rawdatas;
        }
    }

    // Pcap Packet Header
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    struct pcap_hdr_p
    {
        UInt32 ts_sec;         /* timestamp seconds */
        UInt32 ts_usec;        /* timestamp microseconds */
        UInt32 incl_len;       /* number of octets of packet saved in file */
        UInt32 orig_len;       /* actual length of packet */

        public pcap_hdr_p(UInt32 lenght, UInt32 datetime, UInt32 microsecond)
        {
            incl_len = orig_len = lenght;
            ts_sec = datetime;
            ts_usec = microsecond;
        }

        // struct Marshaling
        // Maybe a 'manual' byte by byte serialise could be required on some system
        public byte[] ToByteArray()
        {
            int rawsize = Marshal.SizeOf(this);
            byte[] rawdatas = new byte[rawsize];
            GCHandle handle = GCHandle.Alloc(rawdatas, GCHandleType.Pinned);
            IntPtr buffer = handle.AddrOfPinnedObject();
            Marshal.StructureToPtr(this, buffer, false);
            handle.Free();
            return rawdatas;
        }
    }

    /// <summary>
    /// The class implements method for sending packets to TShark/Wireshark through named pipe.
    /// </summary>
    /// <remarks>
    /// Object creation could be done with:
    ///      
    /// var ws=new Wireshark.WiresharkSender("bacnet",DataLinkType.Ethernet);  // pipe name is \\.\pipe\bacnet
    ///
    ///  Data to wireshark could be sent with something like that
    ///      if (ws.isConnected)
    ///          ws.SendToWireshark(new byte[]{0x55,0xFF,0,5,6,0,0,4}, 0, 8);
    ///
    /// Wireshark can be launched with: Wireshark -ni \\.\pipe\bacnet
    /// TShark can be launced with: tshark -i \\.\pipe\bacnet
    ///</remarks>
    public class TSharkSender : IDisposable
    {
        private NamedPipeServerStream m_wiresharkPipe;

        private bool m_isConnected = false;
        private Task m_connectionCompleted;
        private string m_pipeName;
        private DataLinkType m_linkType;


        public TSharkSender(string pipeName, DataLinkType linkType)
        {
            this.m_pipeName = pipeName;
            this.m_linkType = linkType;

            m_connectionCompleted = Task.Factory.StartNew(PipeCreate);
        }

        /// <summary>
        /// <see cref="Task"/> that completes when the pipe is connected.
        /// </summary>
        public Task Connected => m_connectionCompleted;

        private void PipeCreate()
        {
            try
            {
                m_wiresharkPipe = new NamedPipeServerStream(m_pipeName, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
                m_wiresharkPipe.WaitForConnection();

                // Wireshark Global Header
                pcap_hdr_g p = new pcap_hdr_g(UInt16.MaxValue, (uint)m_linkType);
                var bytes = p.ToByteArray();
                m_wiresharkPipe.Write(bytes, 0, bytes.Length);
                m_isConnected = true;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e);
            }
        }

        /// <summary>
        /// Gets the information if there is connected listener to the sender's pipe.
        /// </summary>
        public bool IsConnected { get => m_isConnected; }

        private UInt32 DateTimeToUnixTimestamp(DateTime dateTime)
        {
            return (UInt32)(dateTime - new DateTime(1970, 1, 1).ToLocalTime()).TotalSeconds;
        }

        /// <summary>
        /// Sends the frame via the pipe to the connected listener.
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="offset"></param>
        /// <param name="lenght"></param>
        /// <returns></returns>
        public bool Send(byte[] buffer, int offset, int lenght)
        {
            var now = DateTimeOffset.Now;
            return Send(buffer, offset, lenght, new PosixTimeval((ulong)now.ToUnixTimeSeconds(),(ulong)now.Millisecond*1000));
        }

        public async Task<bool> SendAsync(byte[] buffer, int offset, int lenght)
        {
            var now = DateTimeOffset.Now;
            return await SendAsync(buffer, offset, lenght, new PosixTimeval((ulong)now.ToUnixTimeSeconds(), (ulong)now.Millisecond * 1000));
        }

        public bool Send(byte[] buffer, int offset, int lenght, PosixTimeval timeval)
        {
            return Send(buffer, offset, lenght, (uint)timeval.Seconds, (uint)timeval.MicroSeconds);
        }

        public async Task<bool> SendAsync(byte[] buffer, int offset, int lenght, PosixTimeval timeval)
        {
            return await SendAsync(buffer, offset, lenght, (uint)timeval.Seconds, (uint)timeval.MicroSeconds);
        }

        public async Task<bool> SendAsync(RawCapture frame)
        {
            return await SendAsync(frame.Data, 0, frame.Data.Length, frame.Timeval);
        }

        public bool Send(RawCapture frame)
        {
            return Send(frame.Data, 0, frame.Data.Length, frame.Timeval);
        }

        public bool Send(byte[] buffer, int offset, int lenght, UInt32 date_sec, UInt32 date_usec)
        {
            if (m_isConnected == false) return false;

            if (buffer == null) return false;
            if (buffer.Length < (offset + lenght)) return false;

            pcap_hdr_p pHdr = new pcap_hdr_p((UInt32)lenght, date_sec, date_usec);
            byte[] b = pHdr.ToByteArray();

            try
            {
                // Wireshark Header
                m_wiresharkPipe.Write(b, 0, b.Length);
                // Packet content
                m_wiresharkPipe.Write(buffer, offset, lenght);

                m_wiresharkPipe.Flush();
            }
            catch (IOException)
            {
                // broken pipe, try to restart
                m_isConnected = false;
                m_wiresharkPipe.Close();
                m_wiresharkPipe.Dispose();
                m_connectionCompleted = Task.Factory.StartNew(PipeCreate);
                return false;
            }
            catch (Exception)
            {
                // Unknow error, not due to the pipe
                // No need to restart it
                return false;
            }
            return true;
        }

        public async Task<bool> SendAsync(byte[] buffer, int offset, int lenght, UInt32 date_sec, UInt32 date_usec)
        {
            if (m_isConnected == false) return false;

            if (buffer == null) return false;
            if (buffer.Length < (offset + lenght)) return false;

            pcap_hdr_p pHdr = new pcap_hdr_p((UInt32)lenght, date_sec, date_usec);
            byte[] b = pHdr.ToByteArray();

            try
            {
                // Wireshark Header
                await m_wiresharkPipe.WriteAsync(b, 0, b.Length);
                // Packet content
                await m_wiresharkPipe.WriteAsync(buffer, offset, lenght);
            }
            catch (IOException)
            {
                // broken pipe, try to restart
                m_isConnected = false;
                m_wiresharkPipe.Close();
                m_wiresharkPipe.Dispose();
                m_connectionCompleted = Task.Factory.StartNew(PipeCreate);
                return false;
            }
            catch (Exception)
            {
                // Unknow error, not due to the pipe
                // No need to restart it
                return false;
            }
            return true;
        }

        public void Dispose()
        {
            m_wiresharkPipe.Dispose();
        }

        public void Close()
        {
            m_wiresharkPipe.Close();
        }
    }
}