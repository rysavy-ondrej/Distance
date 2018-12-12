namespace Distance.Shark
{
    public class TSharkPacketDecoderProcess : TSharkProcess<Packet>
    {
        PacketDecoder m_decoder;
        public TSharkPacketDecoderProcess(string pipename, PacketDecoder decoder) : base(pipename)
        {
            m_decoder = decoder;
        }
        protected override string GetOutputFilter()
        {
            return "";
        }

        protected override Packet GetResult(string line)
        {
            var packet = m_decoder.Decode(line);
            return packet;
        }
    }
}
