using SharpPcap;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Distance.Shark
{
    public static class TSharkDecoder
    {
        /// <summary>
        /// Decodes each <see cref="RawCapture"/> of a sequence into a <typeparamref name="TRecord"/> object.
        /// </summary>
        /// <typeparam name="TRecord"></typeparam>
        /// <param name="frames">A sequence of values to invoke a transform function on.</param>
        /// <param name="tsharkProcess">A decoder process to apply to each element.</param>
        /// <param name="datalinkType">The link layer type used in decoding operation. Default is <see cref="DataLinkType.Ethernet"/>.</param>
        /// <returns>
        /// An IEnumerable<PacketFields> whose elements are the result of invoking the decode function on each element of source.
        /// </returns>
        /// <remarks>
        /// This method is implemented by using deferred execution. The immediate return value is an object that stores all the information 
        /// that is required to perform the action. The query represented by this method is not executed until the object is enumerated 
        /// either by calling its GetEnumerator method directly or by using foreach.
        /// </remarks>
        public static IEnumerable<TRecord> Decode<TRecord>(this IEnumerable<RawCapture> frames, TSharkProcess<TRecord> tsharkProcess, DataLinkType datalinkType = DataLinkType.Ethernet)
        {
            var pipename = $"ndx.tshark_{new Random().Next(Int32.MaxValue)}";
            var wsender = new TSharkSender(pipename, datalinkType);
            tsharkProcess.PipeName = pipename;

            var decodedPackets = new BlockingCollection<TRecord>();
            void PacketDecoded(object sender, TRecord packet)
            {
                decodedPackets.Add(packet);
            }
            tsharkProcess.PacketDecoded += PacketDecoded;
            tsharkProcess.Start();
            if (!wsender.Connected.Wait(5000))
            {
                throw new InvalidOperationException("Cannot connect to TShark process.");
            }

            var pumpTask = Task.Run(async () =>
            {
                foreach (var frame in frames)
                {
                    await wsender.SendAsync(frame);
                }
                wsender.Close();
            });

            while (tsharkProcess.IsRunning || decodedPackets.Count > 0)
            {
                yield return decodedPackets.Take();
            }
        }
    }
}
