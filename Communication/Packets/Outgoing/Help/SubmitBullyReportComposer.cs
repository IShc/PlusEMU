namespace Plus.Communication.Packets.Outgoing.Help
{
    class SubmitBullyReportComposer : MessageComposer
    {
        public int Result { get; }

        public SubmitBullyReportComposer(int result)
            : base(ServerPacketHeader.SubmitBullyReportMessageComposer)
        {
            Result = result;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(Result);
        }
    }
}
