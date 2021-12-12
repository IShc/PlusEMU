namespace Plus.Communication.Packets.Outgoing.Misc
{
    internal class LatencyTestComposer : MessageComposer
    {
        public int TestResponse { get; }

        public LatencyTestComposer(int testResponce)
            : base(ServerPacketHeader.LatencyResponseMessageComposer)
        {
            TestResponse = testResponce;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(TestResponse);
        }
    }
}