namespace Plus.Communication.Packets.Outgoing.Handshake
{
    class GenericErrorComposer : MessageComposer
    {
        public int ErrorId { get; }

        public GenericErrorComposer(int errorId)
            : base(ServerPacketHeader.GenericErrorMessageComposer)
        {
            ErrorId = errorId;
        }

        public override void Compose(ServerPacket packet)
        {
            packet.WriteInteger(ErrorId);
        }
    }
}
